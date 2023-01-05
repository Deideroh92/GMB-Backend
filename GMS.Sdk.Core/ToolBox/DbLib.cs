using GMS.BusinessProfile.Agent.Model;
using GMS.Sdk.Core.Database;
using System.Data.SqlClient;
using System.Text.Encodings.Web;

namespace GMS.Sdk.Core.ToolBox {
    public class DbLib {
        private const string connectionString = @"Data Source=vasano.database.windows.net;Initial Catalog=GMS;User ID=vs-sa;Password=Eu6pkR2J4";
        private readonly SqlConnection Connection;

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        public DbLib() {
            Connection = new SqlConnection(connectionString);
            ConnectToDB();
        }

        /// <summary>
        /// Connect to DB.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void ConnectToDB() {
            try {
                Connection.Open();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("Couldn't connect to DB");
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Disconnect from DB.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void DisconnectFromDB() {
            try {
                Connection.Close();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("Couldn't disconnect from DB");
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Get Categories by given Activity.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>List of Google categories</returns>
        /// <exception cref="Exception"></exception>
        public List<string> GetCategoriesByActivity(string activity) {
            try {
                string selectCommand = "SELECT VALEUR FROM vCATEGORIES WHERE ACTIVITE = @Activity";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Activity", activity);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                List<string> categories = new();

                while (reader.Read())
                    categories.Add(reader.GetValue(0).ToString());

                return categories;
            } catch (Exception) {
                throw;
            }
        }

        #region Business Url

        /// <summary>
        /// Create Business Url.
        /// </summary>
        /// <param name="businessUrl"></param>
        /// <exception cref="Exception"></exception>
        public void CreateBusinessUrl(DbBusinessUrl businessUrl) {
            try {
                string insertCommand = "INSERT INTO BUSINESS_URL VALUES (@Guid, @Url, @DateInsert, @State, @TextSearch, @DateUpdate, @UrlEncoded)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", businessUrl.Guid.ToString());
                cmd.Parameters.AddWithValue("@Url", businessUrl.Url);
                cmd.Parameters.AddWithValue("@DateInsert", businessUrl.DateInsert);
                cmd.Parameters.AddWithValue("@State", businessUrl.State.ToString());
                cmd.Parameters.AddWithValue("@TextSearch", businessUrl.TextSearch);
                cmd.Parameters.AddWithValue("@DateUpdate", businessUrl.DateUpdate);
                cmd.Parameters.AddWithValue("@UrlEncoded", businessUrl.UrlEncoded);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Check if url exist.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckBusinessUrlExist(string urlEncoded) {
            try {
                string selectCommand = "SELECT 1 FROM BUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                if (reader.Read()) {
                    reader.Close();
                    return true;
                } else {
                    reader.Close();
                    return false;
                }
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by url state.
        /// </summary>
        /// <param name="urlState"></param>
        /// <param name="entries"></param>
        /// <returns>List of Bussiness Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListByUrlState(UrlState urlState, int entries) {
            List<DbBusinessAgent> businessAgentList = new();
            try {
                string selectCommand = "SELECT TOP (@Entries) GUID, URL FROM BUSINESS_URL WHERE STATE = @UrlState";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@UrlState", urlState.ToString());
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent business = new(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessAgentList.Add(business);
                }
                return businessAgentList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by url list (Networks only).
        /// </summary>
        /// <param name="urlList"></param>
        /// <returns>List of Business Agentl</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentNetworkListByUrlList(List<string> urlList) {
            List<DbBusinessAgent> businessAgentList = new();
            string urlEncoded;

            foreach (string url in urlList) {
                try {
                    urlEncoded = ToolBox.ComputeMd5Hash(url);
                    string selectCommand = "SELECT GUID, URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU WHERE URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    cmd.Dispose();

                    if (reader.Read()) {
                        DbBusinessAgent business = new(reader.GetValue(0).ToString(), reader.GetValue(1).ToString(), reader.GetValue(2).ToString());
                        businessAgentList.Add(business);
                    }  
                } catch (Exception) {
                    throw;
                }
            }
            return businessAgentList;
        }

        /// <summary>
        /// Get Business Agent list by url list (Excluding networks).
        /// </summary>
        /// <param name="urlList"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentIndependantListByUrlList(List<string> urlList) {
            List<DbBusinessAgent> businessAgentList = new();
            string urlEncoded;

            foreach (string url in urlList) {
                try {
                    urlEncoded = ToolBox.ComputeMd5Hash(url);
                    string selectCommand = "SELECT GUID, URL, ID_ETAB FROM vBUSINESS_PROFILE_HORS_RESEAU WHERE URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    cmd.Dispose();

                    if (reader.Read()) {
                        DbBusinessAgent business = new(reader.GetValue(0).ToString(), reader.GetValue(1).ToString(), reader.GetValue(2).ToString());
                        businessAgentList.Add(business);
                    }
                } catch (Exception) {
                    throw;
                }
            }
            return businessAgentList;
        }

        /// <summary>
        /// Update Business Url state.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="state"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessUrlState(string guid, UrlState state) {
            try {
                string updateCommand = "UPDATE BUSINESS_URL SET STATE = @State, DATE_UPDATE = @DateUpdate WHERE GUID = @Guid";
                using SqlCommand cmd = new(updateCommand, Connection);
                cmd.Parameters.AddWithValue("@State", state.ToString());
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Url Guid by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Guid</returns>
        /// <exception cref="Exception"></exception>
        public string? GetBusinessUrlGuidByUrlEncoded(string urlEncoded) {
            try {
                string selectCommand = "SELECT GUID FROM BUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read())
                    return reader.GetValue(0).ToString();

                return null;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Delete Business Url by Guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <exception cref="Exception"></exception>
        public void DeleteBusinessUrlByGuid(string guid) {
            try {
                string deleteCommand = "DELETE FROM BUSINESS_URL WHERE GUID = @Guid";
                using SqlCommand cmd = new(deleteCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }
        #endregion

        #region Business Profile

        /// <summary>
        /// Create new Business Profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <exception cref="Exception"></exception>
        public void CreateBusinessProfile(DbBusinessProfile businessProfile) {
            try {
                string insertCommand = "INSERT INTO BUSINESS_PROFILE VALUES (@IdEtab, @FirstGuid, @Name, @Category, @Adress, @Tel, @Website, @Geoloc, @DateInsert, @UpdateCount, @DateUpdate, @Status, @Processing)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@FirstGuid", businessProfile.FirstGuid);
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@Category", businessProfile.Category as object ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Adress", businessProfile.Adress as object  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tel", businessProfile.Tel as object  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Website", businessProfile.Website as object  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Geoloc", businessProfile.Geoloc as object  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateInsert", businessProfile.DateInsert);
                cmd.Parameters.AddWithValue("@UpdateCount", 0);
                cmd.Parameters.AddWithValue("@DateUpdate", businessProfile.DateUpdate);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@Processing", businessProfile.Processing);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by Category (Networks only).
        /// </summary>
        /// <param name="category"></param>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListNetworkByCategory(string category, int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE CATEGORY = @Category";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by Activity (Networks only).
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListNetworkByActivity(string activity, int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE ACTIVITE = @Activity";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Activity", activity);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by Sector (Networks only).
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListNetworkBySector(string sector, int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE SECTEUR = @Sector";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Sector", sector);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by Univers (Networks only).
        /// </summary>
        /// <param name="univers"></param>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListNetworkByUnivers(string univers, int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE UNIVERS = @Univers";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Univers", univers);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by Brand (Networks only).
        /// </summary>
        /// <param name="brand"></param>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListNetworkByBrand(string brand, int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE MARQUE = @Brand";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Brand", brand);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list (Networks only).
        /// </summary>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentListNetwork(int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand = "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" + "WHERE PROCESSING = 1" + "ORDER BY ID_ETAB ASC";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Business Agent or Null if not found</returns>
        /// <exception cref="Exception"></exception>
        public DbBusinessAgent? GetBusinessAgentByUrlEncoded(string urlEncoded) {
            try {
                string selectCommand =
                    "SELECT BP.ID_ETAB, BU.GUID, BU.URL FROM BUSINESS_PROFILE as BP" +
                    " JOIN BUSINESS_URL as BU ON BP.FIRST_GUID = BU.GUID" +
                    " WHERE BU.URL_MD5 = @UrlEncoded";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(reader.GetValue(1).ToString(), reader.GetValue(2).ToString(), reader.GetValue(0).ToString());
                    return businessProfile;
                }

                return null;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Check if business profile exist.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckBusinessProfileExist(string idEtab) {
            try {
                string selectCommand = "SELECT 1 FROM BUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                if (reader.Read()) {
                    reader.Close();
                    return true;
                } else {
                    reader.Close();
                    return false;
                }
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Update Business Profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessProfile(DbBusinessProfile businessProfile) {
            try {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET NAME = @Name, ADRESS = @Adress, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name as object ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Adress", businessProfile.Adress as object ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", businessProfile.Category as object ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tel", businessProfile.Tel as object ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Website", businessProfile.Website as object ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateUpdate", businessProfile.DateUpdate);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Update Business Profile state.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="processing"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessProfileProcessingState(string idEtab, bool processing) {
            try {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET PROCESSING = @Processing, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Update Business Profile state.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="businessStatus"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessProfileStatus(string idEtab, BusinessStatus businessStatus) {
            try {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET STATUS = @Status, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Status", businessStatus.ToString());
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        #endregion

        #region Business Score
        /// <summary>
        /// Create Business Score.
        /// </summary>
        /// <param name="businessScore"></param>
        /// <exception cref="Exception"></exception>
        public void CreateBusinessScore(DbBusinessScore businessScore) {
            try {
                string insertCommand = "INSERT INTO BUSINESS_SCORE VALUES (@IdEtab, @Score, @NbReviews, @DateInsert)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessScore.IdEtab);
                cmd.Parameters.AddWithValue("@Score", businessScore.Score);
                cmd.Parameters.AddWithValue("@NbReviews", businessScore.NbReviews);
                cmd.Parameters.AddWithValue("@DateInsert", businessScore.DateInsert);
                cmd.ExecuteNonQuery();
            } catch (Exception) {
                throw;
            }
        }
        #endregion

        #region Business Review

        /// <summary>
        /// Create Business Review.
        /// </summary>
        /// <param name="businessReview"></param>
        /// <exception cref="Exception"></exception>
        public void CreateBusinessReview(DbBusinessReview businessReview) {
            try {
                string insertCommand = "INSERT INTO BUSINESS_REVIEWS VALUES (@IdEtab, @IdReview, @UserName, @UserStatus, @Score, @UserNbReviews, @Review, @ReviewGoogleDate, @ReviewDate, @ReviewReplied, @DateInsert, @DateUpdate)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessReview.IdEtab);
                cmd.Parameters.AddWithValue("@IdReview", businessReview.IdReview);
                cmd.Parameters.AddWithValue("@UserName", businessReview.User.Name);
                cmd.Parameters.AddWithValue("@UserStatus", businessReview.User.LocalGuide);
                cmd.Parameters.AddWithValue("@Score", businessReview.Score);
                cmd.Parameters.AddWithValue("@UserNbReviews", businessReview.User.NbReviews);
                cmd.Parameters.AddWithValue("@Review", businessReview.ReviewText);
                cmd.Parameters.AddWithValue("@ReviewGoogleDate", businessReview.ReviewGoogleDate);
                cmd.Parameters.AddWithValue("@ReviewDate", businessReview.ReviewDate);
                cmd.Parameters.AddWithValue("@ReviewReplied", businessReview.ReviewReplied);
                cmd.Parameters.AddWithValue("@DateInsert", businessReview.DateInsert);
                cmd.Parameters.AddWithValue("@DateUpdate", businessReview.DateUpdate);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Check if a review exist on a Business Profile.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="idReview"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckBusinessReviewExist(string idEtab, string idReview) {
            try {
                string selectCommand = "SELECT 1 FROM BUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                if (reader.Read())
                    return true;
                else
                    return false;
            } catch (Exception) {
                throw;
            }
        }
        #endregion
    }
    #endregion
}