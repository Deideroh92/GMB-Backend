using GMB.BusinessService.Api.Models;
using GMB.Scanner.Agent.Core;
using GMB.Scanner.Agent.Models;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Serilog;
using System.Globalization;

namespace GMB.Scanner.Agent
{
    public class Scanner
    {
        public static readonly string pathOperationIsFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\processed_file_" + DateTime.Today.ToString("MM-dd-yyyy-HH-mm-ss");
        private static readonly string logsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Business.Agent\logs\log";

        public static async Task BusinessScanner(ScannerBusinessRequest request)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            using DbLib db = new();
            SeleniumDriver driver = new();
            ScannerFunctions scanner = new();

            int count = 0;

            DateTime time = DateTime.UtcNow;

            foreach (BusinessAgent businessAgent in request.BusinessList)
            {
                try
                {
                    ToolBox.BreakingHours();

                    count++;
                    GetBusinessProfileRequest BPRequest = new(businessAgent.Url, businessAgent.Guid, businessAgent.IdEtab);
                    DbBusinessProfile? business = null;

                    if (businessAgent.IdEtab != null)
                        business = db.GetBusinessByIdEtab(businessAgent.IdEtab);

                    if (!driver.IsDriverAlive())
                    {
                        driver.Dispose();
                        driver = new();
                    }

                    // Get business profile infos from Google.
                    (DbBusinessProfile? profile, DbBusinessScore? score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, BPRequest, business);

                    // No business found at this url.
                    if (profile == null)
                    {
                        if (request.Operation == Operation.URL_STATE && businessAgent.Guid != null)
                            db.DeleteBusinessUrlByGuid(businessAgent.Guid);
                        else
                        {
                            if (businessAgent.IdEtab != null)
                            {
                                db.UpdateBusinessProfileStatus(businessAgent.IdEtab, BusinessStatus.DELETED);
                                db.UpdateBusinessProfileProcessingState(businessAgent.IdEtab, 0);
                            }
                        }
                        continue;
                    }

                    business ??= db.GetBusinessByIdEtab(profile.IdEtab);

                    if (business == null)
                        db.CreateBusinessProfile(profile);
                    if (business != null && !profile.Equals(business))
                        db.UpdateBusinessProfile(profile);

                    // Insert Business Score if have one.
                    if (score?.Score != null)
                        db.CreateBusinessScore(score);

                    // Getting reviews
                    if (request.GetReviews && request.DateLimit != null && score?.Score != null && profile.Category != "Hébergement")
                    {
                        driver.GetToPage(BPRequest.Url);
                        List<DbBusinessReview>? reviews = ScannerFunctions.GetReviews(profile.IdEtab, request.DateLimit, driver);

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

                                    if ((dbBusinessReview.ReviewReplyGoogleDate == null || dbBusinessReview.ReviewReplyDate == null) && review.ReviewReplied)
                                        db.UpdateBusinessReviewReply(review);

                                    if (!review.Equals(dbBusinessReview))
                                    {
                                        db.UpdateBusinessReview(review, (dbBusinessReview.Score != review.Score) || dbBusinessReview.ReviewText != review.ReviewText);
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
                    if (request.UpdateProcessingState)
                        db.UpdateBusinessProfileProcessingState(profile.IdEtab, 0);

                } catch (Exception e)
                {
                    Log.Error(e, $"An exception occurred on BP with id etab = [{businessAgent.IdEtab}], guid = [{businessAgent.Guid}], url = [{businessAgent.Url}] : {e.Message}");
                }
            }
            Log.CloseAndFlush();
            driver.Dispose();
        }

        /// <summary>
        /// Start the URL Scanner.
        /// </summary>
        /// <param name="request"></param>
        public static void ScannerUrl(ScannerUrlRequest request)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            using DbLib db = new();
            using SeleniumDriver driver = new();
            List<string>? urls = [];
            ScannerFunctions scanner = new();

            foreach (string location in request.Locations)
            {
                ToolBox.BreakingHours();

                try
                {
                    string textSearch = request.TextSearch + "+" + location;
                    string url = "https://www.google.com/maps/search/" + textSearch;
                    urls = scanner.GetUrlsFromGooglePage(driver, url);

                    if (urls == null)
                        continue;

                    foreach (string urlToValidate in urls)
                    {
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(urlToValidate)))
                        {
                            DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), urlToValidate, textSearch, ToolBox.ComputeMd5Hash(urlToValidate));
                            db.CreateBusinessUrl(businessUrl);
                        }
                    }
                } catch (Exception e)
                {
                    Log.Error(e, $"An exception occurred while searching for business urls with search: [{request.TextSearch + "+" + location}] : {e.Message}");
                }
            }
            Log.CloseAndFlush();
        }
    }
}