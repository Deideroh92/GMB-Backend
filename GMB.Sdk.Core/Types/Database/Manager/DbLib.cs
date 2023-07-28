﻿using System.Data.SqlClient;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Database.Manager
{
    public class DbLib : IDisposable
    {
        private const string connectionString = @"Data Source=vasano.database.windows.net;Initial Catalog=GMS;User ID=vs-sa;Password=Eu6pkR2J4";
        private readonly SqlConnection Connection;

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        public DbLib()
        {
            Connection = new SqlConnection(connectionString);
            ConnectToDB();
        }

        /// <summary>
        /// Connect to DB.
        /// </summary>
        public void ConnectToDB()
        {
            try
            {
                Connection.Open();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't connect to DB");
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Disconnect from DB.
        /// </summary>
        public void DisconnectFromDB()
        {
            try
            {
                Connection.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't disconnect from DB");
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Check if value is null or not.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Value or DBNull.Value</returns>
        private static object GetValueOrDefault(object? value)
        {
            return value ?? DBNull.Value;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection?.Close();
                Connection?.Dispose();
            }
        }
        ~DbLib()
        {
            Dispose(false);
        }

        /// <summary>
        /// Get Categories by given Activity.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>List of Google categories</returns>
        public List<string> GetCategoriesByActivity(string activity)
        {
            try
            {
                string selectCommand = "SELECT VALEUR FROM vCATEGORIES WHERE ACTIVITE = @Activity";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Activity", activity);
                using SqlDataReader reader = cmd.ExecuteReader();


                List<string> categories = new();

                while (reader.Read())
                    categories.Add(reader.GetString(0));

                return categories;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Business Url

        #region Creation
        /// <summary>
        /// Create Business Url.
        /// </summary>
        /// <param name="businessUrl"></param>
        public void CreateBusinessUrl(DbBusinessUrl businessUrl)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_URL (GUID, URL, STATE, TEXT_SEARCH, URL_MD5) VALUES (@Guid, @Url, @State, @TextSearch, @UrlEncoded)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", businessUrl.Guid);
                cmd.Parameters.AddWithValue("@Url", businessUrl.Url);
                cmd.Parameters.AddWithValue("@State", businessUrl.State.ToString());
                cmd.Parameters.AddWithValue("@TextSearch", GetValueOrDefault(businessUrl.TextSearch));
                cmd.Parameters.AddWithValue("@UrlEncoded", businessUrl.UrlEncoded);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("Error while creating BU", e);
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Get Business Agent list by url state.
        /// </summary>
        /// <param name="urlState"></param>
        /// <param name="entries"></param>
        /// <returns>List of Bussiness Agent</returns>
        public List<BusinessAgent> GetBusinessAgentListByUrlState(UrlState urlState, int? entries)
        {
            List<BusinessAgent> businessAgentList = new();
            try
            {
                string table = "";
                table = urlState switch {
                    UrlState.NEW => "vBUSINESS_URL_NEW",
                    UrlState.UPDATED => "vBUSINESS_URL_UPDATED",
                    UrlState.NO_CATEGORY => "vBUSINESS_URL_NO_CATEGORY",
                    UrlState.DELETED => "vBUSINESS_URL_DELETED",
                    UrlState.PROCESSING => "vBUSINESS_URL_PROCESSING",
                    _ => "vBUSINESS_URL",
                };
                string selectCommand = entries == null ? ("SELECT GUID, URL FROM " + table + " WHERE STATE = @UrlState") : ("SELECT TOP (@Entries) GUID, URL FROM " + table + " WHERE STATE = @UrlState");
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@UrlState", urlState.ToString());
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BusinessAgent business = new(reader.GetString(0), reader.GetString(1));
                    businessAgentList.Add(business);
                }
                return businessAgentList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BA list with state = [{urlState}] and entries = [{entries}]", e);
            }
        }
        /// <summary>
        /// Get Business Agent list by url list.
        /// </summary>
        /// <param name="urlList"></param>
        /// <param name="isNetwork"></param>
        /// <param name="isIndependant"></param>
        /// <returns>List of Business Agent</returns>
        public List<BusinessAgent> GetBusinessAgentNetworkListByUrlList(List<string> urlList, bool isNetwork, bool isIndependant)
        {
            List<BusinessAgent> businessAgentList = new();
            string urlEncoded;
            string table = "vBUSINESS_PROFILE";

            if (isNetwork)
                table = "vBUSINESS_PROFILE_RESEAU";
            if (isIndependant)
                table = "vBUSINESS_PROFILE_HORS_RESEAU";

            foreach (string url in urlList)
            {
                try
                {
                    urlEncoded = ToolBox.ComputeMd5Hash(url);
                    string selectCommand = "SELECT GUID, URL, ID_ETAB FROM " + table + " WHERE URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {

                        BusinessAgent business = new(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                        businessAgentList.Add(business);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error getting BA list (network) by url list", e);
                }
            }
            return businessAgentList;
        }
        /// <summary>
        /// Get Business Url Guid by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Guid</returns>
        public string? GetBusinessUrlGuidByUrlEncoded(string urlEncoded)
        {
            try
            {
                string selectCommand = "SELECT GUID FROM vBUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return reader.GetString(0);

                return null;
            }
            catch (Exception e)
            {
                throw new Exception($"Error getting BU guid with url encoded = [{urlEncoded}]", e);
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update Business Url state.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="state"></param>
        public void UpdateBusinessUrlState(string guid, UrlState state)
        {
            try
            {
                string updateCommand = "UPDATE BUSINESS_URL SET STATE = @State, DATE_UPDATE = @DateUpdate WHERE GUID = @Guid";
                using SqlCommand cmd = new(updateCommand, Connection);
                cmd.Parameters.AddWithValue("@State", state.ToString());
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(DateTime.UtcNow));
                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error updating BP with guid = [{guid}] and state = [{state}]", e);
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete Business Url by Guid.
        /// </summary>
        /// <param name="guid"></param>
        public void DeleteBusinessUrlByGuid(string guid)
        {
            try
            {
                string deleteCommand = "DELETE FROM BUSINESS_URL WHERE GUID = @Guid";
                using SqlCommand cmd = new(deleteCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error deleting BU with guid = [{guid}]", e);
            }
        }
        #endregion

        #region Other
        /// <summary>
        /// Check if url exist by url encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        public bool CheckBusinessUrlExist(string urlEncoded)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM vBUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read();
            }
            catch (Exception e)
            {
                throw new Exception($"Error checking if BU exists with url encoded = [{urlEncoded}]", e);
            }
        }
        #endregion

        #endregion

        #region Business Profile

        #region Creation
        /// <summary>
        /// Create new Business Profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        public void CreateBusinessProfile(DbBusinessProfile businessProfile)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_PROFILE (ID_ETAB, FIRST_GUID, NAME, CATEGORY, ADRESS, TEL, WEBSITE, STATUS, PROCESSING, URL_PICTURE, A_ADDRESS, A_POSTCODE, A_CITY, A_CITY_CODE, A_LAT, A_LON, A_BAN_ID, A_ADDRESS_TYPE, A_NUMBER) VALUES (@IdEtab, @FirstGuid, @Name, @Category, @GoogleAddress, @Tel, @Website, @Status, @Processing, @UrlPicture, @Address, @PostCode, @City, @CityCode, @Lat, @Lon, @IdBan, @AddressType, @StreetNumber)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@FirstGuid", businessProfile.FirstGuid);
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(businessProfile.Category));
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(businessProfile.Tel));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(businessProfile.Website));
                cmd.Parameters.AddWithValue("@UrlPicture", GetValueOrDefault(businessProfile.PictureUrl));
                cmd.Parameters.AddWithValue("@Processing", 0);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@Address", GetValueOrDefault(businessProfile.Address));
                cmd.Parameters.AddWithValue("@PostCode", GetValueOrDefault(businessProfile.PostCode));
                cmd.Parameters.AddWithValue("@City", GetValueOrDefault(businessProfile.City));
                cmd.Parameters.AddWithValue("@CityCode", GetValueOrDefault(businessProfile.CityCode));
                cmd.Parameters.AddWithValue("@Lat", GetValueOrDefault(businessProfile.Lat));
                cmd.Parameters.AddWithValue("@Lon", GetValueOrDefault(businessProfile.Lon));
                cmd.Parameters.AddWithValue("@IdBan", GetValueOrDefault(businessProfile.IdBan));
                cmd.Parameters.AddWithValue("@AddressType", GetValueOrDefault(businessProfile.AddressType));
                cmd.Parameters.AddWithValue("@StreetNumber", GetValueOrDefault(businessProfile.StreetNumber));
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error creating BP with idEtab = [{businessProfile.IdEtab}] and guid = [{businessProfile.FirstGuid}]", e);
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Get Business agent list.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of Business Agent</returns>
        public List<BusinessAgent> GetBusinessAgentList(GetBusinessListRequest request)
        {
            List<BusinessAgent> businessUrlList = new();
            string table = "vBUSINESS_PROFILE";
            string categoryFilter = "";
            string brand = "";

            if (request.IsNetwork) {
                table = "vBUSINESS_PROFILE_RESEAU";
                if (request.Brand != null)
                    brand = " AND MARQUE = '" + request.Brand + "'";
            }
                
            if (request.IsIndependant)
                table = "vBUSINESS_PROFILE_HORS_RESEAU";

            switch (request.CategoryFamily) {
                case CategoryFamily.UNIVERS:
                    categoryFilter = " AND UNIVERS = '" + request.Category + "'";
                    break;
                case CategoryFamily.SECTEUR:
                    categoryFilter = " AND SECTEUR = '" + request.Category + "'";
                    break;
                case CategoryFamily.ACTIVITE:
                    categoryFilter = " AND ACTIVITE = '" + request.Category + "'";
                    break;
                case CategoryFamily.VALEUR:
                    categoryFilter = " AND CATEGORY = '" + request.Category + "'";
                    break;
                default:
                    break;
            }

            try
            {
                string selectCommand = "SELECT TOP (@Entries) URL, ID_ETAB FROM " + table +
                    " WHERE PROCESSING = @Processing" +
                    brand + 
                    categoryFilter +
                    " AND STATUS != 'CLOSED'" +
                    " ORDER BY ID_ETAB ASC";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", request.Entries);
                cmd.Parameters.AddWithValue("@Processing", request.Processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception e)
            {
                throw new Exception($"Error getting BP agent list", e);
            }
        }
        /// <summary>
        /// Get Business list.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of Business</returns>
        public List<DbBusinessProfile> GetBusinessList(GetBusinessListRequest request) {
            List<DbBusinessProfile> businessList = new();
            string table = "vBUSINESS_PROFILE";
            string categoryFilter = "";
            string brand = "";

            if (request.IsNetwork) {
                table = "vBUSINESS_PROFILE_RESEAU";
                if (request.Brand != null)
                    brand = " AND MARQUE = " + request.Brand;
            }

            if (request.IsIndependant)
                table = "vBUSINESS_PROFILE_HORS_RESEAU";

            switch (request.CategoryFamily) {
                case CategoryFamily.UNIVERS:
                    categoryFilter = " AND UNIVERS = " + request.Category;
                    break;
                case CategoryFamily.SECTEUR:
                    categoryFilter = " AND SECTEUR = " + request.Category;
                    break;
                case CategoryFamily.ACTIVITE:
                    categoryFilter = " AND ACTIVITE = " + request.Category;
                    break;
                case CategoryFamily.VALEUR:
                    categoryFilter = " AND CATEGORY = " + request.Category;
                    break;
                default:
                    break;
            }

            try {
                string selectCommand = "SELECT TOP (@Entries) * FROM " + table +
                    " WHERE PROCESSING = @Processing" +
                    brand +
                    categoryFilter +
                    " ORDER BY ID_ETAB ASC";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", request.Entries);
                cmd.Parameters.AddWithValue("@Processing", request.Processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    DbBusinessProfile businessProfile = new(reader["ID_ETAB"].ToString(), reader["FIRST_GUID"].ToString(), reader["NAME"].ToString(),  reader["CATEGORY"].ToString(), reader["ADRESS"].ToString(), null, null, null, null, null, null, null, null, null, reader["TEL"].ToString(), reader["WEBSITE"].ToString(), DateTime.Parse(reader["DATE_UPDATE"].ToString()), (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()), reader["URL_PICTURE"].ToString());
                    businessList.Add(businessProfile);
                }

                return businessList;
            } catch (Exception e) {
                throw new Exception($"Error getting BP list", e);
            }
        }
        /// <summary>
        /// Get Business Agent by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Business Agent or Null if not found</returns>
        public BusinessAgent? GetBusinessByUrlEncoded(string urlEncoded)
        {
            try
            {
                string selectCommand =
                    "SELECT BP.ID_ETAB, BU.GUID, BU.URL FROM BUSINESS_PROFILE as BP" +
                    " JOIN BUSINESS_URL as BU ON BP.FIRST_GUID = BU.GUID" +
                    " WHERE BU.URL_MD5 = @UrlEncoded";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BusinessAgent businessProfile = new(reader.GetString(1), reader.GetString(2), reader.GetString(0));
                    return businessProfile;
                }

                return null;
            }
            catch (Exception e)
            {
                throw new Exception($"Error getting BP by url encoded = [{urlEncoded}]", e);
            }
        }
        /// <summary>
        /// Get Business by Id Etab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business or null</returns>
        public BusinessAgent? GetBusinessByIdEtab(string idEtab) {
            try {
                string selectCommand = "SELECT FIRST_GUID, URL, ID_ETAB FROM vBUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    BusinessAgent businessProfile = new(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                    return businessProfile;
                }

                return null;
            } catch (Exception e) {
                throw new Exception($"Error getting BP by id etab = [{idEtab}]", e);
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update Business profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        public void UpdateBusinessProfile(DbBusinessProfile businessProfile)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET NAME = @Name, ADRESS = @GoogleAddress, A_ADDRESS = @Address, A_POSTCODE = @PostCode, A_CITY = @City, A_CITY_CODE = @CityCode, A_LON = @Lon, A_LAT = @Lat, A_BAN_ID = @IdBan, A_ADDRESS_TYPE = @AddressType, A_NUMBER = @StreetNumber, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status, URL_PICTURE = @UrlPicture WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@Address", GetValueOrDefault(businessProfile.Address));
                cmd.Parameters.AddWithValue("@City", GetValueOrDefault(businessProfile.City));
                cmd.Parameters.AddWithValue("@CityCode", GetValueOrDefault(businessProfile.CityCode));
                cmd.Parameters.AddWithValue("@IdBan", GetValueOrDefault(businessProfile.IdBan));
                cmd.Parameters.AddWithValue("@AddressType", GetValueOrDefault(businessProfile.AddressType));
                cmd.Parameters.AddWithValue("@Lon", GetValueOrDefault(businessProfile.Lon));
                cmd.Parameters.AddWithValue("@Lat", GetValueOrDefault(businessProfile.Lat));
                cmd.Parameters.AddWithValue("@PostCode", GetValueOrDefault(businessProfile.PostCode));
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(businessProfile.Category));
                cmd.Parameters.AddWithValue("@StreetNumber", GetValueOrDefault(businessProfile.StreetNumber));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(businessProfile.Tel));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(businessProfile.Website));
                cmd.Parameters.AddWithValue("@UrlPicture", GetValueOrDefault(businessProfile.PictureUrl));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessProfile.DateUpdate));
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error updating BP with id_etab = [{businessProfile.IdEtab}] and guid = [{businessProfile.FirstGuid}]", e);
            }
        }
        /// <summary>
        /// Update Business profile address.
        /// </summary>
        /// <param name="businessProfile"></param>
        public void UpdateBusinessProfileAddress(DbBusinessProfile businessProfile) {
            try {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET ADRESS = @GoogleAddress, A_ADDRESS = @Address, A_POSTCODE = @PostCode, A_CITY = @City, A_CITY_CODE = @CityCode, A_LON = @Lon, A_LAT = @Lat, A_BAN_ID = @IdBan, A_ADDRESS_TYPE = @AddressType, A_NUMBER = @StreetNumber, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@Address", GetValueOrDefault(businessProfile.Address));
                cmd.Parameters.AddWithValue("@City", GetValueOrDefault(businessProfile.City));
                cmd.Parameters.AddWithValue("@CityCode", GetValueOrDefault(businessProfile.CityCode));
                cmd.Parameters.AddWithValue("@IdBan", GetValueOrDefault(businessProfile.IdBan));
                cmd.Parameters.AddWithValue("@AddressType", GetValueOrDefault(businessProfile.AddressType));
                cmd.Parameters.AddWithValue("@Lon", GetValueOrDefault(businessProfile.Lon));
                cmd.Parameters.AddWithValue("@Lat", GetValueOrDefault(businessProfile.Lat));
                cmd.Parameters.AddWithValue("@PostCode", GetValueOrDefault(businessProfile.PostCode));
                cmd.Parameters.AddWithValue("@StreetNumber", GetValueOrDefault(businessProfile.StreetNumber));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessProfile.DateUpdate));
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.ExecuteNonQuery();
            } catch (Exception e) {
                throw new Exception($"Error updating BP address with id_etab = [{businessProfile.IdEtab}] and guid = [{businessProfile.FirstGuid}]", e);
            }
        }
        /// <summary>
        /// Update Business Profile processing state.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="processing"></param>
        public void UpdateBusinessProfileProcessingState(string idEtab, int processing)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET PROCESSING = @Processing, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(DateTime.UtcNow));
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error updating BP processing state with id etab = [{idEtab}] and processing = [{processing}]", e);
            }
        }
        /// <summary>
        /// Update Business Profile status.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="businessStatus"></param>
        public void UpdateBusinessProfileStatus(string idEtab, BusinessStatus businessStatus)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET STATUS = @Status, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Status", businessStatus.ToString());
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error updating BP status with id etab category = [{idEtab}] and status = [{businessStatus}]", e);
            }
        }
        #endregion

        #region Other
        /// <summary>
        /// Check if business profile exist by idEtab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        public bool CheckBusinessProfileExist(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM vBUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) return true;
                else return false;
            }
            catch (Exception e)
            {
                throw new Exception($"Error checking BP exist with idEtab = [{idEtab}]", e);
            }
        }
        #endregion

        #endregion

        #region Business Score

        #region Creation
        /// <summary>
        /// Create Business Score.
        /// </summary>
        /// <param name="businessScore"></param>
        public void CreateBusinessScore(DbBusinessScore businessScore)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_SCORE (ID_ETAB, SCORE, NB_REVIEWS) VALUES (@IdEtab, @Score, @NbReviews)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(businessScore.IdEtab));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(businessScore.Score));
                cmd.Parameters.AddWithValue("@NbReviews", GetValueOrDefault(businessScore.NbReviews));
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("Error while creating BS", e);
            }
        }
        #endregion

        #endregion

        #region Business Review

        #region Creation
        /// <summary>
        /// Create Business Review.
        /// </summary>
        /// <param name="businessReview"></param>
        public void CreateBusinessReview(DbBusinessReview businessReview)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_REVIEWS VALUES (@IdEtab, @IdReview, @UserName, @UserStatus, @Score, @UserNbReviews, @Review, @ReviewGoogleDate, @ReviewDate, @ReviewReplied, @DateInsert, @DateUpdate, @Processing)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(businessReview.IdEtab));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(businessReview.IdReview));
                cmd.Parameters.AddWithValue("@UserName", GetValueOrDefault(businessReview.User.Name));
                cmd.Parameters.AddWithValue("@UserStatus", GetValueOrDefault(businessReview.User.LocalGuide));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(businessReview.Score));
                cmd.Parameters.AddWithValue("@UserNbReviews", GetValueOrDefault(businessReview.User.NbReviews));
                cmd.Parameters.AddWithValue("@Review", GetValueOrDefault(businessReview.ReviewText));
                cmd.Parameters.AddWithValue("@ReviewGoogleDate", GetValueOrDefault(businessReview.ReviewGoogleDate));
                cmd.Parameters.AddWithValue("@ReviewDate", GetValueOrDefault(businessReview.ReviewDate));
                cmd.Parameters.AddWithValue("@ReviewReplied", GetValueOrDefault(businessReview.ReviewReplied));
                cmd.Parameters.AddWithValue("@DateInsert", GetValueOrDefault(businessReview.DateInsert));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessReview.DateUpdate));
                cmd.Parameters.AddWithValue("@Processing", 0);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("Error while creating BR", e);
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Get Business Review.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="idReview"></param>
        /// <returns>Business Review or Null (doesn't exist)</returns>
        public DbBusinessReview? GetBusinessReview(string idEtab, string idReview)
        {
            try
            {
                string selectCommand = "SELECT USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_ANSWERED FROM vBUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                using SqlDataReader reader = cmd.ExecuteReader();


                if (reader.Read())
                    return new DbBusinessReview(idEtab, idReview, new GoogleUser(reader.IsDBNull(0) ? null : reader.GetString(0), reader.IsDBNull(3) ? null : reader.GetInt32(3), reader.IsDBNull(1) ? false : reader.GetString(1) == "1"), reader.GetInt32(2), reader.IsDBNull(4) ? null : reader.GetString(4), null, null, reader.GetBoolean(5), null, null);
                else
                    return null;

            }
            catch (Exception e)
            {
                throw new Exception($"Error getting BR with id etab = [{idEtab}] and id review = [{idReview}]", e);
            }
        }
        #endregion Get

        #region Update
        /// <summary>
        /// Update Business Review.
        /// </summary>
        /// <param name="review"></param>
        public void UpdateBusinessReview(DbBusinessReview review)
        {
            try
            {
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET USER_NAME = @UserName, USER_STATUS = @UserStatus, SCORE = @Score, USER_NB_REVIEWS = @UserNbReviews, REVIEW = @Review, REVIEW_ANSWERED = @ReviewAnswered, DATE_UPDATE = @DateUpdate, REVIEW_GOOGLE_DATE = @ReviewGoogleDate, REVIEW_DATE = @ReviewDate WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UserName", GetValueOrDefault(review.User.Name));
                cmd.Parameters.AddWithValue("@UserStatus", GetValueOrDefault(review.User.LocalGuide));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(review.Score));
                cmd.Parameters.AddWithValue("@UserNbReviews", GetValueOrDefault(review.User.NbReviews));
                cmd.Parameters.AddWithValue("@Review", GetValueOrDefault(review.ReviewText));
                cmd.Parameters.AddWithValue("@ReviewAnswered", GetValueOrDefault(review.ReviewReplied));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(review.DateUpdate));
                cmd.Parameters.AddWithValue("@ReviewGoogleDate", GetValueOrDefault(review.ReviewGoogleDate));
                cmd.Parameters.AddWithValue("@ReviewDate", GetValueOrDefault(review.ReviewDate));
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(review.IdEtab));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(review.IdReview));
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error updating BR with id etab = [{review.IdEtab}] and id review = [{review.Id}]", e);
            }
        }
        #endregion

        #region Other
        /// <summary>
        /// Check if a review exist on a Business Profile.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="idReview"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        public bool CheckBusinessReviewExist(string idEtab, string idReview)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM vBUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read();
            }
            catch (Exception e)
            {
                throw new Exception($"Error while checking if BR exists with id etab = [{idEtab}] and id review = [{idReview}]", e);
            }
        }
        #endregion

        #endregion
    }
}