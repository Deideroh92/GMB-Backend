using GMB.BusinessService.Api.Models;
using GMB.Scanner.Agent.Core;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.ScannerService;
using Serilog;
using System.Globalization;

namespace GMB.Scanner.Agent
{
    public class Scanner
    {
        private static readonly string logsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.ScannerService.Api\logs\log";

        public static async Task BusinessScanner(ScannerBusinessParameters request)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            using DbLib db = new();
            SeleniumDriver driver = new();

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

                    if (!driver.IsDriverAlive() || count == 300)
                    {
                        driver.Dispose();
                        driver = new();
                        count = 1;
                    }

                    // Get business profile infos from Google.
                    (DbBusinessProfile? profile, DbBusinessScore? score) = await ScannerFunctions.GetBusinessProfileAndScoreFromGooglePageAsync(driver, BPRequest, business);

                    // No business found at this url.
                    if (profile == null)
                    {
                        if (request.Operation == Operation.URL_STATE && businessAgent.Guid != null)
                            db.DeleteBusinessUrlByGuid(businessAgent.Guid);
                        else
                        {
                            if (businessAgent.IdEtab != null)
                            {
                                if (business == null || (business != null & business.Status != BusinessStatus.DELETED))
                                    db.UpdateBusinessProfileStatus(businessAgent.IdEtab, BusinessStatus.DELETED);
                                
                                if (request.UpdateProcessingState)
                                    db.UpdateBusinessProfileProcessingState(businessAgent.IdEtab, 0);
                            }
                        }
                        continue;
                    }

                    business ??= db.GetBusinessByIdEtab(profile.IdEtab);

                    if (business == null)
                        db.CreateBusinessProfile(profile);
                    if (business != null && !profile.Equals(business))
                    {
                        if (business.GoogleAddress != profile.GoogleAddress)
                            db.UpdateBusinessProfile(profile);
                        else
                            db.UpdateBusinessProfileWithoutAddress(profile);
                    } 

                    // Insert Business Score if have one.
                    if (score?.Score != null)
                        db.CreateBusinessScore(score);

                    // Getting reviews
                    if (request.GetReviews && request.DateLimit != null && score?.Score != null)
                    {
                        try
                        {
                            driver.GetToPage(BPRequest.Url);
                            List<DbBusinessReview>? reviews = ScannerFunctions.GetReviews(profile.IdEtab, request.DateLimit, driver);

                            if (reviews != null)
                            {
                                foreach (DbBusinessReview review in reviews)
                                {
                                    try
                                    {
                                        DbBusinessReview? dbBusinessReview = db.GetBusinessReview(review.IdReview);

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
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error($"Couldn't treat a review : {e.Message}", e);
                                    }
                                }

                                if (reviews.Count > 300)
                                {
                                    driver.Dispose();
                                    driver = new();
                                    count = 1;
                                }
                            }
                        } catch (Exception e)
                        {
                            Log.Error(e, $"An exception occurred when getting reviews from id etab = [{businessAgent.IdEtab}], guid = [{businessAgent.Guid}], url = [{businessAgent.Url}] : {e.Message}");
                            driver.Dispose();
                            driver = new();
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
            driver.Dispose();
            Log.CloseAndFlush(); 
        }

        /// <summary>
        /// Start the URL Scanner.
        /// </summary>
        /// <param name="request"></param>
        public static void UrlScanner(ScannerUrlParameters request)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

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
                    urls = ScannerFunctions.GetUrlsFromGooglePage(driver, url);

                    if (urls == null)
                        continue;

                    foreach (string urlToValidate in urls)
                    {
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(urlToValidate)))
                        {
                            DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), urlToValidate, textSearch);
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