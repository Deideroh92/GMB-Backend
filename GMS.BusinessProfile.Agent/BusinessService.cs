using AngleSharp.Dom;
using GMS.BusinessProfile.Agent.Model;
using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Sdk.Core.XPath;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GMS.Business.Agent {
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
        /// <exception cref="Exception"></exception>
        public static void Start(BusinessAgentRequest request, int? threadNumber = 0) {
            DbLib db = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            SeleniumDriver driver = new(DriverType.CHROME);

            int count = 0 ;

            DateTime time = DateTime.UtcNow;

            foreach (DbBusinessAgent business in request.BusinessList) {
                try {
                    count++;
                    float percentage = (count / request.BusinessList.Count) * 100;

                    // Get business profile infos from Google.
                    (DbBusinessProfile? businessProfile, DbBusinessScore? businessScore) = GetBusinessProfileAndScoreFromGooglePage(driver, business.Url, business.Guid, business.IdEtab);

                    if (request.Operation == Operation.FILE) {
                        if (businessProfile == null) {
                            using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                            operationFileWritter.WriteLine(business.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + "0" + "$$" + "0" + "$$" + "0" + "$$" + driver.WebDriver.Url);
                            continue;
                        }
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(driver.WebDriver.Url))) {
                            DbBusinessUrl businessUrl = new(businessProfile.FirstGuid, driver.WebDriver.Url, DateTime.UtcNow, UrlState.UPDATED, "file", DateTime.UtcNow, ToolBox.ComputeMd5Hash(driver.WebDriver.Url));
                            db.CreateBusinessUrl(businessUrl);
                        }
                    }

                    // No business found at this url.
                    if (businessProfile == null) {
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
                    if (request.Operation == Operation.CATEGORY || db.CheckBusinessProfileExist(businessProfile.IdEtab))
                        db.UpdateBusinessProfile(businessProfile);
                    else
                        db.CreateBusinessProfile(businessProfile);

                    // Insert Business Score if have one.
                    if (businessScore.Score != null)
                        db.CreateBusinessScore(businessScore);

                    // Getting reviews if option checked.
                    if (request.GetReviews && request.DateLimit != null)
                        GetReviews(businessProfile, request.DateLimit, db, driver);

                    // Update Url state when finished.
                    if (request.Operation == Operation.URL_STATE)
                        db.UpdateBusinessUrlState(businessProfile.FirstGuid, UrlState.UPDATED);

                    // Update Business State when finished
                    db.UpdateBusinessProfileProcessingState(businessProfile.IdEtab, 0);

                    if (request.Operation == Operation.FILE) {
                        using StreamWriter operationFileWritter = File.AppendText(pathOperationIsFile + threadNumber.ToString() + ".txt");
                        operationFileWritter.WriteLine(business.Url.Replace("https://www.google.fr/maps/search/", "") + "$$" + businessProfile.Name + "$$" + businessProfile.Adress + "$$" + businessProfile.IdEtab + "$$" + driver.WebDriver.Url);
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

            driver.WebDriver.Quit();
            db.DisconnectFromDB();

            using StreamWriter sw2 = File.AppendText(pathLogFile);
            sw2.WriteLine(DateTime.UtcNow.ToString("G") + " - Thread number " + threadNumber + " finished.");
            sw2.WriteLine("Treated " + count + " businesses in " + (DateTime.UtcNow - time).ToString("g") + ".\n");
        }

        #region Profile & Score

        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="guid"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        /// <exception cref="Exception"></exception>
        public static (DbBusinessProfile?, DbBusinessScore?) GetBusinessProfileAndScoreFromGooglePage(SeleniumDriver driver, string url, string? guid = null, string? idEtab = null, bool isHotel = false) {
            // Initialization of all variables
            string? name = null;
            string? category = null;
            string? adress = null;
            int? reviews = null;
            string? website = null;
            string? tel = null;
            float? score = null;
            string? img = null;
            BusinessStatus status = BusinessStatus.OPEN;
            string? geoloc = null;
            bool hotel = isHotel;

            driver.GetToPage(url);

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name))) {
                name = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name).GetAttribute("aria-label").Trim();
                Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),-]+", "");
                Regex.Replace(name, @"\s{2,}", " ");
            }

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.status))) {
                string status_tmp = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.status).Text.Trim();

                if (status_tmp.Contains("Fermé définitivement"))
                    status = BusinessStatus.CLOSED;

                if (status_tmp.Contains("Définitivement fermé"))
                    status = BusinessStatus.CLOSED;

                if (status_tmp.Contains("Fermé temporairement"))
                    status = BusinessStatus.TEMPORARLY_CLOSED;
            }

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category)))
                category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category).Text.Trim();

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress)))
                adress = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress).GetAttribute("aria-label").Replace("Adresse:", "").Trim();

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress)))
                adress = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress).GetAttribute("aria-label").Replace("Adresse:", "").Trim();

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test)))
                img = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test).GetAttribute("src").Trim();

            try {
                if (float.TryParse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label").Replace("étoiles", "").Trim(), out _))
                   score = float.Parse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label").Replace("étoiles", "").Trim());

                if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews))) {
                    try {
                        if (int.TryParse(Regex.Replace(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews).GetAttribute("aria-label").Replace(" avis", "").Trim(), @"\s", ""), out _)) {
                            reviews = int.Parse(Regex.Replace(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews).GetAttribute("aria-label").Replace("avis", "").Replace(" ", "").Trim(), @"\s", ""));
                        }
                    } catch (Exception) { }
                    try {
                        if (int.TryParse(Regex.Replace(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews).Text.Replace(" avis", "").Trim(), @"\s", ""), out _))
                            reviews = int.Parse(Regex.Replace(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews).Text.Replace("avis", "").Replace(" ", "").Trim(), @"\s", ""));
                    } catch (Exception) { }
                    try {
                        if (int.TryParse(Regex.Replace(((WebElement)ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews)).ComputedAccessibleLabel.Replace("avis", "").Replace(" ", "").Trim(), @"\s", ""), out _))
                            reviews = int.Parse(Regex.Replace(((WebElement)ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews)).ComputedAccessibleLabel.Replace("avis", "").Replace(" ", "").Trim(), @"\s", ""));
                    } catch (Exception) { }
                }
                    
            } catch (Exception) { }

            //HOTEL
            if (hotel) {
                if (category == null && ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelCategory)))
                    category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelCategory).Text.Replace("·", "");

                if (score == null && reviews != null && ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelScore)))
                    score = float.Parse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelScore).Text);
            }

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel)))
                tel = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel).GetAttribute("aria-label").Replace("Numéro de téléphone:", "").Trim();
            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website)))
                website = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website).GetAttribute("href").Trim();

            if (name == null) {
                return (null, null);
            }

            idEtab ??= ToolBox.ComputeMd5Hash(name + adress);
            guid ??= Guid.NewGuid().ToString("N");
            DbBusinessProfile dbBusinessProfile = new(idEtab, guid, name, category, adress, tel, website, geoloc, DateTime.UtcNow, DateTime.UtcNow, status, img);
            DbBusinessScore? dbBusinessScore = new(idEtab, score, reviews, DateTime.UtcNow);
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
        public static DbBusinessReview GetBusinessReviewsInfosFromReviews(IWebElement reviewWebElement, string idEtab, DateTime? dateLimit) {

            // User
            string? userName = null;
            bool localGuide = false;
            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)))
                localGuide = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews).Text.Contains('·');
            int userNbReviews;

            // Review
            DbBusinessReview businessReview;
            string idReview;
            int reviewScore;
            string? reviewText = null;
            string? reviewGoogleDate;
            DateTime reviewDate;

            // Review reply
            bool replied = true;
            string replyGoogleDate;
            string replyText;
            DbBusinessReviewReply? businessReviewReply = null;


            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.googleDate))) {
                reviewGoogleDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.googleDate).Text.Trim();
                reviewDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);
                if (reviewDate < dateLimit)
                    throw new Exception("Review too old.");
            }
            else
                throw new Exception("No date for this review");

            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userName)))
                userName = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userName).GetAttribute("aria-label").Replace("Photo de", "").Trim();

            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.score))) {
                reviewScore = ToolBox.FindElementsSafe(reviewWebElement, XPathReview.score).Count();
            } else
                throw new Exception("No score for this review");

            try {
                userNbReviews = int.Parse(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews).Text.Replace("avis", "").Replace("·", "").Replace("Local Guide", "").Replace(" ", "").Trim());
            } catch (Exception) { 
                userNbReviews = 1;
            }

            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.text)))
                reviewText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.text).Text.Replace("\n", "").Replace("(Traduit par google)", "").Trim();

            try {
                idReview = reviewWebElement.GetAttribute("data-review-id").Trim();
            } catch (Exception) {
                throw new Exception("No review id");
            }

            GoogleUser user = new(userName, userNbReviews, localGuide);
            
            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyText))) {
                replyText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyText).Text.Replace("\n", "").Trim();
                replyGoogleDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyGoogleDate).Text.Replace("\n", "").Trim();
                DateTime replyDate = ToolBox.ComputeDateFromGoogleDate(replyGoogleDate);
                businessReviewReply = new(replyText, idReview, replyGoogleDate, replyDate, DateTime.UtcNow, DateTime.UtcNow);
            }
            else
                replied = false;

            businessReview = new(idEtab, idReview, user, reviewScore, reviewText, reviewGoogleDate, reviewDate, replied, DateTime.UtcNow, DateTime.UtcNow, businessReviewReply);

            return businessReview;
        }

        /// <summary>
        /// Scroll to get all review list.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="dateLimit"></param>
        /// <returns>Review list</returns>
        public static ReadOnlyCollection<IWebElement>? GetReviewsFromGooglePage(IWebDriver driver, DateTime? dateLimit) {
            ReadOnlyCollection<IWebElement>? reviewList;
            string reviewGoogleDate;
            int reviewListLength = 0;

            if (!ToolBox.Exists(ToolBox.FindElementsSafe(driver, XPathReview.reviewList)))
                throw new Exception("Failed to get review list.");

            reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

            if (reviewList == null || reviewList.Count == 0)
                return null;

            IWebElement? scrollingPanel = ToolBox.FindElementSafe(driver, XPathReview.scrollingPanel);

            try {
                IWebElement parent = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].parentNode.parentNode.parentNode;", scrollingPanel);
                while (reviewListLength != reviewList.Count) {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", parent);
                    Thread.Sleep(2000);
                    reviewListLength = reviewList.Count;
                    reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);
                    if (ToolBox.Exists(ToolBox.FindElementsSafe(driver, XPathReview.googleDate))) {
                        var test = reviewList.Last();
                        reviewGoogleDate = ToolBox.FindElementSafe(test, XPathReview.googleDate).Text.Trim();
                        if (ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate) < dateLimit)
                            break;
                    }
                };
            } catch (Exception e) {
                if (e.Message.Contains("javascript error: Cannot read properties of null (reading 'parentNode')"))
                    return null;
                else
                    throw new Exception();
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
                ToolBox.FindElementSafe(driver, XPathReview.sortReviews).Click();
                Thread.Sleep(1000);
                ToolBox.FindElementSafe(driver, XPathReview.sortReviews2).Click();
                Thread.Sleep(1000);
            } catch(Exception) {
                throw new Exception("Couldn't sort");
            }
        }

        /// <summary>
        /// Getting reviews for business.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <param name="dateLimit"></param>
        /// <exception cref="Exception"></exception>
        public static void GetReviews(DbBusinessProfile businessProfile, DateTime? dateLimit, DbLib db, SeleniumDriver driver) {
            if (!ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)))
                return;

            ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage).Click();
            Thread.Sleep(2500);

            // Sorting reviews.
            SortReviews(driver.WebDriver);

            // Getting reviews.
            ReadOnlyCollection<IWebElement>? reviews = GetReviewsFromGooglePage(driver.WebDriver, dateLimit);

            if (reviews == null)
                return;

            foreach (IWebElement review in reviews) {
                try {
                    DbBusinessReview businessReview = GetBusinessReviewsInfosFromReviews(review, businessProfile.IdEtab, dateLimit);
                    if (!db.CheckBusinessReviewExist(businessProfile.IdEtab, businessReview.IdReview))
                        db.CreateBusinessReview(businessReview);
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