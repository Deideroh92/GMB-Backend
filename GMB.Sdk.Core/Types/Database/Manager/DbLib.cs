using GMB.Sdk.Core.FileGenerators.Sticker;
using GMB.Sdk.Core.Types.BusinessService;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.PlaceService;
using System.Data;
using System.Data.SqlClient;

namespace GMB.Sdk.Core.Types.Database.Manager
{
    public class DbLib : IDisposable
    {

        private const string connectionString = @"Data Source=vasano.database.windows.net;Initial Catalog=GMS;User ID=vs-sa;Password=Eu6pkR2J4";
        private const string connectionStringStickers = @"Data Source=vasano.database.windows.net;Initial Catalog=STICKERS_DEV;User ID=vs-sa;Password=Eu6pkR2J4";
        private readonly SqlConnection Connection;

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        public DbLib(bool stickers = false)
        {
            if (stickers)
                Connection = new SqlConnection(connectionStringStickers);
            else
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
            } catch (Exception e)
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
            } catch (Exception e)
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


                List<string> categories = [];

                while (reader.Read())
                    categories.Add(reader.GetString(0));

                return categories;
            } catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Get Brand total.
        /// </summary>
        /// <returns>Total or null</returns>
        public int? GetBrandTotal()
        {
            try
            {
                string selectCommand = "SELECT COUNT(1), MARQUE FROM MARQUES_SECTEURS GROUP BY MARQUE";
                using SqlCommand cmd = new(selectCommand, Connection);
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read() ? reader.GetInt32(0) : null;

            } catch (Exception e)
            {
                throw new Exception($"Error getting brand total", e);
            }
        }
        #endregion

        #region Users
        public DbUser? GetUser(string login, string password)
        {
            try
            {
                string selectCommand = "SELECT * FROM USERS WHERE LOGIN = @Login and PASSWORD = @Paswword";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Login", login);
                cmd.Parameters.AddWithValue("@Paswword", password);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return new(reader.GetString(1), reader.GetString(2), reader.GetInt64(0));
                else
                    return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting User with login = [{login}] and password = [{password}]", e);
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
            } catch (Exception e)
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
        public List<BusinessAgent> GetBusinessAgentListByUrlState(UrlState urlState, int? entries, int? processing)
        {
            List<BusinessAgent> businessAgentList = [];
            try
            {
                string table = "";
                table = urlState switch
                {
                    UrlState.NEW => "vBUSINESS_URL_NEW",
                    UrlState.UPDATED => "vBUSINESS_URL_UPDATED",
                    UrlState.NO_CATEGORY => "vBUSINESS_URL_NO_CATEGORY",
                    UrlState.DELETED => "vBUSINESS_URL_DELETED",
                    UrlState.PROCESSING => "vBUSINESS_URL_PROCESSING",
                    _ => "vBUSINESS_URL",
                };
                string selectCommand = entries == null ? ("SELECT GUID, URL FROM " + table + " WHERE STATE = @UrlState AND PROCESSING = @Processing") : ("SELECT TOP (@Entries) GUID, URL FROM " + table + " WHERE STATE = @UrlState AND PROCESSING = @Processing");
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", entries);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.Parameters.AddWithValue("@UrlState", urlState.ToString());
                cmd.CommandTimeout = 10000;
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
            List<BusinessAgent> businessAgentList = [];
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
                    string selectCommand = "SELECT URL, ID_ETAB, PLACE_ID FROM " + table + " WHERE URL_MD5 = @UrlEncoded";
                    using SqlCommand cmd = new(selectCommand, Connection);
                    cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {

                        BusinessAgent business = new(null, reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2));
                        businessAgentList.Add(business);
                    }
                } catch (Exception e)
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
            } catch (Exception e)
            {
                throw new Exception($"Error getting BU guid with url encoded = [{urlEncoded}]", e);
            }
        }
        /// <summary>
        /// Get Business Url Guid by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Guid</returns>
        public DbBusinessUrl? GetBusinessUrlByUrlEncoded(string urlEncoded)
        {
            try
            {
                string selectCommand = "SELECT GUID, URL FROM vBUSINESS_URL WHERE URL_MD5 = @UrlEncoded";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return new(reader.GetString(0), reader.GetString(1), null);

                return null;
            } catch (Exception e)
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
            } catch (Exception e)
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
            } catch (Exception e)
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
            } catch (Exception e)
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
                string insertCommand = "INSERT INTO BUSINESS_PROFILE (PLACE_ID, ID_ETAB, FIRST_GUID, NAME, CATEGORY, ADRESS, PLUS_CODE, TEL, WEBSITE, GEOLOC, STATUS, PROCESSING, URL_PICTURE, A_ADDRESS, A_POSTCODE, A_CITY, A_CITY_CODE, A_LAT, A_LON, A_BAN_ID, A_ADDRESS_TYPE, A_NUMBER, A_SCORE, A_COUNTRY, URL_PLACE, TEL_INT, LOCATED_IN, HAS_OPENING_HOURS, IS_VERIFIED) VALUES (@PlaceId, @IdEtab, @FirstGuid, @Name, @Category, @GoogleAddress, @PlusCode, @Tel, @Website, @Geoloc, @Status, @Processing, @UrlPicture, @Address, @PostCode, @City, @CityCode, @Lat, @Lon, @IdBan, @AddressType, @StreetNumber, @AddressScore, @Country, @UrlPlace, @TelInt, @LocatedIn, @HasOpeningHours, @IsVerified)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", GetValueOrDefault(businessProfile.PlaceId));
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@FirstGuid", businessProfile.FirstGuid);
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(businessProfile.Category));
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@PlusCode", GetValueOrDefault(businessProfile.PlusCode));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(businessProfile.Tel));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(businessProfile.Website));
                cmd.Parameters.AddWithValue("@Geoloc", GetValueOrDefault(businessProfile.Geoloc));
                cmd.Parameters.AddWithValue("@UrlPicture", GetValueOrDefault(businessProfile.PictureUrl));
                cmd.Parameters.AddWithValue("@Processing", 8);
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
                cmd.Parameters.AddWithValue("@AddressScore", GetValueOrDefault(businessProfile.AddressScore));
                cmd.Parameters.AddWithValue("@Country", GetValueOrDefault(businessProfile.Country));
                cmd.Parameters.AddWithValue("@UrlPlace", GetValueOrDefault(businessProfile.PlaceUrl));
                cmd.Parameters.AddWithValue("@TelInt", GetValueOrDefault(businessProfile.TelInt));
                cmd.Parameters.AddWithValue("@LocatedIn", GetValueOrDefault(businessProfile.LocatedIn));
                cmd.Parameters.AddWithValue("@HasOpeningHours", GetValueOrDefault(businessProfile.HasBusinessHours));
                cmd.Parameters.AddWithValue("@IsVerified", GetValueOrDefault(businessProfile.IsVerified));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
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
            List<BusinessAgent> businessUrlList = [];
            string table = "vBUSINESS_PROFILE";
            string categoryFilter = "";
            string brand = "";

            if (request.IsNetwork)
            {
                table = "vBUSINESS_PROFILE_RESEAU";
                if (request.Brand != null)
                    brand = " AND MARQUE = '" + request.Brand + "'";
            }

            if (request.IsIndependant)
                table = "vBUSINESS_PROFILE_HORS_RESEAU";

            switch (request.CategoryFamily)
            {
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
                    categoryFilter;

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
            } catch (Exception e)
            {
                throw new Exception($"Error getting BA list", e);
            }
        }
        /// <summary>
        /// Get Business list.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of Business</returns>
        public List<DbBusinessProfile> GetBusinessList(GetBusinessListRequest request)
        {
            List<DbBusinessProfile> businessList = [];
            string table = "vBUSINESS_PROFILE";
            string categoryFilter = "";
            string brand = "";

            if (request.IsNetwork)
            {
                table = "vBUSINESS_PROFILE_RESEAU";
                if (request.Brand != null)
                    brand = " AND MARQUE = " + request.Brand;
            }

            if (request.IsIndependant)
                table = "vBUSINESS_PROFILE_HORS_RESEAU";

            switch (request.CategoryFamily)
            {
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

            try
            {
                string selectCommand = "SELECT TOP (@Entries) * FROM " + table +
                    " WHERE PROCESSING = @Processing" +
                    brand +
                    categoryFilter;

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Entries", request.Entries);
                cmd.Parameters.AddWithValue("@Processing", request.Processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()!),
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                    );
                    businessList.Add(businessProfile);
                }

                return businessList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP list", e);
            }
        }
        /// <summary>
        /// Get Business Agent by Url Encoded.
        /// </summary>
        /// <param name="urlEncoded"></param>
        /// <returns>Business Agent or Null if not found</returns>
        public BusinessAgent? GetBusinessAgentByUrlEncoded(string urlEncoded)
        {
            try
            {
                string selectCommand = "SELECT GUID, URL FROM vBUSINESS_URL WHERE URL_MD5 = @UrlEncoded";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@UrlEncoded", urlEncoded);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BusinessAgent businessProfile = new(reader.GetString(0), reader.GetString(1), null);
                    return businessProfile;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BA by url encoded = [{urlEncoded}]", e);
            }
        }
        /// <summary>
        /// Get Business by Id Etab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business or null</returns>
        public BusinessAgent? GetBusinessAgentByIdEtab(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT FIRST_GUID, URL, ID_ETAB FROM vBUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    BusinessAgent businessProfile = new(reader.GetString(0), reader.GetString(1), reader.GetString(2));
                    return businessProfile;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BA by id etab = [{idEtab}]", e);
            }
        }
        /// <summary>
        /// Get Business by Name and Address.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <returns>Business or null</returns>
        public List<DbBusinessProfile> GetBusinessByNameAndAdress(string name, string adress)
        {
            try
            {
                List<DbBusinessProfile> bpList = [];
                string selectCommand = "SELECT * FROM vBUSINESS_PROFILE WHERE NAME = @Name AND ADRESS = @Adress";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Adress", adress);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()!),
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                        );

                    bpList.Add(businessProfile);
                }

                return bpList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP by name = [{name}] and adress = [{adress}]", e);
            }
        }
        /// <summary>
        /// Get Business by Id Etab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business or null</returns>
        public DbBusinessProfile? GetBusinessByIdEtab(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT * FROM vBUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        Enum.TryParse<BusinessStatus>(reader["STATUS"].ToString(), out var status) ? status : BusinessStatus.OPERATIONAL,
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                        );
                    return businessProfile;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP by id etab = [{idEtab}]", e);
            }
        }
        /// <summary>
        /// Get Business by Id Etab.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Business or null</returns>
        public DbBusinessProfile? GetBusinessByPlaceId(string placeId)
        {
            try
            {
                string selectCommand = "SELECT * FROM vBUSINESS_PROFILE WHERE PLACE_ID = @PlaceId";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", placeId);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()!),
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                        );
                    return businessProfile;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP by id etab = [{placeId}]", e);
            }
        }
        /// <summary>
        /// Get Business by Id Etab.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Business or null</returns>
        public List<DbBusinessProfile>? GetBusinessListByPlaceId(string placeId)
        {
            try
            {
                string selectCommand = "SELECT * FROM vBUSINESS_PROFILE WHERE PLACE_ID = @PlaceId";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", placeId);
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbBusinessProfile> bpList = new();

                while (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()!),
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                        );
                    bpList.Add(businessProfile);
                    
                }
                return bpList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP by id etab = [{placeId}]", e);
            }
        }
        /// <summary>
        /// Get Business by Id Etab.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Business or null</returns>
        public DbBusinessProfile? GetBusinessByGuid(string guid)
        {
            try
            {
                string selectCommand = "SELECT * FROM vBUSINESS_PROFILE WHERE FIRST_GUID = @Guid";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Guid", guid);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()!),
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                        );
                    return businessProfile;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP by url = [{guid}]", e);
            }
        }
        /// <summary>
        /// Get Business by URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Business or null</returns>
        public DbBusinessProfile? GetBusinessByUrl(string url)
        {
            try
            {
                string urlEncoded = ToolBox.ComputeMd5Hash(url);
                string selectCommand = "SELECT * FROM vBUSINESS_PROFILE WHERE URL_MD5 = @urlEncoded";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@urlEncoded", urlEncoded);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DbBusinessProfile businessProfile = new(
                        (reader["PLACE_ID"] != DBNull.Value) ? reader["PLACE_ID"].ToString() : null,
                        reader["ID_ETAB"].ToString()!,
                        reader["FIRST_GUID"].ToString()!,
                        (reader["NAME"] != DBNull.Value) ? reader["NAME"].ToString() : null,
                        (reader["CATEGORY"] != DBNull.Value) ? reader["CATEGORY"].ToString() : null,
                        (reader["ADRESS"] != DBNull.Value) ? reader["ADRESS"].ToString() : null,
                        (reader["A_ADDRESS"] != DBNull.Value) ? reader["A_ADDRESS"].ToString() : null,
                        (reader["A_POSTCODE"] != DBNull.Value) ? reader["A_POSTCODE"].ToString() : null,
                        (reader["A_CITY"] != DBNull.Value) ? reader["A_CITY"].ToString() : null,
                        (reader["A_CITY_CODE"] != DBNull.Value) ? reader["A_CITY_CODE"].ToString() : null,
                        (reader["A_LAT"] != DBNull.Value) ? Convert.ToDouble(reader["A_LAT"]) : null,
                        (reader["A_LON"] != DBNull.Value) ? Convert.ToDouble(reader["A_LON"]) : null,
                        (reader["A_BAN_ID"] != DBNull.Value) ? reader["A_BAN_ID"].ToString() : null,
                        (reader["A_ADDRESS_TYPE"] != DBNull.Value) ? reader["A_ADDRESS_TYPE"].ToString() : null,
                        (reader["A_NUMBER"] != DBNull.Value) ? reader["A_NUMBER"].ToString() : null,
                        (reader["A_SCORE"] != DBNull.Value) ? Convert.ToDouble(reader["A_SCORE"]) : null,
                        (reader["TEL"] != DBNull.Value) ? reader["TEL"].ToString() : null,
                        (reader["WEBSITE"] != DBNull.Value) ? reader["WEBSITE"].ToString() : null,
                        (reader["PLUS_CODE"] != DBNull.Value) ? reader["PLUS_CODE"].ToString() : null,
                        (reader["DATE_UPDATE"] != DBNull.Value) ? DateTime.Parse(reader["DATE_UPDATE"].ToString()!) : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["STATUS"].ToString()!),
                        (reader["URL_PICTURE"] != DBNull.Value) ? reader["URL_PICTURE"].ToString() : null,
                        (reader["A_COUNTRY"] != DBNull.Value) ? reader["A_COUNTRY"].ToString() : null,
                        (reader["URL_PLACE"] != DBNull.Value) ? reader["URL_PLACE"].ToString() : null,
                        (reader["GEOLOC"] != DBNull.Value) ? reader["GEOLOC"].ToString() : null,
                        (short)reader["PROCESSING"],
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null,
                        (reader["TEL_INT"] != DBNull.Value) ? reader["TEL_INT"].ToString() : null,
                        (reader["LOCATED_IN"] != DBNull.Value) ? reader["LOCATED_IN"].ToString() : null,
                        (reader["HAS_OPENING_HOURS"] != DBNull.Value) ? Convert.ToBoolean(reader["HAS_OPENING_HOURS"]) : null,
                        (reader["IS_VERIFIED"] != DBNull.Value) ? Convert.ToBoolean(reader["IS_VERIFIED"]) : null
                        );
                    return businessProfile;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BP by url = [{url}]", e);
            }
        }
        /// <summary>
        /// Get Id Etab by place Id.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Id Etab or null if not found</returns>
        public string? GetIdEtabByPlaceId(string placeId)
        {
            try
            {
                string selectCommand = "SELECT ID_ETAB FROM vBUSINESS_PROFILE WHERE PLACE_ID = @placeId";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@PLACE_ID", placeId);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return reader.GetString(0);
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting id etab by place id = [{placeId}]", e);
            }
        }
        /// <summary>
        /// Get Business Profile total.
        /// </summary>
        /// <returns>Total or null</returns>
        public int? GetBPTotal()
        {
            try
            {
                string selectCommand = "SELECT COUNT(1) FROM vBUSINESS_PROFILE";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read() ? reader.GetInt32(0) : null;

            } catch (Exception e)
            {
                throw new Exception($"Error getting business profile total", e);
            }
        }
        /// <summary>
        /// Get Business Profile Network total.
        /// </summary>
        /// <returns>Total or null</returns>
        public int? GetBPNetworkTotal()
        {
            try
            {
                string selectCommand = "SELECT COUNT(1) FROM vBUSINESS_PROFILE_RESEAU";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read() ? reader.GetInt32(0) : null;

            } catch (Exception e)
            {
                throw new Exception($"Error getting business profile network total", e);
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
                string insertCommand = "UPDATE BUSINESS_PROFILE SET PLACE_ID = @PlaceId, NAME = @Name, ADRESS = @GoogleAddress, GEOLOC = @Geoloc, PLUS_CODE = @PlusCode, A_ADDRESS = @Address, A_POSTCODE = @PostCode, A_CITY = @City, A_CITY_CODE = @CityCode, A_LON = @Lon, A_LAT = @Lat, A_BAN_ID = @IdBan, A_ADDRESS_TYPE = @AddressType, A_NUMBER = @StreetNumber, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status, URL_PICTURE = @UrlPicture, A_SCORE = @AddressScore, A_COUNTRY = @Country, LOCATED_IN = @LocatedIn, HAS_OPENING_HOURS = @HasOpeningHours, IS_VERIFIED = @IsVerified WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@PlaceId", GetValueOrDefault(businessProfile.PlaceId));
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@Address", GetValueOrDefault(businessProfile.Address));
                cmd.Parameters.AddWithValue("@PlusCode", GetValueOrDefault(businessProfile.PlusCode));
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
                cmd.Parameters.AddWithValue("@AddressScore", GetValueOrDefault(businessProfile.AddressScore));
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@Geoloc", GetValueOrDefault(businessProfile.Geoloc));
                cmd.Parameters.AddWithValue("@Country", GetValueOrDefault(businessProfile.Country));
                cmd.Parameters.AddWithValue("@LocatedIn", GetValueOrDefault(businessProfile.LocatedIn));
                cmd.Parameters.AddWithValue("@HasOpeningHours", GetValueOrDefault(businessProfile.HasBusinessHours));
                cmd.Parameters.AddWithValue("@IsVerified", GetValueOrDefault(businessProfile.IsVerified));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BP with id etab = [{businessProfile.IdEtab}] and guid = [{businessProfile.FirstGuid}]", e);
            }
        }
        /// <summary>
        /// Update Business profile without address details.
        /// </summary>
        /// <param name="businessProfile"></param>
        public void UpdateBusinessProfileWithoutAddress(DbBusinessProfile businessProfile)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET PLACE_ID = @PlaceId, NAME = @Name, PLUS_CODE = @PlusCode, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status, URL_PICTURE = @UrlPicture, LOCATED_IN = @LocatedIn, HAS_OPENING_HOURS = @HasOpeningHours, IS_VERIFIED = @IsVerified WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@PlaceId", GetValueOrDefault(businessProfile.PlaceId));
                cmd.Parameters.AddWithValue("@Name", businessProfile.Name);
                cmd.Parameters.AddWithValue("@PlusCode", GetValueOrDefault(businessProfile.PlusCode));
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(businessProfile.Category));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(businessProfile.Tel));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(businessProfile.Website));
                cmd.Parameters.AddWithValue("@UrlPicture", GetValueOrDefault(businessProfile.PictureUrl));
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Status", businessProfile.Status.ToString());
                cmd.Parameters.AddWithValue("@LocatedIn", GetValueOrDefault(businessProfile.LocatedIn));
                cmd.Parameters.AddWithValue("@HasOpeningHours", GetValueOrDefault(businessProfile.HasBusinessHours));
                cmd.Parameters.AddWithValue("@IsVerified", GetValueOrDefault(businessProfile.IsVerified));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BP without address with id etab = [{businessProfile.IdEtab}] and guid = [{businessProfile.FirstGuid}]", e);
            }
        }
        /// <summary>
        /// Update Business from web portal.
        /// </summary>
        /// <param name="business"></param>
        public void UpdateBusinessProfileFromWeb(Business business)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET NAME = @Name, ADRESS = @GoogleAddress, GEOLOC = @Geoloc, PLUS_CODE = @PlusCode, A_ADDRESS = @Address, A_POSTCODE = @PostCode, A_CITY = @City, A_CITY_CODE = @CityCode, A_LON = @Lon, A_LAT = @Lat, A_BAN_ID = @IdBan, A_ADDRESS_TYPE = @AddressType, A_NUMBER = @StreetNumber, CATEGORY = @Category, TEL = @Tel, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status, URL_PICTURE = @UrlPicture, A_SCORE = @AddressScore, A_COUNTRY = @Country, URL_PLACE = @Url, TEL_INT = @TelInt, PROCESSING = @Processing, LOCATED_IN = @LocatedIn, PLACE_ID = @PlaceId WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", business.IdEtab);
                cmd.Parameters.AddWithValue("@PlaceId", GetValueOrDefault(business.PlaceId));
                cmd.Parameters.AddWithValue("@Name", business.Name);
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(business.GoogleAddress));
                cmd.Parameters.AddWithValue("@Address", GetValueOrDefault(business.Address));
                cmd.Parameters.AddWithValue("@PlusCode", GetValueOrDefault(business.PlusCode));
                cmd.Parameters.AddWithValue("@City", GetValueOrDefault(business.City));
                cmd.Parameters.AddWithValue("@CityCode", GetValueOrDefault(business.CityCode));
                cmd.Parameters.AddWithValue("@IdBan", GetValueOrDefault(business.IdBan));
                cmd.Parameters.AddWithValue("@AddressType", GetValueOrDefault(business.AddressType));
                cmd.Parameters.AddWithValue("@Lon", GetValueOrDefault(business.Lon));
                cmd.Parameters.AddWithValue("@Lat", GetValueOrDefault(business.Lat));
                cmd.Parameters.AddWithValue("@PostCode", GetValueOrDefault(business.PostCode));
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(business.Category));
                cmd.Parameters.AddWithValue("@StreetNumber", GetValueOrDefault(business.StreetNumber));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(business.Tel));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(business.Website));
                cmd.Parameters.AddWithValue("@UrlPicture", GetValueOrDefault(business.PictureUrl));
                cmd.Parameters.AddWithValue("@AddressScore", GetValueOrDefault(business.AddressScore));
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Status", business.Status.ToString());
                cmd.Parameters.AddWithValue("@Geoloc", GetValueOrDefault(business.Geoloc));
                cmd.Parameters.AddWithValue("@Country", GetValueOrDefault(business.Country));
                cmd.Parameters.AddWithValue("@Url", GetValueOrDefault(business.PlaceUrl));
                cmd.Parameters.AddWithValue("@TelInt", GetValueOrDefault(business.TelInt));
                cmd.Parameters.AddWithValue("@Processing", GetValueOrDefault(business.Processing));
                cmd.Parameters.AddWithValue("@LocatedIn", GetValueOrDefault(business.LocatedIn));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BP from web with id etab = [{business.IdEtab}] and guid = [{business.FirstGuid}]", e);
            }
        }
        /// <summary>
        /// Update Business profile with place Details.
        /// </summary>
        /// <param name="placeDetails"></param>
        public void UpdateBusinessProfileFromPlaceDetails(PlaceDetails placeDetails, string idEtab)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET NAME = @Name, ADRESS = @GoogleAddress, GEOLOC = @Geoloc, PLUS_CODE = @PlusCode, A_POSTCODE = @PostCode, A_CITY = @City, A_LON = @Lon, A_LAT = @Lat, A_NUMBER = @StreetNumber, CATEGORY = @Category, TEL = @Tel, TEL_INT = @TelInt, WEBSITE = @Website, UPDATE_COUNT = UPDATE_COUNT + 1, DATE_UPDATE = @DateUpdate, STATUS = @Status, A_COUNTRY = @Country, URL_PLACE = @PlaceUrl WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@Name", placeDetails.Name);
                cmd.Parameters.AddWithValue("@Category", GetValueOrDefault(placeDetails.FirstType));
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(placeDetails.Address));
                cmd.Parameters.AddWithValue("@PlusCode", GetValueOrDefault(placeDetails.PlusCode));
                cmd.Parameters.AddWithValue("@City", GetValueOrDefault(placeDetails.City));
                cmd.Parameters.AddWithValue("@Lon", GetValueOrDefault(placeDetails.Long));
                cmd.Parameters.AddWithValue("@Lat", GetValueOrDefault(placeDetails.Lat));
                cmd.Parameters.AddWithValue("@PostCode", GetValueOrDefault(placeDetails.PostalCode));
                cmd.Parameters.AddWithValue("@StreetNumber", GetValueOrDefault(placeDetails.StreetNumber));
                cmd.Parameters.AddWithValue("@Tel", GetValueOrDefault(placeDetails.Phone));
                cmd.Parameters.AddWithValue("@Website", GetValueOrDefault(placeDetails.Website));
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Status", placeDetails.Status);
                cmd.Parameters.AddWithValue("@Geoloc", GetValueOrDefault(placeDetails.Lat) + " , " + GetValueOrDefault(placeDetails.Long));
                cmd.Parameters.AddWithValue("@Country", GetValueOrDefault(placeDetails.Country));
                cmd.Parameters.AddWithValue("@PlaceUrl", GetValueOrDefault(placeDetails.Url));
                cmd.Parameters.AddWithValue("@TelInt", GetValueOrDefault(placeDetails.PhoneInternational));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BP from place details with place ID = [{placeDetails.PlaceId}]", e);
            }
        }
        /// <summary>
        /// Update Business profile address.
        /// </summary>
        /// <param name="businessProfile"></param>
        public void UpdateBusinessProfileAddress(DbBusinessProfile businessProfile)
        {
            try
            {
                string insertCommand = "UPDATE BUSINESS_PROFILE SET ADRESS = @GoogleAddress, PLUS_CODE = @PlusCode, A_ADDRESS = @Address, A_POSTCODE = @PostCode, A_CITY = @City, A_CITY_CODE = @CityCode, A_LON = @Lon, A_LAT = @Lat, A_BAN_ID = @IdBan, A_ADDRESS_TYPE = @AddressType, A_NUMBER = @StreetNumber, A_SCORE = @AddressScore, DATE_UPDATE = @DateUpdate, A_COUNTRY = @Country, LOCATED_IN = @LocatedIn WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.Parameters.AddWithValue("@GoogleAddress", GetValueOrDefault(businessProfile.GoogleAddress));
                cmd.Parameters.AddWithValue("@PlusCode", GetValueOrDefault(businessProfile.PlusCode));
                cmd.Parameters.AddWithValue("@Address", GetValueOrDefault(businessProfile.Address));
                cmd.Parameters.AddWithValue("@Country", GetValueOrDefault(businessProfile.Country));
                cmd.Parameters.AddWithValue("@City", GetValueOrDefault(businessProfile.City));
                cmd.Parameters.AddWithValue("@CityCode", GetValueOrDefault(businessProfile.CityCode));
                cmd.Parameters.AddWithValue("@IdBan", GetValueOrDefault(businessProfile.IdBan));
                cmd.Parameters.AddWithValue("@AddressType", GetValueOrDefault(businessProfile.AddressType));
                cmd.Parameters.AddWithValue("@Lon", GetValueOrDefault(businessProfile.Lon));
                cmd.Parameters.AddWithValue("@Lat", GetValueOrDefault(businessProfile.Lat));
                cmd.Parameters.AddWithValue("@PostCode", GetValueOrDefault(businessProfile.PostCode));
                cmd.Parameters.AddWithValue("@StreetNumber", GetValueOrDefault(businessProfile.StreetNumber));
                cmd.Parameters.AddWithValue("@AddressScore", GetValueOrDefault(businessProfile.AddressScore));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessProfile.DateUpdate));
                cmd.Parameters.AddWithValue("@LocatedIn", GetValueOrDefault(businessProfile.LocatedIn));
                cmd.Parameters.AddWithValue("@IdEtab", businessProfile.IdEtab);
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
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
                string insertCommand = "UPDATE BUSINESS_PROFILE SET PROCESSING = @Processing WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.ExecuteNonQuery();
            } catch (Exception e)
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
            } catch (Exception e)
            {
                throw new Exception($"Error updating BP status with id etab category = [{idEtab}] and status = [{businessStatus}]", e);
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete Business Profile.
        /// </summary>
        /// <param name="idEtab"></param>
        public void DeleteBusinessProfile(string idEtab)
        {
            try
            {
                string insertCommand = "DELETE FROM BUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(idEtab));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while deleting BP", e);
            }
        }
        #endregion

        #region Other
        /// <summary>
        /// Check if business profile exist by name and adress.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adress"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        public bool CheckBusinessProfileExistByNameAndAdress(string name, string adress)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM vBUSINESS_PROFILE WHERE NAME = @Name AND ADRESS = @Adress";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Adress", adress);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return true;
                else
                    return false;
            } catch (Exception e)
            {
                throw new Exception($"Error checking BP exist with name = [{name}] and adress = [{adress}]", e);
            }
        }
        /// <summary>
        /// Check if business profile exist by idEtab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        public bool CheckBusinessProfileExistByIdEtab(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM vBUSINESS_PROFILE WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return true;
                else
                    return false;
            } catch (Exception e)
            {
                throw new Exception($"Error checking BP exist with idEtab = [{idEtab}]", e);
            }
        }
        /// <summary>
        /// Check if business profile exist by idEtab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>True (exist) or False (doesn't exist)</returns>
        public bool CheckBusinessProfileExistByPlaceId(string placeId)
        {
            try
            {
                string selectCommand = "SELECT 1 FROM vBUSINESS_PROFILE WHERE ID_ETAB = @PlaceId";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", placeId);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                    return true;
                else
                    return false;
            } catch (Exception e)
            {
                throw new Exception($"Error checking BP exist with idEtab = [{placeId}]", e);
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
            } catch (Exception e)
            {
                throw new Exception("Error while creating BS", e);
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Get Business Score by Id Etab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business score or null</returns>
        public DbBusinessScore? GetBusinessScoreByIdEtab(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT TOP(1) * FROM BUSINESS_SCORE WHERE ID_ETAB = @IdEtab ORDER BY DATE_INSERT DESC";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DbBusinessScore businessScore = new(
                        reader["ID_ETAB"].ToString()!,
                        (reader["SCORE"] != DBNull.Value) ? (double)reader["SCORE"] : null,
                        (reader["NB_REVIEWS"] != DBNull.Value) ? (int?)reader["NB_REVIEWS"] : null,
                        (reader["DATE_INSERT"] != DBNull.Value) ? DateTime.Parse(reader["DATE_INSERT"].ToString()!) : null);
                    return businessScore;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BS by id etab = [{idEtab}]", e);
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete Business Score.
        /// </summary>
        /// <param name="idEtab"></param>
        public void DeleteBusinessScore(string idEtab)
        {
            try
            {
                string insertCommand = "DELETE FROM BUSINESS_SCORE WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(idEtab));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while deleting BS", e);
            }
        }
        #endregion

        #endregion

        #region Business Photos
        /// <summary>
        /// Create Business Photo.
        /// </summary>
        /// <param name="businessPhoto"></param>
        public void CreateBusinessPhoto(DbBusinessPhoto businessPhoto)
        {
            try
            {
                string insertCommand = "INSERT INTO BUSINESS_PHOTO (ID_ETAB, IS_OWNER, PHOTO_URL, DATE_INSERT) VALUES (@IdEtab, @IsOwner, @PhotoUrl, @DateInsert)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(businessPhoto.IdEtab));
                cmd.Parameters.AddWithValue("@IsOwner", GetValueOrDefault(businessPhoto.IsOwner));
                cmd.Parameters.AddWithValue("@PhotoUrl", GetValueOrDefault(businessPhoto.PhotoUrl));
                cmd.Parameters.AddWithValue("@DateInsert", GetValueOrDefault(businessPhoto.DateInsert));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while creating Business Photo", e);
            }
        }

        /// <summary>
        /// Delete Business Photo.
        /// </summary>
        /// <param name="idEtab"></param>
        public void DeleteBusinessPhoto(string idEtab)
        {
            try
            {
                string insertCommand = "DELETE FROM BUSINESS_PHOTO WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(idEtab));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while deleting Business Photo", e);
            }
        }
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
                string insertCommand = "INSERT INTO BUSINESS_REVIEWS (ID_ETAB, REVIEW_ID, USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_GOOGLE_DATE, REVIEW_GOOGLE_DATE_UPDATE, REVIEW_DATE, REVIEW_DATE_UPDATE, REVIEW_ANSWERED, DATE_UPDATE, PROCESSING, GOOGLE_REVIEW_ID, REVIEW_ANSWERED_GOOGLE_DATE, REVIEW_ANSWERED_DATE, VISIT_DATE) VALUES (@IdEtab, @IdReview, @UserName, @UserStatus, @Score, @UserNbReviews, @Review, @ReviewGoogleDate, @ReviewGoogleDateUpdate, @ReviewDate, @ReviewDateUpdate, @ReviewReplied, @DateUpdate, @Processing, @GoogleReviewId, @ReviewReplyGoogleDate, @ReviewReplyDate, @VisitDate)";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", businessReview.IdEtab);
                cmd.Parameters.AddWithValue("@IdReview", businessReview.IdReview);
                cmd.Parameters.AddWithValue("@UserName", GetValueOrDefault(businessReview.User.Name));
                cmd.Parameters.AddWithValue("@UserStatus", GetValueOrDefault(businessReview.User.LocalGuide));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(businessReview.Score));
                cmd.Parameters.AddWithValue("@UserNbReviews", GetValueOrDefault(businessReview.User.NbReviews));
                cmd.Parameters.AddWithValue("@Review", GetValueOrDefault(businessReview.ReviewText));
                cmd.Parameters.AddWithValue("@ReviewGoogleDate", GetValueOrDefault(businessReview.ReviewGoogleDate));
                cmd.Parameters.AddWithValue("@ReviewGoogleDateUpdate", GetValueOrDefault(businessReview.ReviewGoogleDate));
                cmd.Parameters.AddWithValue("@ReviewDate", GetValueOrDefault(businessReview.ReviewDate));
                cmd.Parameters.AddWithValue("@ReviewDateUpdate", GetValueOrDefault(businessReview.ReviewDate));
                cmd.Parameters.AddWithValue("@ReviewReplied", GetValueOrDefault(businessReview.ReviewReplied));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(businessReview.DateUpdate));
                cmd.Parameters.AddWithValue("@GoogleReviewId", businessReview.GoogleReviewId);
                cmd.Parameters.AddWithValue("@Processing", 0);
                cmd.Parameters.AddWithValue("@ReviewReplyGoogleDate", GetValueOrDefault(businessReview.ReviewReplyGoogleDate));
                cmd.Parameters.AddWithValue("@ReviewReplyDate", GetValueOrDefault(businessReview.ReviewReplyDate));
                cmd.Parameters.AddWithValue("@VisitDate", GetValueOrDefault(businessReview.VisitDate));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while creating BR", e);
            }
        }

        public void InsertThemeMatches(string reviewId, HashSet<int> themeIds)
        {
            if (themeIds.Count == 0)
                return;

            using var tx = Connection.BeginTransaction();

            var delete = new SqlCommand(
                "DELETE FROM REVIEW_THEME_MATCH WHERE REVIEW_ID = @rid",
                Connection, tx);
            delete.Parameters.AddWithValue("@rid", reviewId);
            delete.ExecuteNonQuery();

            var insert = new SqlCommand(
                "INSERT INTO REVIEW_THEME_MATCH (REVIEW_ID, THEME_ID) VALUES (@rid, @tid)",
                Connection, tx);

            insert.Parameters.Add("@rid", SqlDbType.NVarChar);
            insert.Parameters.Add("@tid", SqlDbType.Int);

            foreach (var themeId in themeIds)
            {
                insert.Parameters["@rid"].Value = reviewId;
                insert.Parameters["@tid"].Value = themeId;
                insert.ExecuteNonQuery();
            }

            tx.Commit();
        }


        #endregion

        #region Get
        /// <summary>
        /// Get Business Review.
        /// </summary>
        /// <param name="idReview"></param>
        /// <returns>Business Review or Null (doesn't exist)</returns>
        public DbBusinessReview? GetBusinessReview(string idReview)
        {
            try
            {
                string selectCommand = "SELECT USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_ANSWERED, GOOGLE_REVIEW_ID, REVIEW_ANSWERED_GOOGLE_DATE, REVIEW_ANSWERED_DATE, VISIT_DATE, REVIEW_DATE, REVIEW_GOOGLE_DATE, ID_ETAB, DELETED FROM vBUSINESS_REVIEWS WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                using SqlDataReader reader = cmd.ExecuteReader();


                if (reader.Read())
                {
                    return new DbBusinessReview(reader.GetString(12),
                        idReview,
                        reader.GetString(6),
                        new GoogleUser(reader.IsDBNull(0) ? null : reader.GetString(0),
                            reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            !reader.IsDBNull(1) && reader.GetBoolean(1)),
                        reader.GetInt32(2),
                        reader.IsDBNull(4) ? null : reader.GetString(4),
                        reader.IsDBNull(11) ? null : reader.GetString(11),
                        reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                        reader.GetBoolean(5),
                        null,
                        reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        reader.IsDBNull(7) ? null : reader.GetString(7),
                        reader.IsDBNull(9) ? null : reader.GetString(9),
                        null,
                        null,
                        !reader.IsDBNull(13) && reader.GetBoolean(13));  

                } else
                    return null;

            } catch (Exception e)
            {
                throw new Exception($"Error getting BR with id = [{idReview}]", e);
            }
        }
        /// <summary>
        /// Get list of Business Reviews.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business Reviews list or Null (doesn't exist)</returns>
        public List<DbBusinessReview> GetBusinessReviewsList(string idEtab)
        {
            try
            {
                string selectCommand = "SELECT USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_ANSWERED, GOOGLE_REVIEW_ID, REVIEW_ANSWERED_GOOGLE_DATE, REVIEW_ANSWERED_DATE, REVIEW_ID, VISIT_DATE, REVIEW_GOOGLE_DATE, REVIEW_DATE, DATE_UPDATE, DATE_INSERT, DELETED FROM vBUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbBusinessReview> brList = [];

                while (reader.Read())
                {
                    brList.Add(new DbBusinessReview(idEtab,
                        reader.GetString(9),
                        reader.GetString(6),
                        new GoogleUser(reader.IsDBNull(0) ? null : reader.GetString(0),
                            reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            !reader.IsDBNull(1) && reader.GetBoolean(1)),
                        reader.GetInt32(2),
                        reader.IsDBNull(4) ? null : reader.GetString(4),
                        reader.IsDBNull(11) ? null : reader.GetString(11),
                        reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                        reader.GetBoolean(5),
                        reader.IsDBNull(13) ? null : reader.GetDateTime(13),
                        reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        reader.IsDBNull(7) ? null : reader.GetString(7),
                        reader.IsDBNull(10) ? null : reader.GetString(10),
                        null,
                        reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                        !reader.IsDBNull(15) && reader.GetBoolean(15)
                        ));
                }
                return brList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BR list with id etab = [{idEtab}]", e);
            }
        }

        public List<DbBusinessReview> GetBusinessReviewsListByProcessing(int processing)
        {
            try
            {
                string selectCommand = "SELECT USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_ANSWERED, GOOGLE_REVIEW_ID, REVIEW_ANSWERED_GOOGLE_DATE, REVIEW_ANSWERED_DATE, REVIEW_ID, VISIT_DATE, REVIEW_GOOGLE_DATE, REVIEW_DATE, DATE_UPDATE, DATE_INSERT, DELETED, ID_ETAB FROM vBUSINESS_REVIEWS WHERE PROCESSING = @Processing";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Processing", processing);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbBusinessReview> brList = [];

                while (reader.Read())
                {
                    brList.Add(new DbBusinessReview(reader.GetString(16),
                        reader.GetString(9),
                        reader.GetString(6),
                        new GoogleUser(reader.IsDBNull(0) ? null : reader.GetString(0),
                            reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            !reader.IsDBNull(1) && reader.GetBoolean(1)),
                        reader.GetInt32(2),
                        reader.IsDBNull(4) ? null : reader.GetString(4),
                        reader.IsDBNull(11) ? null : reader.GetString(11),
                        reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                        reader.GetBoolean(5),
                        reader.IsDBNull(13) ? null : reader.GetDateTime(13),
                        reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        reader.IsDBNull(7) ? null : reader.GetString(7),
                        reader.IsDBNull(10) ? null : reader.GetString(10),
                        null,
                        reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                        !reader.IsDBNull(15) && reader.GetBoolean(15)
                        ));
                }
                return brList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BR list with processing = [{processing}]", e);
            }
        }

        /// <summary>
        /// Get list of Business Reviews.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business Reviews list or Null (doesn't exist)</returns>
        public List<DbBusinessReview> GetBusinessReviewsListWithDate(string idEtab, DateTime? dateLimit)
        {
            try
            {
                string selectCommand = "SELECT USER_NAME, USER_STATUS, SCORE, USER_NB_REVIEWS, REVIEW, REVIEW_ANSWERED, GOOGLE_REVIEW_ID, REVIEW_ANSWERED_GOOGLE_DATE, REVIEW_ANSWERED_DATE, REVIEW_ID, VISIT_DATE, REVIEW_GOOGLE_DATE, REVIEW_DATE, DATE_UPDATE, DATE_INSERT, DELETED FROM vBUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab AND REVIEW_DATE >= @DateLimit";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", idEtab);
                cmd.Parameters.AddWithValue("@DateLimit", dateLimit);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbBusinessReview> brList = [];

                while (reader.Read())
                {
                    brList.Add(new DbBusinessReview(idEtab,
                        reader.GetString(9),
                        reader.GetString(6),
                        new GoogleUser(reader.IsDBNull(0) ? null : reader.GetString(0),
                            reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            !reader.IsDBNull(1) && reader.GetBoolean(1)),
                        reader.GetInt32(2),
                        reader.IsDBNull(4) ? null : reader.GetString(4),
                        reader.IsDBNull(11) ? null : reader.GetString(11),
                        reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                        reader.GetBoolean(5),
                        reader.IsDBNull(13) ? null : reader.GetDateTime(13),
                        reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        reader.IsDBNull(7) ? null : reader.GetString(7),
                        reader.IsDBNull(10) ? null : reader.GetString(10),
                        null,
                        reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                        !reader.IsDBNull(15) && reader.GetBoolean(15)
                        ));
                }
                return brList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting BR list with id etab = [{idEtab}]", e);
            }
        }

        /// <summary>
        /// Get Business Review total.
        /// </summary>
        /// <returns>Total or null</returns>
        public int? GetBRTotal()
        {
            try
            {
                string selectCommand = "SELECT COUNT(1) FROM vBUSINESS_REVIEWS";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read() ? reader.GetInt32(0) : null;

            } catch (Exception e)
            {
                throw new Exception($"Error getting business review total", e);
            }
        }
        /// <summary>
        /// Get Business Review Feelings total.
        /// </summary>
        /// <returns>Total or null</returns>
        public int? GetBRFeelingsTotal()
        {
            try
            {
                string selectCommand = "SELECT COUNT(1) FROM BUSINESS_REVIEW_FEELING";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read() ? reader.GetInt32(0) : null;

            } catch (Exception e)
            {
                throw new Exception($"Error getting business reviews feelings total", e);
            }
        }

        public Dictionary<string, List<int>> GetKeywordThemeMap()
        {
            var map = new Dictionary<string, List<int>>();

            string sql = @"
        SELECT k.KEYWORD, k.THEME_ID
        FROM REVIEW_THEME_KEYWORD k
        JOIN REVIEW_THEME t ON t.THEME_ID = k.THEME_ID
        WHERE k.IS_ACTIVE = 1 AND t.IS_ACTIVE = 1";

            using var cmd = new SqlCommand(sql, Connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var keyword = ToolBox.Normalize(reader.GetString(0));
                var themeId = reader.GetInt32(1);

                if (!map.TryGetValue(keyword, out var list))
                {
                    list = [];
                    map[keyword] = list;
                }

                list.Add(themeId);
            }

            return map;
        }

        #endregion Get

        #region Update
        /// <summary>
        /// Update Business Review.
        /// </summary>
        /// <param name="review"></param>
        /// <param name="changeDate"></param>
        public void UpdateBusinessReview(DbBusinessReview review, bool changeDate)
        {
            try
            {
                string changeDateCommand = changeDate ? ", REVIEW_DATE_UPDATE = @ReviewDateUpdate, REVIEW_GOOGLE_DATE = @ReviewGoogleDate, REVIEW_DATE = @ReviewDate" : "";
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET USER_NAME = @UserName, USER_STATUS = @UserStatus, SCORE = @Score, USER_NB_REVIEWS = @UserNbReviews, REVIEW = @Review, REVIEW_ANSWERED = @ReviewAnswered, DATE_UPDATE = @DateUpdate, DELETED = @Deleted, VISIT_DATE = @VisitDate";
                string whereCommand = " WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand + changeDateCommand + whereCommand, Connection);
                cmd.Parameters.AddWithValue("@UserName", GetValueOrDefault(review.User.Name));
                cmd.Parameters.AddWithValue("@UserStatus", GetValueOrDefault(review.User.LocalGuide));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(review.Score));
                cmd.Parameters.AddWithValue("@UserNbReviews", GetValueOrDefault(review.User.NbReviews));
                cmd.Parameters.AddWithValue("@Review", GetValueOrDefault(review.ReviewText));
                cmd.Parameters.AddWithValue("@ReviewAnswered", GetValueOrDefault(review.ReviewReplied));
                cmd.Parameters.AddWithValue("@DateUpdate", GetValueOrDefault(review.DateUpdate));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(review.IdReview));
                cmd.Parameters.AddWithValue("@VisitDate", GetValueOrDefault(review.VisitDate));
                cmd.Parameters.AddWithValue("@Deleted", GetValueOrDefault(review.Deleted));
                if (changeDate)
                {
                    cmd.Parameters.AddWithValue("@ReviewDate", GetValueOrDefault(review.ReviewDate));
                    cmd.Parameters.AddWithValue("@ReviewDateUpdate", GetValueOrDefault(review.ReviewDate));
                    cmd.Parameters.AddWithValue("@ReviewGoogleDate", GetValueOrDefault(review.ReviewGoogleDate));
                }
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BR with id etab = [{review.IdEtab}] and id review = [{review.Id}]", e);
            }
        }        
        /// <summary>       
        /// Update Business Review.     
        /// </summary>     
        /// <param name="reviewId"></param>      
        /// <param name="deleted"></param>
        public void UpdateBusinessReviewDeleted(string reviewId, bool deleted = false)
        {
            try
            {
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET DELETED = @Deleted WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Deleted", GetValueOrDefault(deleted));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(reviewId));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BR with id review = [{reviewId}]", e);
            }
        }
        /// <summary>       
        /// Update Business Review.     
        /// </summary>     
        /// <param name="reviewId"></param>      
        /// <param name="processing"></param>
        public void UpdateBusinessReviewProcessing(string reviewId, int processing)
        {
            try
            {
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET PROCESSING = @Processing WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Processing", GetValueOrDefault(processing));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(reviewId));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BR with id review = [{reviewId}]", e);
            }
        }
        /// <summary>
        /// Update Business Review.
        /// </summary>
        /// <param name="review"></param>
        public void UpdateBusinessReviewReply(DbBusinessReview review)
        {
            try
            {
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET REVIEW_ANSWERED_GOOGLE_DATE = @ReplyGoogleDate, REVIEW_ANSWERED_DATE = @ReplyDate WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@ReplyDate", GetValueOrDefault(review.ReviewReplyDate));
                cmd.Parameters.AddWithValue("@ReplyGoogleDate", GetValueOrDefault(review.ReviewReplyGoogleDate));
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(review.IdReview));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BR with id etab = [{review.IdEtab}] and id review = [{review.Id}]", e);
            }
        }
        /// <summary>
        /// Update Business Review.
        /// </summary>
        /// <param name="idReview"></param>
        /// <param name="replied"></param>
        public void UpdateBusinessReviewReply(string idReview, bool replied)
        {
            try
            {
                string selectCommand = "UPDATE BUSINESS_REVIEWS SET REVIEW_ANSWERED = @Replied, REVIEW_ANSWERED_DATE = @Today, DATE_UPDATE = @Today WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Replied", replied);
                cmd.Parameters.AddWithValue("@Today", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@IdReview", idReview);
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error updating BR with id review = [{idReview}]", e);
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete Business Reviews.
        /// </summary>
        /// <param name="idEtab"></param>
        public void DeleteBusinessReviews(string idEtab)
        {
            try
            {
                string insertCommand = "DELETE FROM BUSINESS_REVIEWS WHERE ID_ETAB = @IdEtab";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdEtab", GetValueOrDefault(idEtab));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while deleting BR", e);
            }
        }
        /// <summary>
        /// Delete Business Review.
        /// </summary>
        /// <param name="idReview"></param>
        public void DeleteBusinessReview(string idReview)
        {
            try
            {
                string insertCommand = "DELETE FROM BUSINESS_REVIEWS WHERE REVIEW_ID = @IdReview";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@IdReview", GetValueOrDefault(idReview));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error while deleting BR withd id = [{idReview}]", e);
            }
        }
        /// <summary>
        /// Delete Business Reviews feeling.
        /// </summary>
        /// <param name="reviewId"></param>
        public void DeleteBusinessReviewsFeeling(string reviewId)
        {
            try
            {
                string insertCommand = "DELETE FROM BUSINESS_REVIEW_FEELING WHERE REVIEW_ID = @ReviewId";
                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@ReviewId", GetValueOrDefault(reviewId));
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception("Error while deleting BR feeling", e);
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
            } catch (Exception e)
            {
                throw new Exception($"Error while checking if BR exists with id etab = [{idEtab}] and id review = [{idReview}]", e);
            }
        }
        #endregion

        #endregion

        #region Order

        /// <summary>
        /// Get Order by Order Id.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>Order or null</returns>
        public DbOrder? GetOrderByID(int orderId)
        {
            try
            {
                string selectCommand = "SELECT * FROM [Order] WHERE id = @OrderId";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DbOrder order = new
                        (DateTime.Parse(reader["createdAt"].ToString()!),
                        DateTime.Parse(reader["updatedAt"].ToString()!),
                        reader["ownerId"].ToString()!,
                        (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["status"].ToString()!),
                        (StickerLanguage)Enum.Parse(typeof(StickerLanguage), reader["language"].ToString()!),
                        (int)decimal.Parse(reader["basePrice"].ToString()!),
                        reader["name"].ToString()!,
                        (int)decimal.Parse(reader["VATamount"].ToString()!),
                        (int)decimal.Parse(reader["VATprice"].ToString()!),
                        (int)decimal.Parse(reader["discount"].ToString()!),
                        (int)decimal.Parse(reader["priceNoVAT"].ToString()!),
                        (int)decimal.Parse(reader["priceWithVAT"].ToString()!)
                        );
                    return order;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting order for id = [{orderId}]", e);
            }
        }

        /// <summary>
        /// Get Order by Status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Order list</returns>
        public List<DbOrder> GetOrderByStatus(OrderStatus status)
        {
            try
            {
                string selectCommand = "SELECT * FROM [Order] WHERE status = @Status";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Status", status);
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbOrder> orders = new([]);

                while (reader.Read())
                {
                    DbOrder order = new
                        (DateTime.Parse(reader["createdAt"].ToString()!),
                        DateTime.Parse(reader["updatedAt"].ToString()!),
                        reader["ownerId"].ToString()!,
                        (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["status"].ToString()!),
                        (StickerLanguage)Enum.Parse(typeof(StickerLanguage), reader["language"].ToString()!),
                        (int)decimal.Parse(reader["basePrice"].ToString()!),
                        reader["name"].ToString()!,
                        (int)decimal.Parse(reader["VATamount"].ToString()!),
                        (int)decimal.Parse(reader["VATprice"].ToString()!),
                        (int)decimal.Parse(reader["discount"].ToString()!),
                        (int)decimal.Parse(reader["priceNoVAT"].ToString()!),
                        (int)decimal.Parse(reader["priceWithVAT"].ToString()!)
                        );
                    orders.Add(order);
                }
                return orders;

            } catch (Exception e)
            {
                throw new Exception($"Error getting order for status = [{status}]", e);
            }
        }

        /// <summary>
        /// Update Order Status.
        /// </summary>
        /// <param name="status"></param>
        public void UpdateOrderStatus(int id, OrderStatus status)
        {
            try
            {
                string selectCommand = "UPDATE [Order] SET Status = @Status WHERE Id = @Id";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Status", status.ToString());
                cmd.Parameters.AddWithValue("@Id", id);
                using SqlDataReader reader = cmd.ExecuteReader();
            } catch (Exception e)
            {
                throw new Exception($"Error updating order status with id = [{id}]", e);
            }
        }

        /// <summary>
        /// Get Place list from order id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>Place list</returns>
        public List<DbPlace> GetPlacesFromOrderId(int orderId)
        {
            try
            {
                string selectCommand = "SELECT * FROM Place pl join _OrderToPlace ordToPlace on ordToPlace.B = pl.id where ordToPlace.A = @OrderId";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbPlace> placeList = new([]);

                while (reader.Read())
                {
                    DbPlace place = new(
                        reader["id"].ToString()!,
                        reader["name"].ToString()!,
                        (reader["category"] != DBNull.Value) ? reader["category"].ToString() : null,
                        (reader["address"] != DBNull.Value) ? reader["address"].ToString() : null,
                        (reader["postCode"] != DBNull.Value) ? reader["postCode"].ToString() : null,
                        (reader["city"] != DBNull.Value) ? reader["city"].ToString() : null,
                        (reader["lat"] != DBNull.Value) ? Convert.ToDouble(reader["lat"]) : null,
                        (reader["long"] != DBNull.Value) ? Convert.ToDouble(reader["long"]) : null,
                        (reader["nationalPhoneNumber"] != DBNull.Value) ? reader["nationalPhoneNumber"].ToString() : null,
                        (reader["internationalPhoneNumber"] != DBNull.Value) ? reader["internationalPhoneNumber"].ToString() : null,
                        (reader["website"] != DBNull.Value) ? reader["website"].ToString() : null,
                        (reader["plusCode"] != DBNull.Value) ? reader["plusCode"].ToString() : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["businessStatus"].ToString()!),
                        (reader["country"] != DBNull.Value) ? reader["country"].ToString() : null,
                        reader["url"].ToString()!,
                        (reader["score"] != DBNull.Value) ? Convert.ToDouble(reader["score"]) : null,
                        (reader["nbReviews"] != DBNull.Value) ? int.Parse(reader["nbReviews"].ToString()!) : null
                        );

                    placeList.Add(place);
                }

                return placeList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting place list for order id = [{orderId}]", e);
            }

        }

        /// <summary>
        /// Get Place by place id
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Place</returns>
        public DbPlace? GetPlaceByPlaceId(string placeId)
        {
            try
            {
                string selectCommand = "SELECT * FROM Place WHERE Id = @placeId";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@placeId", placeId);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbPlace> placeList = new([]);

                if (reader.Read())
                {
                    DbPlace place = new(
                        reader["id"].ToString()!,
                        reader["name"].ToString()!,
                        (reader["category"] != DBNull.Value) ? reader["category"].ToString() : null,
                        (reader["address"] != DBNull.Value) ? reader["address"].ToString() : null,
                        (reader["postCode"] != DBNull.Value) ? reader["postCode"].ToString() : null,
                        (reader["city"] != DBNull.Value) ? reader["city"].ToString() : null,
                        (reader["lat"] != DBNull.Value) ? Convert.ToDouble(reader["lat"]) : null,
                        (reader["long"] != DBNull.Value) ? Convert.ToDouble(reader["long"]) : null,
                        (reader["nationalPhoneNumber"] != DBNull.Value) ? reader["nationalPhoneNumber"].ToString() : null,
                        (reader["internationalPhoneNumber"] != DBNull.Value) ? reader["internationalPhoneNumber"].ToString() : null,
                        (reader["website"] != DBNull.Value) ? reader["website"].ToString() : null,
                        (reader["plusCode"] != DBNull.Value) ? reader["plusCode"].ToString() : null,
                        (BusinessStatus)Enum.Parse(typeof(BusinessStatus), reader["businessStatus"].ToString()!),
                        (reader["country"] != DBNull.Value) ? reader["country"].ToString() : null,
                        reader["url"].ToString()!,
                        (reader["score"] != DBNull.Value) ? Convert.ToDouble(reader["score"]) : null,
                        (reader["nbReviews"] != DBNull.Value) ? int.Parse(reader["nbReviews"].ToString()!) : null,
                        (reader["dateInsert"] != DBNull.Value) ? DateTime.Parse(reader["dateInsert"].ToString()!) : null,
                        (reader["dateUpdate"] != DBNull.Value) ? DateTime.Parse(reader["dateUpdate"].ToString()!) : null
                        );

                    return place;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting place for id = [{placeId}]", e);
            }

        }

        #endregion

        #region Stickers
        /// <summary>
        /// Create Sticker.
        /// </summary>
        /// <param name="sticker"></param>
        public int CreateSticker(DbSticker sticker)
        {
            try
            {
                string insertCommand = @"
                INSERT INTO Stickers (placeId, score, createdDate, image, orderId, certificate, nbRating1, nbRating2, nbRating3, nbRating4, nbRating5) 
                VALUES (@PlaceId, @Score, @CreatedDate, @Image, @OrderId, @Certificate, @NbRating1, @NbRating2, @NbRating3, @NbRating4, @NbRating5);
                SELECT SCOPE_IDENTITY();";

                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", GetValueOrDefault(sticker.PlaceId));
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(sticker.Score));
                cmd.Parameters.AddWithValue("@CreatedDate", GetValueOrDefault(sticker.CreatedDate));
                cmd.Parameters.AddWithValue("@OrderId", GetValueOrDefault(sticker.OrderId));
                cmd.Parameters.AddWithValue("@NbRating1", GetValueOrDefault(sticker.NbRating1));
                cmd.Parameters.AddWithValue("@NbRating2", GetValueOrDefault(sticker.NbRating2));
                cmd.Parameters.AddWithValue("@NbRating3", GetValueOrDefault(sticker.NbRating3));
                cmd.Parameters.AddWithValue("@NbRating4", GetValueOrDefault(sticker.NbRating4));
                cmd.Parameters.AddWithValue("@NbRating5", GetValueOrDefault(sticker.NbRating5));

                cmd.Parameters.Add("@Image", SqlDbType.VarBinary).Value = (object?)sticker.Image ?? DBNull.Value;
                cmd.Parameters.Add("@Certificate", SqlDbType.VarBinary).Value = (object?)sticker.Certificate ?? DBNull.Value;

                return Convert.ToInt32(cmd.ExecuteScalar());
            } catch (Exception e)
            {
                throw new Exception($"Error creating sticker with id = [{sticker.PlaceId}]", e);
            }
        }

        /// <summary>
        /// Create Sticker.
        /// </summary>
        /// <param name="sticker"></param>
        public int CreateStickerNetwork(DbStickerNetwork sticker)
        {
            try
            {
                string insertCommand = @"
                INSERT INTO StickersNetwork (score, createdDate, brandName, nbEtab, nbReview, geoZone, year, image, certificate) 
                VALUES (@Score, @CreatedDate, @BrandName, @NbEtab, @NbReview, @GeoZone, @Year, @Image, @Certificate);
                SELECT SCOPE_IDENTITY();";

                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Score", GetValueOrDefault(sticker.Score));
                cmd.Parameters.AddWithValue("@CreatedDate", GetValueOrDefault(sticker.CreatedDate));
                cmd.Parameters.AddWithValue("@BrandName", GetValueOrDefault(sticker.BrandName));
                cmd.Parameters.AddWithValue("@NbEtab", GetValueOrDefault(sticker.NbEtab));
                cmd.Parameters.AddWithValue("@NbReview", GetValueOrDefault(sticker.NbReview));
                cmd.Parameters.AddWithValue("@GeoZone", GetValueOrDefault(sticker.GeoZone));
                cmd.Parameters.AddWithValue("@Year", GetValueOrDefault(sticker.Year));

                cmd.Parameters.Add("@Image", SqlDbType.VarBinary).Value = (object?)sticker.Image ?? DBNull.Value;
                cmd.Parameters.Add("@Certificate", SqlDbType.VarBinary).Value = (object?)sticker.Certificate ?? DBNull.Value;

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(cmd.ExecuteScalar());
            } catch (Exception e)
            {
                throw new Exception($"Error creating sticker with brand name = [{sticker.BrandName}]", e);
            }
        }

        /// <summary>
        /// Get Sticker network by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Place</returns>
        public DbStickerNetwork? GetStickerNetworkById(string id)
        {
            try
            {
                string selectCommand = "SELECT * FROM StickersNetwork WHERE Id = @Id";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbPlace> placeList = new([]);

                if (reader.Read())
                {
                    DbStickerNetwork sticker = new(
                        Convert.ToDouble(reader["score"]),
                        DateTime.Parse(reader["createdDate"].ToString()!),
                        (reader["image"] != DBNull.Value) ? (byte[])reader["image"] : null,
                        (reader["certificate"] != DBNull.Value) ? (byte[])reader["certificate"] : null,
                        int.Parse(reader["nbEtab"].ToString()!),
                        int.Parse(reader["nbReview"].ToString()!),
                        int.Parse(reader["year"].ToString()!),
                        reader["brandName"].ToString()!,
                        reader["geoZone"].ToString()!,
                        int.Parse(reader["id"].ToString()!)
                        );

                    return sticker;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting sticker for id = [{id}]", e);
            }

        }

        /// <summary>
        /// Create Sticker.
        /// </summary>
        /// <param name="sticker"></param>
        public void UpdateSticker(DbSticker sticker)
        {
            try
            {
                string insertCommand = "UPDATE Stickers SET IMAGE = @Image, CERTIFICATE = @Certificate WHERE ID = @Id";

                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Id", GetValueOrDefault(sticker.Id));
                cmd.Parameters.Add("@Image", SqlDbType.VarBinary).Value = (object?)sticker.Image ?? DBNull.Value;
                cmd.Parameters.Add("@Certificate", SqlDbType.VarBinary).Value = (object?)sticker.Certificate ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error creating sticker with id = [{sticker.PlaceId}]", e);
            }
        }

        /// <summary>
        /// Create Sticker.
        /// </summary>
        /// <param name="sticker"></param>
        public void UpdateStickerNetwork(DbStickerNetwork sticker)
        {
            try
            {
                string insertCommand = "UPDATE StickersNetwork SET IMAGE = @Image, CERTIFICATE = @Certificate WHERE ID = @Id";

                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@Id", GetValueOrDefault(sticker.Id));
                cmd.Parameters.Add("@Image", SqlDbType.VarBinary).Value = (object?)sticker.Image ?? DBNull.Value;
                cmd.Parameters.Add("@Certificate", SqlDbType.VarBinary).Value = (object?)sticker.Certificate ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error creating sticker with brand name = [{sticker.BrandName}]", e);
            }
        }

        /// <summary>
        /// Delete Sticker by Place Id and Year.
        /// </summary>
        /// <param name="placeId"></param>
        /// <param name="year"></param>
        public void DeleteStickerByPlaceIdAndYear(string placeId, int year)
        {
            try
            {
                string deleteCommand = "DELETE FROM STICKERS WHERE PLACE_ID = @PlaceId AND YEAR = @Year";
                using SqlCommand cmd = new(deleteCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", placeId);
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error deleting sticker for place id = [{placeId}] and year = [{year}]", e);
            }
        }

        public DbUserVasanoIO? GetVasanoIOUser(string id)
        {
            try
            {
                string selectCommand = "SELECT * FROM Users WHERE Id = @Id";

                using SqlCommand cmd = new(selectCommand, Connection);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.CommandTimeout = 10000;
                using SqlDataReader reader = cmd.ExecuteReader();

                List<DbPlace> placeList = new([]);

                if (reader.Read())
                {
                    DbUserVasanoIO user = new(
                        reader["firstName"].ToString()!,
                        reader["lastName"].ToString()!,
                        reader["email"].ToString()!,
                        id,
                        reader["companyCountry"].ToString()!
                        );

                    return user;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Error getting user with id = [{id}]", e);
            }
        }
        #endregion

        #region Error Table
        /// <summary>
        /// Create Error.
        /// </summary>
        /// <param name="placeId"></param>
        /// <param name="year"></param>
        /// <param name="message"></param>
        public void CreateError(string placeId, int year, string message)
        {
            try
            {
                string insertCommand = "INSERT INTO ERROR (PLACE_ID, YEAR, MESSAGE) VALUES (@PlaceId, @Year, @Message)";

                using SqlCommand cmd = new(insertCommand, Connection);
                cmd.Parameters.AddWithValue("@PlaceId", placeId);
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.ExecuteNonQuery();
            } catch (Exception e)
            {
                throw new Exception($"Error creating error with place id = [{placeId}] and year = [{year}]", e);
            }
        }
        #endregion
    }
}