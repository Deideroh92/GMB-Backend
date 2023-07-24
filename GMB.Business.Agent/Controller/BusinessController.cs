using System.Globalization;
using GMB.Sdk.Core;
using Serilog;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Business.Api.Models;
using OpenQA.Selenium;

namespace GMB.Business.Api.Controllers
{
    /// <summary>
    /// Business Agent Controller.
    /// </summary>
    public class BusinessController {
        public static readonly string pathOperationIsFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\processed_file_" + DateTime.Today.ToString("MM-dd-yyyy-HH-mm-ss");
        private static readonly string logsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Business.Agent\logs\log";

        public static void LogTest()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            Log.Error("This is an error");
            Log.Information("This is an info");
        }

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

            foreach (BusinessAgent business in request.BusinessList) {
                try {

                    ToolBox.BreakingHours();

                    count++;
                    GetBusinessProfileRequest BPRequest = new(business.Url, business.Guid, business.IdEtab);

                    // Get business profile infos from Google.
                    (DbBusinessProfile? profile, DbBusinessScore? score) = await BusinessService.GetBusinessProfileAndScoreFromGooglePageAsync(driver, BPRequest);

                    continue;

                    if (request.Operation == Operation.FILE) {
                        if (profile == null) {
                            using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                            operationFileWritter.WriteLine(business.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + "0" + "$$" + "0" + "$$" + "0" + "$$" + driver.WebDriver.Url);
                            continue;
                        }
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(driver.WebDriver.Url))) {
                            DbBusinessUrl businessUrl = new(profile.FirstGuid, driver.WebDriver.Url, "file", DateTime.UtcNow, ToolBox.ComputeMd5Hash(driver.WebDriver.Url), UrlState.UPDATED);
                            db.CreateBusinessUrl(businessUrl);
                        }
                    }
                    
                    // No business found at this url.
                    if (profile == null) {
                        if (request.Operation == Operation.URL_STATE && business.Guid != null)
                            db.DeleteBusinessUrlByGuid(business.Guid);
                        else {
                            if (business.IdEtab != null) {
                                db.UpdateBusinessProfileStatus(business.IdEtab, BusinessStatus.DELETED);
                                db.UpdateBusinessProfileProcessingState(business.IdEtab, 0);
                            }
                        }   
                        continue;
                    }
                    
                    // Update or insert Business Profile if exist or not.
                    if (request.Operation == Operation.OTHER || db.CheckBusinessProfileExist(profile.IdEtab))
                        db.UpdateBusinessProfile(profile);
                    else
                        db.CreateBusinessProfile(profile);

                    // Insert Business Score if have one.
                    if (score?.Score != null)
                        db.CreateBusinessScore(score);

                    // Check if it's an hotel.
                    bool isHotel = ToolBox.FindElementSafe(driver.WebDriver, new() { By.XPath("//div[text() = 'VÉRIFIER LA DISPONIBILITÉ']") })?.Text == "VÉRIFIER LA DISPONIBILITÉ";

                    // Getting reviews
                    if (request.GetReviews && request.DateLimit != null && score?.Score != null && !isHotel) {
                        List<DbBusinessReview>? reviews = BusinessService.GetReviews(profile.IdEtab, request.DateLimit, driver);

                        foreach (DbBusinessReview review in reviews) {
                            try {
                                DbBusinessReview? dbBusinessReview = db.GetBusinessReview(profile.IdEtab, review.IdReview);

                                if (dbBusinessReview == null) {
                                    db.CreateBusinessReview(review);
                                    continue;
                                }

                                if (dbBusinessReview.ReviewText == "")
                                    dbBusinessReview.ReviewText = null;

                                db.UpdateBusinessReview(review);

                                if (!review.Equals(dbBusinessReview)) {
                                    db.UpdateBusinessReview(review);
                                    continue;
                                }
                            } catch (Exception e) {
                                Log.Error($"Couldn't treat a review : {e.Message}", e);
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
                        operationFileWritter.WriteLine(business.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + profile.Name + "$$" + profile.GoogleAddress + "$$" + profile.IdEtab + "$$" + driver.WebDriver.Url);
                    }
                }
                catch (Exception e) {
                    Log.Error(e, $"An exception occurred on BP with id etab = [{business.IdEtab}], guid = [{business.Guid}], url = [{business.Url}] : {e.Message}");
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
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> GetGoogleBusinessProfileAndScoreByUrl(string url) {
            try {
                using DbLib db = new();
                using SeleniumDriver driver = new();

                return await BusinessService.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url));
            } catch (Exception e) {
                return (null, null);
            }
        }
        /// <summary>
        /// Get Google Business Profile and Score by id etab.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <returns>Business Profile and Score as a Tuple</returns>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> GetGoogleBusinessProfileAndScoreByIdEtab(string idEtab) {
            try {
                using DbLib db = new();
                using SeleniumDriver driver = new();

                BusinessAgent? business = db.GetBusinessByIdEtab(idEtab);

                if (business == null) {
                    return (null, null);
                }

                GetBusinessProfileRequest request = new(business.Url, business.Guid, business.IdEtab);

                return await BusinessService.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request);
            } catch (Exception e) {
                return (null, null);
            }
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
                return BusinessService.GetReviews(idEtab, dateLimit, driver);
            }
            catch (Exception e) {
                return null;
            }
        }
        #endregion

    }
}