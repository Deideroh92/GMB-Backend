using GMS.BusinessProfile.Agent.Model;
using GMS.Sdk.Core.Database;
using System.Data.SqlClient;

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
                throw new Exception("Connection to DB failed!");
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
                throw new Exception("Disconnection from DB failed!");
            }
        }

        #region Business Url

        /// <summary>
        /// Insert a Business Url into DB.
        /// </summary>
        /// <param name="businessUrl"></param>
        /// <exception cref="Exception"></exception>
        public void InsertBusinessUrl(DbBusinessUrl businessUrl) {
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
                throw new Exception("Couldn't insert business url: " + businessUrl.Url + " in DB");
            }
        }

        /// <summary>
        /// Check if url exist in DB.
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
                throw new Exception("Failed searching url encoded: " + urlEncoded + " in DB");
            }
        }

        /// <summary>
        /// Select Business Url (Guid & Url) from DB (used in case of a search by url state).
        /// </summary>
        /// <param name="urlState"></param>
        /// <param name="entries"></param>
        /// <returns>List of Guid & Url</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessList(UrlState urlState, int entries) {
            List<DbBusinessAgent> businessUrlList = new();
            try {
                string selectCommand = "SELECT TOP (@Entries) ID, GUID, URL FROM BUSINESS_URL WHERE STATE = @UrlState";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@UrlState", urlState.ToString());
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessUrl = new((long)reader.GetValue(0), reader.GetValue(1).ToString(), reader.GetValue(2).ToString());
                    businessUrlList.Add(businessUrl);
                }

                return businessUrlList;
            } catch (Exception) {
                throw new Exception("Failed selecting " + entries.ToString() + " business url for profile agent with state: " + urlState + " from DB");
            }
        }

        /// <summary>
        /// Select Business Url (Guid & Url) from DB (used in case of a search by url state).
        /// </summary>
        /// <param name="urlState"></param>
        /// <param name="entries"></param>
        /// <returns>List of Guid & Url</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessList(List<string> urlList) {
            List<DbBusinessAgent> businessUrlList = new();
            string urlEncoded;

            foreach (string url in urlList) {
                try {
                    urlEncoded = ToolBox.ComputeMd5Hash(url);
                    string selectCommand = "SELECT BU.ID, BU.GUID, BU.URL, BP.ID_ETAB FROM BUSINESS_URL as BU JOIN BUSINESS_PROFILE as BP on BU.GUID = BP.FIRST_GUID WHERE BU.URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    cmd.Dispose();

                    if (reader.Read()) {
                        DbBusinessAgent businessUrl = new((long)reader.GetValue(0), reader.GetValue(1).ToString(), reader.GetValue(2).ToString(), reader.GetValue(3).ToString());
                        businessUrlList.Add(businessUrl);
                    }  
                } catch (Exception e) {
                    throw new Exception("Failed selecting urls encoded from DB");
                }
            }
            return businessUrlList;
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
                throw new Exception("Failed updating business url state with guid: " + guid + " and state: " + state.ToString() + " from DB");
            }
        }

        /// <summary>
        /// Get Guid by Url Encoded
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Guid</returns>
        /// <exception cref="Exception"></exception>
        public string? SelectGuidByUrlEncoded(string urlEncoded) {
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
                throw new Exception("Failed getting guid from url encoded :  " + urlEncoded);
            }
        }

        public void DeleteUrlByGuid(string guid) {
            try {
                string deleteCommand = "DELETE FROM BUSINESS_URL WHERE GUID = @Guid";
                using SqlCommand cmd = new(deleteCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw new Exception("Couldn't delete business url with guid: " + guid);
            }
        }
        #endregion

        #region Business Profile

        /// <summary>
        /// Insert a Business Profile into DB.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <exception cref="Exception"></exception>
        public void InsertBusinessProfile(DbBusinessProfile businessProfile) {
            try {
                string insertCommand = "INSERT INTO BUSINESS_PROFILE VALUES (@IdEtab, @FirstGuid, @Name, @Category, @Adress, @Tel, @Website, @Geoloc, @DateInsert, @UpdateCount, @DateUpdate, @Status, @Processing)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@FirstGuid", businessProfile.FirstGuid);
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@Category", (object)businessProfile.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Adress", (object)businessProfile.Adress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tel", (object)businessProfile.Tel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Website", (object)businessProfile.Website ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Geoloc", (object)businessProfile.Geoloc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateInsert", businessProfile.DateInsert);
                cmd.Parameters.AddWithValue("@UpdateCount", 0);
                cmd.Parameters.AddWithValue("@DateUpdate", businessProfile.DateUpdate);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@Processing", businessProfile.Processing);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw new Exception("Couldn't insert business profile with guid: " + businessProfile.FirstGuid + " and with IdEtab: " + businessProfile.IdEtab + " in DB");
            }
        }

        /// <summary>
        /// Select Business Profile (Guid & Url & IdEtab) from DB.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="entries"></param>
        /// <returns>List of Guid & Url & IdEtab</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessList(string category, int entries) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand =
                    "SELECT TOP (@Entries) BP.ID, BP.ID_ETAB, BU.GUID, BU.URL FROM BUSINESS_PROFILE as BP" +
                    " JOIN BUSINESS_URL as BU ON BP.FIRST_GUID = BU.GUID" +
                    " JOIN CATEGORIES as CAT on BP.CATEGORY = CAT.CATEGORY" +
                    " WHERE CAT.SECTOR = @Category AND BP.PROCESSING = 0";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new((long)reader.GetValue(0), reader.GetValue(2).ToString(), reader.GetValue(3).ToString(), reader.GetValue(1).ToString());
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw new Exception("Failed fetching " + entries.ToString() +  " business url with category: " + category + " from DB");
            }
        }

        /// <summary>
        /// Select Business Profile by Url Encoded.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="entries"></param>
        /// <returns>List of Guid & Url & IdEtab</returns>
        /// <exception cref="Exception"></exception>
        public DbBusinessAgent? SelectBusinessByUrlEncoded(string urlEncoded) {
            try {
                string selectCommand =
                    "SELECT BP.ID, BP.ID_ETAB, BU.GUID, BU.URL FROM BUSINESS_PROFILE as BP" +
                    " JOIN BUSINESS_URL as BU ON BP.FIRST_GUID = BU.GUID" +
                    " WHERE BU.URL_MD5 = @UrlEncoded";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new((long)reader.GetValue(0), reader.GetValue(2).ToString(), reader.GetValue(3).ToString(), reader.GetValue(1).ToString());
                    return businessProfile;
                }

                return null;
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw new Exception("Failed fetching business with url encoded : " + urlEncoded + " from DB");
            }
        }

        /// <summary>
        /// Check if business profile exist in DB.
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
                throw new Exception("Failed checking if business profile exist for IdEtab: " + idEtab + " in DB");
            }
        }

        /// <summary>
        /// Update a Business Profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessProfile(DbBusinessProfile businessProfile) {
            try {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET NAME = @Name, ADRESS = @Adress, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Name", (object)businessProfile.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Adress", (object)businessProfile.Adress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", (object)businessProfile.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tel", (object)businessProfile.Tel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Website", (object)businessProfile.Website ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateUpdate", businessProfile.DateUpdate);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw new Exception("Failed updating business profile information for IdEtab: " + businessProfile.IdEtab + " in DB");
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
                throw new Exception("Failed updating business profile state for IdEtab: " + idEtab + " in DB");
            }
        }

        /// <summary>
        /// Update Business Profile state.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="processing"></param>
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
                throw new Exception("Failed updating business profile state for IdEtab: " + idEtab + " in DB");
            }
        }

        public int CountBusinessProfileByCategory(string category) {
            try {
                string selectCommand =
                    "SELECT COUNT(*) FROM BUSINESS_PROFILE as BP" +
                    " JOIN CATEGORIES as CAT on BP.CATEGORY = CAT.CATEGORY" +
                    " WHERE CAT.SECTOR = @Category";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Category", category);
                using SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();

                if (reader.Read()) {
                    return (int)reader.GetValue(0);
                } else
                    return 0;

            } catch (Exception) {
                throw new Exception("Failed counting businesses with category : " + category + " from DB");
            }
        }

        #endregion

        #region Business Score
        /// <summary>
        /// Insert a Business Score into DB.
        /// </summary>
        /// <param name="businessScore"></param>
        /// <exception cref="Exception"></exception>
        public void InsertBusinessScore(DbBusinessScore businessScore) {
            try {
                string insertCommand = "INSERT INTO BUSINESS_SCORE VALUES (@IdEtab, @Score, @NbReviews, @DateInsert)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessScore.IdEtab);
                cmd.Parameters.AddWithValue("@Score", businessScore.Score);
                cmd.Parameters.AddWithValue("@NbReviews", businessScore.NbReviews);
                cmd.Parameters.AddWithValue("@DateInsert", businessScore.DateInsert);
                cmd.ExecuteNonQuery();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw new Exception("Failed inserting business score (" + businessScore.Score + "/" + businessScore.NbReviews + ") for IdEtab: " + businessScore.IdEtab + " in DB");
            }
        }
        #endregion

        #region Business Review

        /// <summary>
        /// Insert a Business Review into DB.
        /// </summary>
        /// <param name="businessReview"></param>
        /// <exception cref="Exception"></exception>
        public void InsertBusinessReview(DbBusinessReview businessReview) {
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
                throw new Exception("Failed inserting business review with id: " + businessReview.IdReview + " on business profile with IdEtab: " + businessReview.IdEtab + " in DB");
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
                throw new Exception("Failed checking if business review with id: " + idReview + " exist on business profile with IdEtab: " + idEtab + " in DB");
            }
        }
        #endregion
    }
    #endregion
}