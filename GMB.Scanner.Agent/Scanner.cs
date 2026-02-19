using GMB.BusinessService.Api.Models;
using GMB.Scanner.Agent.Core;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.ScannerService;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using Serilog;
using System.Globalization;
using System.Net;
using System.Linq;
using iText.Layout.Element;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using OpenQA.Selenium.DevTools.V118.Network;

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

            string basePath = AppContext.BaseDirectory; // ou AppDomain.CurrentDomain.BaseDirectory
            string filePath = Path.Combine(basePath, "Files", "CustomTheme.txt");

            List<string> idEtabForTheme = [];

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        idEtabForTheme.Add(line);
                }
            }

            using DbLib db = new();
            SeleniumDriver driver = new(request.Number);

            int count = 0;

            DateTime time = DateTime.UtcNow;

            Dictionary<string, List<int>> themes = db.GetKeywordThemeMap();

            foreach (BusinessAgent businessAgent in request.BusinessList)
            {
                try
                {
                    ToolBox.BreakingHours();

                    count++;
                    GetBusinessProfileRequest BPRequest = new(businessAgent.Url, businessAgent.Guid, businessAgent.IdEtab);
                    DbBusinessProfile? business = null;

                    List<DbBusinessReview> reviewList = new([]);

                    if (businessAgent.IdEtab != null)
                        business = db.GetBusinessByIdEtab(businessAgent.IdEtab);

                    if (!driver.IsDriverAlive() || count == 300)
                    {
                        driver.Dispose();
                        driver = new(request.Number);
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

                    if (profile.PlaceId == null && business != null && business.PlaceId != null)
                        profile.PlaceId = business.PlaceId;

                    business ??= db.GetBusinessByIdEtab(profile.IdEtab);

                    if (business == null && profile.PlaceId != null)
                    {
                        business ??= db.GetBusinessByPlaceId(profile.PlaceId);
                        if (business != null)
                        {
                            profile.IdEtab = business.IdEtab;
                            score.IdEtab = business.IdEtab;
                        }
                    }
                        

                    if (business == null)
                        db.CreateBusinessProfile(profile);

                    if (business != null && !profile.Equals(business))
                    {
                        //exception PAUL
                        if (business.GoogleAddress == null || ((business.GoogleAddress != profile.GoogleAddress) && !business.GoogleAddress.Contains("99999")))
                            db.UpdateBusinessProfile(profile);
                        else
                            db.UpdateBusinessProfileWithoutAddress(profile);
                    }

                    // Insert Business Score if have one.
                    if (score?.Score != null)
                        db.CreateBusinessScore(score);

                    if (request.GetPhotos)
                    {
                        try
                        {
                            WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(10));

                            driver.GetToPage(businessAgent.Url);

                            IWebElement toImagePage = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(@jsaction, 'heroHeaderImage')]")));
                            toImagePage.Click();

                            Thread.Sleep(1000);

                            IReadOnlyCollection<IWebElement> links = driver.WebDriver.FindElements(By.XPath("//a[@href and @data-photo-index and contains(@jsaction, 'gallery.main') and contains(@aria-label, 'Photo')]"));

                            bool isOwner = false;

                            if (links.Count == 0)
                            {
                                IWebElement card = driver.WebDriver.FindElement(By.XPath("//div[@role='navigation' and @tabindex='-1' and @jsaction='focus:titlecard.main']"));

                                try
                                {
                                    // Wait for the new content to load (adjust the condition as needed for your application)
                                    wait.Until(ExpectedConditions.ElementExists(By.XPath("//a[contains(@data-attribution-url, '//maps.google.com/maps/contrib/')]")));
                                    var cardName = ToolBox.FindElementSafe(card, [By.XPath("//a[contains(@data-attribution-url, '//maps.google.com/maps/contrib/')]")]);

                                    // Retrieve the aria-label attribute
                                    string OwnerName = WebUtility.HtmlDecode(cardName.GetAttribute("aria-label"));

                                    if (OwnerName.Contains(profile.Name))
                                        isOwner = true;
                                    else
                                        isOwner = false;
                                } catch (Exception)
                                {
                                    isOwner = false;
                                }
                                db.CreateBusinessPhoto(new(business.IdEtab, "Only one photo", isOwner, DateTime.UtcNow));
                            } else
                            {

                                DateTime dateInsert = DateTime.UtcNow;

                                IWebElement parent = (IWebElement)((IJavaScriptExecutor)driver.WebDriver).ExecuteScript("return arguments[0].parentNode.parentNode.parentNode.parentNode;", links.First());

                                foreach (IWebElement link in links)
                                {
                                    try
                                    {
                                        try
                                        {
                                            link.Click();
                                        } catch (Exception)
                                        {
                                            ((IJavaScriptExecutor)driver.WebDriver).ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", parent);
                                            Thread.Sleep(1000);
                                            link.Click();
                                        }

                                        Thread.Sleep(1000);

                                        IWebElement card = driver.WebDriver.FindElement(By.XPath("//div[@role='navigation' and @tabindex='-1' and @jsaction='focus:titlecard.main']"));

                                        try
                                        {
                                            // Wait for the new content to load (adjust the condition as needed for your application)
                                            wait.Until(ExpectedConditions.ElementExists(By.XPath("//a[contains(@data-attribution-url, '//maps.google.com/maps/contrib/')]")));
                                            var cardName = ToolBox.FindElementSafe(card, [By.XPath("//a[contains(@data-attribution-url, '//maps.google.com/maps/contrib/')]")]);

                                            // Retrieve the aria-label attribute
                                            string OwnerName = WebUtility.HtmlDecode(cardName.GetAttribute("aria-label"));

                                            if (OwnerName.Contains(profile.Name))
                                                isOwner = true;
                                            else
                                                isOwner = false;
                                        } catch (Exception)
                                        {
                                            isOwner = false;
                                        }

                                        var divElement = link.FindElement(By.XPath(".//div[@role='img']"));

                                        string styleAttribute = divElement.GetAttribute("style");

                                        // Extract the URL from the background-image property
                                        var regex = new System.Text.RegularExpressions.Regex(@"background-image:\s*url\(\""(.*?)\""\)");
                                        var match = regex.Match(styleAttribute);

                                        string? photoUrl = match.Success
                                            ? WebUtility.HtmlDecode(match.Groups[1].Value)
                                            : null;

                                        if (photoUrl == null)
                                            continue;
                                        db.CreateBusinessPhoto(new(business.IdEtab, photoUrl, isOwner, dateInsert));

                                        // Re-locate the links to avoid StaleElementReferenceException
                                        links = driver.WebDriver.FindElements(By.XPath("//a[@href and @data-photo-index and contains(@jsaction, 'pane.gallery.main')]"));
                                    } catch (Exception ex)
                                    {
                                        Console.WriteLine($"An error occurred: {ex.Message}");
                                    }
                                }
                            }
                            } catch (Exception)
                        {
                            db.CreateBusinessPhoto(new(business.IdEtab, "Only one photo", false, DateTime.UtcNow));
                        }
                    }

                    // Getting reviews
                    if (request.GetReviews && request.DateLimit != null && score?.Score != null)
                    {
                        try
                        {
                            driver.GetToPage(BPRequest.Url);
                            List<DbBusinessReview>? reviews = ScannerFunctions.GetReviews(business?.IdEtab ?? profile.IdEtab, request.DateLimit, driver);

                            if (reviews != null)
                            {
                                foreach (DbBusinessReview review in reviews)
                                {
                                    try
                                    {
                                        DbBusinessReview? dbBusinessReview = db.GetBusinessReview(review.IdReview);

                                        if (dbBusinessReview == null)
                                        {
                                            review.LastSeenAt = time;
                                            db.CreateBusinessReview(review);
                                            continue;
                                        } else
                                        {
                                            if ((dbBusinessReview.ReviewReplyGoogleDate == null || dbBusinessReview.ReviewReplyDate == null) && review.ReviewReplied)
                                                db.UpdateBusinessReviewReply(review);

                                            if (!review.Equals(dbBusinessReview))
                                            {
                                                db.UpdateBusinessReview(review, (dbBusinessReview.Score != review.Score) || (dbBusinessReview.ReviewText != review.ReviewText));
                                            }

                                            db.UpdateBusinessReviewLastSeen(review.IdReview, time);

                                            if (dbBusinessReview.Deleted == true)
                                            {
                                                db.UpdateBusinessReviewDeleted(review.IdReview, false);
                                            }
                                        }


                                        if (!string.IsNullOrWhiteSpace(review.ReviewText) && idEtabForTheme.Contains(business?.IdEtab ?? profile.IdEtab))
                                        {
                                            HashSet<int> themesFound =
                                                ToolBox.DetectThemes(review.ReviewText, themes);

                                            if (themesFound.Count > 0 && !db.CheckBusinessReviewThemeMatchExist(review.IdReview))
                                                db.InsertThemeMatches(review.IdReview, themesFound);
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
                                    driver = new(request.Number);
                                    count = 1;
                                }
                            }

                            if (request.CheckReviewStatus && request.DateLimit.HasValue)
                            {
                                // DateLimit = "il y a 2 mois" dans ton cas mensuel
                                DateTime dateLimit = request.DateLimit.Value;

                                // On récupère tous les avis du business dans la période (>= dateLimit)
                                // qui n'ont PAS été vus pendant ce run (LastSeenAt < scanStart)
                                List<DbBusinessReview> deletedCandidates =
                                    db.GetBusinessReviewsNotSeenSinceInPeriod(profile.IdEtab, dateLimit, time);

                                foreach (DbBusinessReview review in deletedCandidates)
                                {
                                    db.UpdateBusinessReviewDeleted(review.IdReview, true);
                                }
                            }

                        } catch (Exception e)
                        {
                            Log.Error(e, $"An exception occurred when getting reviews from id etab = [{businessAgent.IdEtab}], guid = [{businessAgent.Guid}], url = [{businessAgent.Url}] : {e.Message}");
                            driver.Dispose();
                            driver = new(request.Number);
                        }
                    }

                    // Update Url state when finished.
                    if (request.Operation == Operation.URL_STATE)
                        db.UpdateBusinessUrlState(profile.FirstGuid, UrlState.UPDATED);

                    // Update Business State when finished
                    if (request.UpdateProcessingState)
                    {
                        if (business.Processing == 8) db.UpdateBusinessProfileProcessingState(profile.IdEtab, 9);
                        else db.UpdateBusinessProfileProcessingState(profile.IdEtab, 0);
                    }

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