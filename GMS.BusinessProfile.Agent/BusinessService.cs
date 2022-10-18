using GMS.BusinessProfile.Agent.Model;
using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Sdk.Core.XPath;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GMS.Business.Agent {
    /// <summary>
    /// Business Service.
    /// </summary>
    public class BusinessService {

        #region Local

        /// <summary>
        /// Start the Service.
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="Exception"></exception>
        public static void Start(BusinessAgentRequest request) {

            string log = @"D:\Projects\VASANO\C#\Logs\log.txt";

            DbLib dbLib = new();

            List<DbBusinessAgent>? businessList = request.BusinessList;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            if (request.UrlState != null) {
                foreach (DbBusinessAgent business in businessList)
                    dbLib.UpdateBusinessUrlState(business.Guid, UrlState.PROCESSING);
            }

            foreach (DbBusinessAgent business in businessList)
                dbLib.UpdateBusinessProfileProcessingState(business.IdEtab, true);

            SeleniumDriver driver = new(DriverType.CHROME);

            foreach (DbBusinessAgent business in businessList) {
                try {

                    // Get business profile infos from Google.
                    (DbBusinessProfile? businessProfile, DbBusinessScore? businessScore) = GetBusinessProfileFromGooglePage(driver, business.Url, business.Guid, business.IdEtab);

                    // No business found at this url.
                    if (businessProfile == null) {
                        if (business.IdEtab != null)
                            dbLib.UpdateBusinessProfileStatus(business.IdEtab, BusinessStatus.DELETED);
                        else
                            dbLib.DeleteUrlByGuid(business.Guid);
                        continue;
                    }

                    // Update or insert Business Profile if exist or not.
                    if (request.Category != null || dbLib.CheckBusinessProfileExist(businessProfile.IdEtab))
                        dbLib.UpdateBusinessProfile(businessProfile);
                    else 
                        dbLib.InsertBusinessProfile(businessProfile);

                    // Update Url state to updated when finished.
                    if (request.UrlState != null)
                        dbLib.UpdateBusinessUrlState(business.Guid, UrlState.UPDATED);

                    // Insert Business Score if have one.
                    if (businessScore.Score != null)
                        dbLib.InsertBusinessScore(businessScore);

                    // Getting reviews if option checked.
                    if (request.GetReviews && request.DateLimit != null)
                        GetReviews(businessProfile, request.DateLimit, dbLib, driver);

                } catch (Exception e) {
                    using StreamWriter sw = File.AppendText(log);
                    sw.WriteLine(business.Url);
                    sw.WriteLine(e.Message);
                    sw.WriteLine(e.StackTrace);
                    sw.WriteLine("\n\n");
                }
            }

            if (request.Category != null) {
                foreach (DbBusinessAgent business in businessList)
                    dbLib.UpdateBusinessProfileProcessingState(business.IdEtab, false);
            }

            driver.WebDriver.Quit();
            dbLib.DisconnectFromDB();
        }

        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="guid"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        /// <exception cref="Exception"></exception>
        public static (DbBusinessProfile?, DbBusinessScore?) GetBusinessProfileFromGooglePage(SeleniumDriver driver, string url, string guid, string? idEtab = null) {
            // Initialization of all variables
            string? name = null;
            string? category = null;
            string? adress = null;
            int? reviews = null;
            string? website = null;
            string? tel = null;
            float? score = null;
            BusinessStatus status = BusinessStatus.OPEN;
            string? geoloc = null;

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

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.globalScore))) {
                if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score)) && float.TryParse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label").Replace("étoiles", "").Trim(), out _))
                   score = float.Parse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label").Replace("étoiles", "").Trim());

                if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews, XPathProfile.nbReviews2)) && int.TryParse(Regex.Replace(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews, XPathProfile.nbReviews2).GetAttribute("aria-label").Replace("avis", "").Replace(" ", "").Trim(), @"\s", ""), out _))
                    reviews = int.Parse(Regex.Replace(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews, XPathProfile.nbReviews2).GetAttribute("aria-label").Replace("avis", "").Replace(" ", "").Trim(), @"\s", ""));
            }

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel)))
                tel = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel).GetAttribute("aria-label").Replace("Numéro de téléphone:", "").Trim();
            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website2, XPathProfile.website)))
                website = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website2, XPathProfile.website).GetAttribute("href").Trim();

            if (name == null) {
                return (null, null);
            }

            idEtab ??= ToolBox.ComputeMd5Hash(name + adress);
            DbBusinessProfile dbBusinessProfile = new(idEtab, guid, name, category, adress, tel, website, geoloc, DateTime.UtcNow, DateTime.UtcNow, status);
            DbBusinessScore dbBusinessScore = new(idEtab, score, reviews, DateTime.UtcNow);
            return (dbBusinessProfile, dbBusinessScore);
        }

        /// <summary>
        /// Getting all reviews
        /// </summary>
        /// <param name="reviewWebElement"></param>
        /// <param name="idEtab"></param>
        /// <param name="dateLimit"></param>
        /// <returns>Business Review</returns>
        /// <exception cref="Exception"></exception>
        public static DbBusinessReview GetBusinessReviewsInfos(IWebElement reviewWebElement, string idEtab, DateTime? dateLimit) {

            // User
            string? userName = null;
            bool localGuide = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.localGuide));
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

            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.score)))
                reviewScore = int.Parse(ToolBox.FindElementSafe(reviewWebElement, XPathReview.score).GetAttribute("aria-label").Replace("étoiles", "").Trim());
            else
                throw new Exception("No score for this review");

            if (ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)))
                userNbReviews = int.Parse(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews).Text.Replace("avis", "").Replace("·", "").Replace(" ", "").Trim());
            else
                userNbReviews = 1;

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

            if (reviewList == null)
                return null;

            IWebElement scrollingPanel = ToolBox.FindElementSafe(driver, XPathReview.scrollingPanel);

            IWebElement parent = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].parentNode;", scrollingPanel);

            while (reviewListLength != reviewList.Count) {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", parent);
                Thread.Sleep(2000);
                reviewListLength = reviewList.Count;
                reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList, XPathReview.reviewList2);
                if (ToolBox.Exists(ToolBox.FindElementsSafe(driver, XPathReview.googleDate))) {
                    reviewGoogleDate = ToolBox.FindElementSafe(reviewList.Last(), XPathReview.googleDate).Text.Trim();
                    if (ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate) < dateLimit)
                        break;
                }
            };

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
        /// Get business list from request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<DbBusinessAgent>? GetBusinessListFromRequest(BusinessAgentRequest request, DbLib dbLib) {
            List<DbBusinessAgent> businessList = new();
            try {
                // Getting business list.
                // With url state.
                if (request.UrlState != null && request.Entries.HasValue) {
                    businessList = dbLib.GetBusinessList(request.UrlState.Value, request.Entries.Value);
                    foreach (DbBusinessAgent business in businessList)
                        dbLib.UpdateBusinessUrlState(business.Guid, UrlState.PROCESSING);
                }

                // With category.
                if (request.Category != null && request.Entries.HasValue) {
                    businessList = dbLib.GetBusinessList(request.Category, request.Entries.Value);
                    foreach (DbBusinessAgent business in businessList)
                        if (business.IdEtab != null)
                            dbLib.UpdateBusinessProfileProcessingState(business.IdEtab, true);
                }

                // With url list.
                /*if (request.UrlList != null) {
                    businessList = dbLib.GetBusinessList(request.UrlList);
                    foreach (DbBusinessAgent business in businessList)
                        if (business.IdEtab != null)
                            dbLib.UpdateBusinessProfileProcessingState(business.IdEtab, true);
                }*/
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                return null;
            }

            return businessList;
        }

        /// <summary>
        /// Getting reviews for business.
        /// </summary>
        /// <param name="businessProfile"></param>
        /// <param name="dateLimit"></param>
        /// <exception cref="Exception"></exception>
        public static void GetReviews(DbBusinessProfile businessProfile, DateTime? dateLimit, DbLib dbLib, SeleniumDriver driver) {
            if (!ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)))
                return;

            driver.WebDriver.FindElement(XPathReview.toReviewsPage).Click();
            Thread.Sleep(2500);

            // Sorting reviews.
            SortReviews(driver.WebDriver);

            // Getting reviews.
            ReadOnlyCollection<IWebElement>? reviews = GetReviewsFromGooglePage(driver.WebDriver, dateLimit);

            if (reviews == null)
                return;

            foreach (IWebElement review in reviews) {
                try {
                    DbBusinessReview businessReview = GetBusinessReviewsInfos(review, businessProfile.IdEtab, dateLimit);
                    if (!dbLib.CheckBusinessReviewExist(businessProfile.IdEtab, businessReview.IdReview))
                        dbLib.InsertBusinessReview(businessReview);
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }
        #endregion
    }
}