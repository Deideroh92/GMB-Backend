using GMS.Sdk.Core.DbModels;
using GMS.Sdk.Core.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using SeleniumExtras.WaitHelpers;
using GMS.Sdk.Core;
using System.Reflection.PortableExecutable;

namespace GMS.Business.Agent
{
    /// <summary>
    /// Business Service.
    /// </summary>
    public class BusinessService {

        public static readonly string pathLogFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Logs\Business-Agent\log-" + DateTime.Today.ToString("MM-dd-yyyy-HH-mm-ss") + ".txt";
        public static readonly string pathOperationIsFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\processed_file_" + DateTime.Today.ToString("MM-dd-yyyy-HH-mm-ss");

        #region Local

        /// <summary>
        /// Start the Service.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="threadNumber"></param>
        /// <exception cref="Exception"></exception>
        public static async Task StartAsync(BusinessAgentRequest request, int? threadNumber = 0) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            using DbLib db = new();
            using SeleniumDriver driver = new();

            int count = 0 ;

            DateTime time = DateTime.UtcNow;

            foreach (DbBusinessAgent business in request.BusinessList) {
                try {

                    ToolBox.BreakingHours();

                    count++;

                    // Get business profile infos from Google.
                    (DbBusinessProfile? profile, DbBusinessScore? score) = await GetBusinessProfileAndScoreFromGooglePageAsync(driver, business.Url, business.Guid, business.IdEtab);

                    if (request.Operation == Operation.FILE) {
                        if (profile == null) {
                            using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                            operationFileWritter.WriteLine(business.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + "0" + "$$" + "0" + "$$" + "0" + "$$" + driver.WebDriver.Url);
                            continue;
                        }
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(driver.WebDriver.Url))) {
                            using DbBusinessUrl businessUrl = new(profile.FirstGuid, driver.WebDriver.Url, DateTime.UtcNow, "file", DateTime.UtcNow, ToolBox.ComputeMd5Hash(driver.WebDriver.Url), UrlState.UPDATED);
                            db.CreateBusinessUrl(businessUrl);
                        }
                    }
                    
                    // No business found at this url.
                    if (profile == null) {
                        if (request.Operation == Operation.URL_STATE && business.Guid != null)
                            db.DeleteBusinessUrlByGuid(business.Guid);
                        else
                            if (business.IdEtab != null) {
                            db.UpdateBusinessProfileStatus(business.IdEtab, BusinessStatus.DELETED);
                            db.UpdateBusinessProfileProcessingState(business.IdEtab, 0);
                        }     
                        continue;
                    }
                    
                    // Update or insert Business Profile if exist or not.
                    if (request.Operation == Operation.CATEGORY || db.CheckBusinessProfileExist(profile.IdEtab))
                        db.UpdateBusinessProfile(profile);
                    else
                        db.CreateBusinessProfile(profile);

                    // Insert Business Score if have one.
                    if (score?.Score != null)
                        db.CreateBusinessScore(score);
                    
                    // Getting reviews if option checked.s
                    if (request.GetReviews && request.DateLimit != null && score?.Score != null)
                        GetReviews(profile, request.DateLimit, db, driver);

                    // Update Url state when finished.
                    if (request.Operation == Operation.URL_STATE)
                        db.UpdateBusinessUrlState(profile.FirstGuid, UrlState.UPDATED);

                    // Update Business State when finished
                    db.UpdateBusinessProfileProcessingState(profile.IdEtab, 0);

                    if (request.Operation == Operation.FILE) {
                        using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                        operationFileWritter.WriteLine(business.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + profile.Name + "$$" + profile.GoogleAddress + "$$" + profile.IdEtab + "$$" + driver.WebDriver.Url);
                    }

                    } catch (Exception e) {
                    if (e.Message != "Couldn't sort") {
                        using StreamWriter sw = File.AppendText(pathLogFile);
                        sw.WriteLine(DateTime.UtcNow.ToString("G") + " - " + business.Url);
                        sw.WriteLine("Message : " + e.Message);
                        sw.WriteLine("Stack : " + e.StackTrace);
                        sw.WriteLine("\n");
                    }
                }
            }

            using StreamWriter sw2 = File.AppendText(pathLogFile);
            sw2.WriteLine(DateTime.UtcNow.ToString("G") + " - Thread number " + threadNumber + " finished.");
            sw2.WriteLine("Treated " + count + " businesses in " + (DateTime.UtcNow - time).ToString("g") + ".\n");
        }

        #region Profile & Score
        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        /// <param name="guid"></param>
        /// <param name="idEtab"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        /// <exception cref="Exception"></exception>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> GetBusinessProfileAndScoreFromGooglePageAsync(SeleniumDriver driver, string url, string? guid = null, string? idEtab = null) {
            // Initialization of all variables
            int? reviews = null;
            string? address = null;
            string? postCode = null;
            string? city = null;
            string? cityCode = null;
            float? lat = null;
            float? lon = null;
            string? idBan = null;
            string? addressType = null;
            string? streetNumber = null;
            BusinessStatus status = BusinessStatus.OPEN;

            driver.GetToPage(url);

            string? category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category)?.Text?.Replace("·", "").Trim();
            string? googleAddress = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress)?.GetAttribute("aria-label")?.Replace("Adresse:", "")?.Trim();
            string? img = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test)?.GetAttribute("src")?.Trim();
            string? tel = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel)?.GetAttribute("aria-label")?.Replace("Numéro de téléphone:", "")?.Trim();
            string? website = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website)?.GetAttribute("href")?.Trim();
            float? parsedScore = null;
            float? score = null;
            string? name = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim();
            if (name == null)
                return (null, null);
            else
                Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),\s-]+|\s{2,}", "");

            if (ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score) != null && float.TryParse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label")?.Replace("étoiles", "")?.Trim(), out float parsedScoreValue))
                parsedScore = parsedScoreValue;

            score ??= parsedScore ?? float.Parse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelScore)?.Text ?? "0");


            string? status_tmp = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.status)?.Text.Trim();
            if (status_tmp != null) {
                if (status_tmp.Contains("Fermé définitivement") || status_tmp.Contains("Définitivement fermé"))
                    status = BusinessStatus.CLOSED;
                if (status_tmp.Contains("Fermé temporairement"))
                    status = BusinessStatus.TEMPORARLY_CLOSED;
            }

            if (ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews) is WebElement nbReviewsElement) {
                string? label = nbReviewsElement.GetAttribute("aria-label")?.Replace("avis", "").Replace(" ", "").Trim();
                if (label == null && score != null)
                    label = nbReviewsElement.ComputedAccessibleLabel.Replace("avis", "").Replace(" ", "").Trim();
                if (label != null && int.TryParse(Regex.Replace(label, @"\s", ""), out int parsedReviews))
                    reviews = parsedReviews;
            }

            if (googleAddress != null) {
                AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(googleAddress);
                if (addressResponse != null)
                {
                    lon = (float?)(addressResponse.Features[0]?.Geometry?.Coordinates[0]);
                    lat = (float?)(addressResponse.Features[0]?.Geometry?.Coordinates[1]);
                    city = addressResponse.Features[0]?.Properties?.City;
                    postCode = addressResponse.Features[0]?.Properties?.Postcode;
                    cityCode = addressResponse.Features[0]?.Properties?.CityCode;
                    address = addressResponse.Features[0]?.Properties?.Street;
                    addressType = addressResponse.Features[0]?.Properties?.PropertyType;
                    idBan = addressResponse.Features[0]?.Properties?.Id;
                    streetNumber = addressResponse.Features[0]?.Properties?.HouseNumber;
                }
            }

            idEtab ??= ToolBox.ComputeMd5Hash(name + googleAddress);
            guid ??= Guid.NewGuid().ToString("N");
            DbBusinessProfile dbBusinessProfile = new(idEtab, guid, name, category, googleAddress, address, postCode, city, cityCode, lat, lon, idBan, addressType, streetNumber, tel, website, DateTime.UtcNow, status, img);
            DbBusinessScore? dbBusinessScore = new(idEtab, score, reviews);
            return (dbBusinessProfile, dbBusinessScore);
        }
        #endregion

        #region Reviews
        /// <summary>
        /// Getting all reviews
        /// </summary>
        /// <param name="reviewWebElement"></param>
        /// <param name="idEtab"></param>
        /// <param name="dateLimit"></param>
        /// <returns>Business Review</returns>
        /// <exception cref="Exception"></exception>
        public static DbBusinessReview? GetBusinessReviewsInfosFromReviews(IWebElement reviewWebElement, string idEtab, DateTime? dateLimit) {
            string idReview = reviewWebElement.GetAttribute("data-review-id")?.Trim() ?? throw new Exception("No review id");

            string? reviewGoogleDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.googleDate)?.Text?.Trim();
            DateTime reviewDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);

            if (reviewGoogleDate == null || (dateLimit.HasValue && reviewDate < dateLimit.Value))
                return null;

            string userName = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userName)?.GetAttribute("aria-label")?.Replace("Photo de", "").Trim() ?? "";

            int reviewScore = ToolBox.FindElementsSafe(reviewWebElement, XPathReview.score).Count;

            int userNbReviews = 1;
            string? userNbReviewsText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)?.Text?.Replace("avis", "").Replace("·", "").Replace("Local Guide", "").Replace(" ", "").Trim();
            if (!string.IsNullOrEmpty(userNbReviewsText) && int.TryParse(userNbReviewsText, out int parsedUserNbReviews)) {
                userNbReviews = parsedUserNbReviews;
            }

            string? reviewText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.text)?.Text?.Replace("\n", "").Replace("(Traduit par google)", "").Trim();
            if (reviewText != null && reviewText.EndsWith(" Plus")) {
                try {
                    var test = ToolBox.FindElementSafe(reviewWebElement, XPathReview.plusButton);
                    test.Click();
                    Thread.Sleep(1000);
                    reviewText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.text)?.Text?.Replace("\n", "").Replace("(Traduit par google)", "").Trim();
                } catch {
                    reviewText = reviewText.TrimEnd(" Plus".ToCharArray());
                }
            }
                
            bool localGuide = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)) && ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews).Text.Contains('·');
            GoogleUser user = new(userName, userNbReviews, localGuide);

            bool replied = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyText));

            return new DbBusinessReview(idEtab, idReview, user, reviewScore, reviewText, reviewGoogleDate, reviewDate, replied, DateTime.UtcNow);
        }


        /// <summary>
        /// Scroll to get all review list.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="dateLimit"></param>
        /// <returns>Review list</returns>
        public static ReadOnlyCollection<IWebElement>? GetReviewsFromGooglePage(IWebDriver driver, DateTime? dateLimit) {
            ReadOnlyCollection<IWebElement>? reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

            if (reviewList == null || reviewList.Count == 0) {
                return null;
            }

            IWebElement? scrollingPanel = ToolBox.FindElementSafe(driver, XPathReview.scrollingPanel);

            try {
                IWebElement parent = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].parentNode.parentNode.parentNode;", scrollingPanel);

                int reviewListLength = 0;
                string? reviewGoogleDate;
                DateTime realDate;

                while (reviewListLength != reviewList.Count) {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", parent);
                    Thread.Sleep(2000);
                    reviewListLength = reviewList.Count;
                    reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

                    if (reviewList == null) {
                        break;
                    }

                    reviewGoogleDate = ToolBox.FindElementSafe(reviewList.Last(), XPathReview.googleDate)?.Text?.Trim();
                    if (reviewGoogleDate != null)
                    {
                        realDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);
                        if (realDate < dateLimit)
                        {
                            break;
                        }
                    }
                }
            } catch (Exception e) {
                if (e.Message.Contains("javascript error: Cannot read properties of null (reading 'parentNode')")) {
                    return null;
                } else {
                    throw new Exception("Error occurred while getting reviews from the Google page.", e);
                }
            }

            return reviewList;
        }


        /// <summary>
        /// Sort reviews in ascending date order.
        /// </summary>
        /// <param name="driver"></param>
        /// <exception cref="Exception"></exception>
        public static void SortReviews(IWebDriver driver) {
            try {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));

                IWebElement sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews)));
                sortButton.Click();

                IWebElement sortButton2 = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews2)));
                sortButton2.Click();
            } catch(Exception) {
                throw new Exception("Couldn't sort");
            }
        }

        /// <summary>
        /// Getting reviews for business.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <param name="dateLimit"></param>
        /// <param name="db"></param>
        /// <param name="driver"></param>
        /// <exception cref="Exception"></exception>
        public static void GetReviews(DbBusinessProfile businessProfile, DateTime? dateLimit, DbLib db, SeleniumDriver driver) {

            try
            {
                WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(2));
                IWebElement toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)));
                toReviewPage.Click();
            }
            catch (Exception) { return; }

            Thread.Sleep(2000);

            // Sorting reviews.
            SortReviews(driver.WebDriver);

            Thread.Sleep(1000);

            // Getting reviews.
            ReadOnlyCollection<IWebElement>? reviews = GetReviewsFromGooglePage(driver.WebDriver, dateLimit);

            if (reviews == null) {
                return;
            }

            foreach (IWebElement review in reviews) {
                try {
                    DbBusinessReview? businessReview = GetBusinessReviewsInfosFromReviews(review, businessProfile.IdEtab, dateLimit);
                    if (businessReview == null)
                        continue;
                    DbBusinessReview? dbBusinessReview = db.GetBusinessReview(businessProfile.IdEtab, businessReview.IdReview);

                    if (dbBusinessReview == null) {
                        db.CreateBusinessReview(businessReview);
                        continue;
                    }

                    if (dbBusinessReview.ReviewText == "") dbBusinessReview.ReviewText = null;

                    db.UpdateBusinessReview(businessReview);

                    /*if (!businessReview.Equals(dbBusinessReview)) {
                        db.UpdateBusinessReview(businessReview);
                        continue;
                    }*/
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        #endregion

        #endregion
    }
}