using System.Data.SqlClient;

namespace GMS.Sdk.Core.DbModels
{
    public class DbLib : IDisposable {
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
        /// Check if value is null or not.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Value or DBNull.Value</returns>
        private static object GetValueOrDefault(object? value) {
            return value ?? DBNull.Value;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Connection?.Close();
                Connection?.Dispose();
            }
        }
        ~DbLib() {
            Dispose(false);
        }

        /// <summary>
        /// Get Categories by given Activity.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>List of Google categories</returns>
        /// <exception cref="Exception"></exception>
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
        /// <summary>
        /// Create Business Url.
        /// </summary>
        /// <param name="businessUrl"></param>
        /// <exception cref="Exception"></exception>
        public void CreateBusinessUrl(DbBusinessUrl businessUrl)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_URL VALUES (@Guid, @Url, @DateInsert, @State, @TextSearch, @DateUpdate, @UrlEncoded)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", businessUrl.Guid.ToString());
                cmd.Parameters.AddWithValue("@Url", GetValueOrDefault(businessUrl.Url));
                cmd.Parameters.AddWithValue("@DateInsert", GetValueOrDefault(businessUrl.DateInsert));
                cmd.Parameters.AddWithValue("@State", businessUrl.State.ToString());
                cmd.Parameters.AddWithValue("@TextSearch", GetValueOrDefault(businessUrl.TextSearch));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessUrl.DateUpdate));
                cmd.Parameters.AddWithValue("@UrlEncoded", GetValueOrDefault(businessUrl.UrlEncoded));
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check if url exist.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckBusinessUrlExist(string urlEncoded)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM BUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    reader.Close();
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            catch (Exception)
            {
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
        public List<DbBusinessAgent> GetBusinessAgentListByUrlState(UrlState urlState, int entries)
        {
            List<DbBusinessAgent> businessAgentList = new();
            try
            {
                string selectCommand = "SELECT TOP (@Entries) GUID, URL FROM BUSINESS_URL WHERE STATE = @UrlState";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@UrlState", urlState.ToString());
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent business = new(reader.GetString(0), reader.GetString(1));
                    businessAgentList.Add(business);
                }
                return businessAgentList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Business Agent list by url list (Networks only).
        /// </summary>
        /// <param name="urlList"></param>
        /// <returns>List of Business Agentl</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessAgentNetworkListByUrlList(List<string> urlList)
        {
            List<DbBusinessAgent> businessAgentList = new();
            string urlEncoded;

            foreach (string url in urlList)
            {
                try
                {
                    urlEncoded = ToolBox.ComputeMd5Hash(url);
                    string selectCommand = "SELECT GUID, URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU WHERE URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        DbBusinessAgent business = new(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                        businessAgentList.Add(business);
                    }
                }
                catch (Exception)
                {
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
        public List<DbBusinessAgent> GetBusinessAgentIndependantListByUrlList(List<string> urlList)
        {
            List<DbBusinessAgent> businessAgentList = new();
            string urlEncoded;

            foreach (string url in urlList)
            {
                try
                {
                    urlEncoded = ToolBox.ComputeMd5Hash(url);
                    string selectCommand = "SELECT GUID, URL, ID_ETAB FROM vBUSINESS_PROFILE_HORS_RESEAU WHERE URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        DbBusinessAgent business = new(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                        businessAgentList.Add(business);
                    }
                }
                catch (Exception)
                {
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Business Url Guid by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Guid</returns>
        /// <exception cref="Exception"></exception>
        public string? GetBusinessUrlGuidByUrlEncoded(string urlEncoded)
        {
            try
            {
                string selectCommand = "SELECT GUID FROM BUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                    return reader.GetString(0);

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Delete Business Url by Guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <exception cref="Exception"></exception>
        public void DeleteBusinessUrlByGuid(string guid)
        {
            try
            {
                string deleteCommand = "DELETE FROM BUSINESS_URL WHERE GUID = @Guid";
                using SqlCommand cmd = new(deleteCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.ExecuteNonQuery(); 
            }
            catch (Exception)
            {
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
        public void CreateBusinessProfile(DbBusinessProfile businessProfile)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_PROFILE VALUES (@IdEtab, @FirstGuid, @Name, @Category, @GoogleAddress, @Tel, @Website, @Geoloc, @DateInsert, @UpdateCount, @DateUpdate, @Status, @Processing, @UrlPicture, @Address, @PostCode, @City, @CityCode, @Lat, @Lon, @IdBan, @AddressType, @StreetNumber)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(businessProfile.IdEtab));
                cmd.Parameters.AddWithValue("@FirstGuid", GetValueOrDefault(businessProfile.FirstGuid));
                cmd.Parameters.AddWithValue("@Name", GetValueOrDefault(businessProfile.Name));
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(businessProfile.Category));
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(businessProfile.Tel));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(businessProfile.Website));
                cmd.Parameters.AddWithValue("@UrlPicture", GetValueOrDefault(businessProfile.PictureUrl));
                cmd.Parameters.AddWithValue("@DateInsert", GetValueOrDefault(businessProfile.DateInsert));
                cmd.Parameters.AddWithValue("@Geoloc", GetValueOrDefault(DBNull.Value));
                cmd.Parameters.AddWithValue("@Processing", 0);
                cmd.Parameters.AddWithValue("@UpdateCount", 0);
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessProfile.DateUpdate));
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
            catch (Exception)
            {
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
        public List<DbBusinessAgent> GetBusinessListNetworkByCategory(string category, int entries)
        {
            List<DbBusinessAgent> businessUrlList = new();

            try
            {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE CATEGORY = @Category";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception)
            {
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
        public List<DbBusinessAgent> GetBusinessListNetworkByActivity(string activity, int entries)
        {
            List<DbBusinessAgent> businessUrlList = new();

            try
            {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE ACTIVITE = @Activity";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Activity", activity);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception)
            {
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
        public List<DbBusinessAgent> GetBusinessListNetworkBySector(string sector, int entries)
        {
            List<DbBusinessAgent> businessUrlList = new();

            try
            {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE SECTEUR = @Sector";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Sector", sector);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception)
            {
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
        public List<DbBusinessAgent> GetBusinessListNetworkByUnivers(string univers, int entries)
        {
            List<DbBusinessAgent> businessUrlList = new();

            try
            {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE UNIVERS = @Univers";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Univers", univers);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception)
            {
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
        public List<DbBusinessAgent> GetBusinessListNetworkByBrand(string brand, int entries)
        {
            List<DbBusinessAgent> businessUrlList = new();

            try
            {
                string selectCommand =
                    "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU" +
                    "WHERE MARQUE = @Brand";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Brand", brand);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Business list (Networks only).
        /// </summary>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessListNetwork(int entries, int processing)
        {
            List<DbBusinessAgent> businessUrlList = new();

            try
            {
                string selectCommand = "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_RESEAU WHERE PROCESSING = @Processing order by id_etab asc";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Business list.
        /// </summary>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessList(int entries, int processing) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand = "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE WHERE PROCESSING = @Processing order by id_etab desc";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
                    businessUrlList.Add(businessProfile);
                }

                return businessUrlList;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Get Business list (Not network).
        /// </summary>
        /// <param name="entries"></param>
        /// <returns>List of Business Agent</returns>
        /// <exception cref="Exception"></exception>
        public List<DbBusinessAgent> GetBusinessListNotNetwork(int entries, int processing) {
            List<DbBusinessAgent> businessUrlList = new();

            try {
                string selectCommand = "SELECT TOP (@Entries) URL, ID_ETAB FROM vBUSINESS_PROFILE_HORS_RESEAU WHERE PROCESSING = @Processing";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    DbBusinessAgent businessProfile = new(null, reader.GetString(0), reader.GetString(1));
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
        public DbBusinessAgent? GetBusinessByUrlEncoded(string urlEncoded)
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
                    DbBusinessAgent businessProfile = new(reader.GetString(1), reader.GetString(2), reader.GetString(0));
                    return businessProfile;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check if business profile exist.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        /// <exception cref="Exception"></exception>
        public bool CheckBusinessProfileExist(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM BUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Business Profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessProfile(DbBusinessProfile businessProfile)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET NAME = @Name, ADRESS = @GoogleAddress, A_ADDRESS = @Address, A_POSTCODE = @PostCode, A_CITY = @City, A_CITY_CODE = @CityCode, A_LON = @Lon, A_LAT = @Lat, A_BAN_ID = @IdBan, A_ADDRESS_TYPE = @AddressType, A_NUMBER = @StreetNumber, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status, URL_PICTURE = @UrlPicture WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Name", GetValueOrDefault(businessProfile.Name));
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
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(businessProfile.IdEtab));
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Business Profile state.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="processing"></param>
        /// <exception cref="Exception"></exception>
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Business Profile state.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="businessStatus"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessProfileStatus(string idEtab, BusinessStatus businessStatus)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET STATUS = @Status, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Status", businessStatus.ToString());
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(DateTime.UtcNow));
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.ExecuteNonQuery(); 
            }
            catch (Exception)
            {
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
        public void CreateBusinessScore(DbBusinessScore businessScore)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_SCORE VALUES (@IdEtab, @Score, @NbReviews, @DateInsert)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(businessScore.IdEtab));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(businessScore.Score));
                cmd.Parameters.AddWithValue("@NbReviews", GetValueOrDefault(businessScore.NbReviews));
                cmd.Parameters.AddWithValue("@DateInsert", GetValueOrDefault(businessScore.DateInsert));
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
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
        public bool CheckBusinessReviewExist(string idEtab, string idReview)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM BUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Business Review.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="idReview"></param>
        /// <returns>Business Review or Null (doesn't exist)</returns>
        /// <exception cref="Exception"></exception>
        public DbBusinessReview? GetBusinessReview(string idEtab, string idReview)
        {
            try
            {
                string selectCommand = "SELECT USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_ANSWERED FROM BUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                using SqlDataReader reader = cmd.ExecuteReader();


                if (reader.Read())
                    return new DbBusinessReview(idEtab, idReview, new GoogleUser(reader.IsDBNull(0) ? null : reader.GetString(0), reader.IsDBNull(3) ? null : reader.GetInt32(3), reader.IsDBNull(1) ? false : reader.GetString(1) == "1"), reader.GetInt32(2), reader.IsDBNull(4) ? null : reader.GetString(4), null, null, reader.GetBoolean(5), null, null);
                else
                    return null;

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Business Review.
        /// </summary>
        /// <param name="review"></param>
        /// <exception cref="Exception"></exception>
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Business Review without updating date.
        /// </summary>
        /// <param name="review"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateBusinessReviewWithoutUpdatingDate(DbBusinessReview review)
        {
            try
            {
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET USER_NAME = @UserName, USER_STATUS = @UserStatus, SCORE = @Score, USER_NB_REVIEWS = @UserNbReviews, REVIEW = @Review, REVIEW_ANSWERED = @ReviewAnswered, DATE_UPDATE = @DateUpdate WHERE ID_ETAB = @IdEtab AND REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UserName", GetValueOrDefault(review.User.Name));
                cmd.Parameters.AddWithValue("@UserStatus", GetValueOrDefault(review.User.LocalGuide));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(review.Score));
                cmd.Parameters.AddWithValue("@UserNbReviews", GetValueOrDefault(review.User.NbReviews));
                cmd.Parameters.AddWithValue("@Review", GetValueOrDefault(review.ReviewText));
                cmd.Parameters.AddWithValue("@ReviewAnswered", GetValueOrDefault(review.ReviewReplied));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(review.DateUpdate));
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(review.IdEtab));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(review.IdReview));
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }  
}