using System.Globalization;
using GMB.Sdk.Core;
using Serilog;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Business.Api.Models;
using OpenQA.Selenium;
using GMB.Url.Api;
using System.Diagnostics.Metrics;
using GMB.Business.Api.API;
using System.Linq;
using AngleSharp.Dom;

namespace GMB.Business.Api.Controllers
{
    /// <summary>
    /// Business Agent Controller.
    /// </summary>
    public class BusinessController {
        public static readonly string pathOperationIsFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\processed_file_" + DateTime.Today.ToString("MM-dd-yyyy-HH-mm-ss");
        private static readonly string logsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Business.Agent\logs\log";

        #region Scanner
        /// <summary>
        /// Start the Scanner.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="threadNumber"></param>
        public static async Task Scanner(BusinessAgentRequest request, int? threadNumber = 0) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            using DbLib db = new();
            using SeleniumDriver driver = new();

            int count = 0;

            DateTime time = DateTime.UtcNow;

            foreach (BusinessAgent businessAgent in request.BusinessList) {
                try {
                    ToolBox.BreakingHours();

                    count++;
                    GetBusinessProfileRequest BPRequest = new(businessAgent.Url, businessAgent.Guid, businessAgent.IdEtab);
                    DbBusinessProfile? business = null;

                    if (businessAgent.IdEtab != null)
                        business = db.GetBusinessByIdEtab(businessAgent.IdEtab);

                    // Get business profile infos from Google.
                    (DbBusinessProfile? profile, DbBusinessScore? score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, BPRequest, business);

                    if (request.Operation == Operation.FILE) {
                        if (profile == null) {
                            using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                            operationFileWritter.WriteLine(businessAgent.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + "0" + "$$" + "0" + "$$" + "0" + "$$" + driver.WebDriver.Url);
                            continue;
                        }
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(driver.WebDriver.Url))) {
                            DbBusinessUrl businessUrl = new(profile.FirstGuid, driver.WebDriver.Url, "file", ToolBox.ComputeMd5Hash(driver.WebDriver.Url), UrlState.UPDATED);
                            db.CreateBusinessUrl(businessUrl);
                        }
                    }
                    
                    // No business found at this url.
                    if (profile == null) {
                        if (request.Operation == Operation.URL_STATE && businessAgent.Guid != null)
                            db.DeleteBusinessUrlByGuid(businessAgent.Guid);
                        else {
                            if (businessAgent.IdEtab != null) {
                                db.UpdateBusinessProfileStatus(businessAgent.IdEtab, BusinessStatus.DELETED);
                                db.UpdateBusinessProfileProcessingState(businessAgent.IdEtab, 0);
                            }
                        }   
                        continue;
                    }

                    if (business == null)
                        db.CreateBusinessProfile(profile);
                    if (business != null && !profile.Equals(business))
                        db.UpdateBusinessProfile(profile);

                    // Insert Business Score if have one.
                    if (score?.Score != null)
                        db.CreateBusinessScore(score);

                    // Check if it's an hotel.
                    bool isHotel = ToolBox.FindElementSafe(driver.WebDriver, new() { By.XPath("//div[text() = 'VÉRIFIER LA DISPONIBILITÉ']") })?.Text == "VÉRIFIER LA DISPONIBILITÉ";

                    // Getting reviews
                    if (request.GetReviews && request.DateLimit != null && score?.Score != null && !isHotel) {
                        driver.GetToPage(BPRequest.Url);
                        List<DbBusinessReview>? reviews = BusinessServiceApi.GetReviews(profile.IdEtab, request.DateLimit, driver);

                        if (reviews != null)
                        {
                            foreach (DbBusinessReview review in reviews)
                            {
                                try
                                {
                                    DbBusinessReview? dbBusinessReview = db.GetBusinessReview(profile.IdEtab, review.IdReview);

                                    if (dbBusinessReview == null)
                                    {
                                        db.CreateBusinessReview(review);
                                        continue;
                                    }

                                    if (!review.Equals(dbBusinessReview))
                                    {
                                        db.UpdateBusinessReview(review);
                                        continue;
                                    }
                                } catch (Exception e)
                                {
                                    Log.Error($"Couldn't treat a review : {e.Message}", e);
                                }
                            }
                        }
                    }

                    // Update Url state when finished.
                    if (request.Operation == Operation.URL_STATE)
                        db.UpdateBusinessUrlState(profile.FirstGuid, UrlState.UPDATED);

                    // Update Business State when finished
                    db.UpdateBusinessProfileProcessingState(profile.IdEtab, 0);

                    if (request.Operation == Operation.FILE) {
                        using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                        operationFileWritter.WriteLine(businessAgent.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + profile.Name + "$$" + profile.GoogleAddress + "$$" + profile.IdEtab + "$$" + driver.WebDriver.Url);
                    }
                }
                catch (Exception e) {
                    Log.Error(e, $"An exception occurred on BP with id etab = [{businessAgent.IdEtab}], guid = [{businessAgent.Guid}], url = [{businessAgent.Url}] : {e.Message}");
                }
            }

            Log.Information(DateTime.UtcNow.ToString("G") + " - Thread number " + threadNumber + " finished.");
            Log.Information("Treated " + count + " businesses in " + (DateTime.UtcNow - time).ToString("g") + ".\n");
            Log.CloseAndFlush();
        }
        #endregion

        #region Profile & Score
        /// <summary>
        /// Get Google Business Profile and Score by given url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Business Profile and Score as a Tuple</returns>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> ScanGMBByUrl(string url) {
            try {
                using SeleniumDriver driver = new();

                return await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url), null);
            } catch (Exception e) {
                return (null, null);
            }
        }
        /// <summary>
        /// Get Google Business Profile and Score by id etab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business Profile and Score as a Tuple</returns>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> ScanGMBByIdEtab(string idEtab) {
            try {
                using DbLib db = new();
                using SeleniumDriver driver = new();

                BusinessAgent? business = db.GetBusinessAgentByIdEtab(idEtab);

                if (business == null) {
                    return (null, null);
                }

                GetBusinessProfileRequest request = new(business.Url, business.Guid, business.IdEtab);

                return await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            } catch (Exception e) {
                return (null, null);
            }
        }
        /// <summary>
        /// Create a new business url and business if it doesn't exist already.
        /// </summary>
        /// <param name="url"></param>
        public static async void CreateNewBusinessProfileByUrl(string url)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();
            try {
                DbBusinessUrl? businessUrl = UrlController.CreateUrl(url);

                if (businessUrl == null) { 
                    return;
                }

                using SeleniumDriver driver = new();

                GetBusinessProfileRequest request = new(businessUrl.Url, businessUrl.Guid, null);
                (DbBusinessProfile? profile, DbBusinessScore? score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
                DbLib db = new();

                // Update or insert Business Profile if exist or not.
                if (db.CheckBusinessProfileExist(profile.IdEtab))
                    db.UpdateBusinessProfile(profile);
                else
                    db.CreateBusinessProfile(profile);

                // Insert Business Score if have one.
                if (score?.Score != null)
                    db.CreateBusinessScore(score);
            } catch (Exception e) {
                Log.Error(e, $"An exception occurred while inserting new BU and BP with url = [{url}] : {e.Message}");
            }
        }
        /// <summary>
        /// Find business by query (should be name + address) then insert it in DB if new.
        /// </summary>
        /// <param name="query"></param>
        public static async Task CreateNewBusinessByQueryFromGoogleApi(string query)
        {
            string? placeId = await GoogleApi.GetPlaceId(query);
            if (placeId == null)
            {
                return;
            }
            using DbLib db = new();
            string idEtab = ToolBox.ComputeMd5Hash(placeId);

            if (db.CheckBusinessProfileExist(idEtab))
            { 
                return;
            }

            PlaceDetailsResponse? placeDetails = await GoogleApi.GetGMB(placeId);
            if (placeDetails == null || placeDetails.Result.Url == null)
            { 
                return;
            }

            DbBusinessUrl? businessUrl = db.GetBusinessUrlByUrlEncoded(ToolBox.ComputeMd5Hash(placeDetails.Result.Url));

            businessUrl ??= UrlController.CreateUrl(placeDetails.Result.Url);

            DbBusinessProfile? profile = new(
                placeDetails.Result.PlaceId,
                idEtab,
                businessUrl.Guid,
                placeDetails.Result.Name,
                placeDetails.Result.Types?.FirstOrDefault(),
                placeDetails.Result.FormattedAdress, placeDetails.Result.FormattedAdress,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("postal_code")).LongName,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("locality")).LongName,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("route")).LongName,
                (float?)placeDetails.Result.Geometry.Location.Latitude,
                (float?)placeDetails.Result.Geometry.Location.Longitude,
                null,
                null,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("street_number")).LongName,
                null,
                placeDetails.Result.FormattedPhoneNumber,
                placeDetails.Result.Website,
                placeDetails.Result.PlusCode.GlobalCode,
                null,
                (BusinessStatus)Enum.Parse(typeof(BusinessStatus), placeDetails.Result.BusinessStatus.ToString()),
                null,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("country")).LongName,
                placeDetails.Result.Geometry.Location.Latitude.ToString() + ", " + placeDetails.Result.Geometry.Location.Longitude.ToString());
            ;
            return;
        }
        #endregion

        #region Reviews
        /// <summary>
        /// Get Business Reviews list from url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dateLimit"></param>
        /// <param name="idEtab"></param>
        /// <returns>List of business reviews</returns>
        public static List<DbBusinessReview>? GetBusinessReviews(string url, DateTime? dateLimit, string idEtab = "-1") {
            using SeleniumDriver driver = new();

            driver.GetToPage(url);
            try {
                return BusinessServiceApi.GetReviews(idEtab, dateLimit, driver);
            }
            catch (Exception e) {
                return null;
            }
        }
        #endregion

    }
}