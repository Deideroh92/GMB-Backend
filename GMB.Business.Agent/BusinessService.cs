using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using SeleniumExtras.WaitHelpers;
using GMB.Sdk.Core;
using Serilog;
using GMB.Sdk.Core.Types.Models;
using GMB.Business.Api.Models;
using GMB.Sdk.Core.Types.Database.Models;
using static System.Net.Mime.MediaTypeNames;

namespace GMB.Business.Api
{
    /// <summary>
    /// Business Service.
    /// </summary>
    public class BusinessService {

        #region Profile & Score
        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="request"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> GetBusinessProfileAndScoreFromGooglePageAsync(SeleniumDriver driver, GetBusinessProfileRequest request) {
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
            float? score = null;
            string? longPlusCode = null;
            string? geoloc = null;
            string? country = null;
            BusinessStatus status = BusinessStatus.OPEN;

            driver.GetToPage(request.Url);

            try {
                string? category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category)?.Text?.Replace("·", "").Trim();
                string? googleAddress = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress)?.GetAttribute("aria-label")?.Replace("Adresse:", "")?.Trim();
                string? img = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.img)?.GetAttribute("src")?.Trim();
                string? tel = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel)?.GetAttribute("aria-label")?.Replace("Numéro de téléphone:", "")?.Trim();
                string? website = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website)?.GetAttribute("href")?.Trim();

                // Business Name
                string? name = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim();
                if (name == null)
                    return (null, null);
                else
                    Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),\s-]+|\s{2,}", "");

                // Business Status
                string? status_tmp = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.status)?.Text.Trim();
                if (status_tmp != null)
                {
                    if (status_tmp.Contains("Fermé définitivement") || status_tmp.Contains("Définitivement fermé"))
                        status = BusinessStatus.CLOSED;
                    if (status_tmp.Contains("Fermé temporairement"))
                        status = BusinessStatus.TEMPORARLY_CLOSED;
                }

                #region Plus Code
                string? plusCode = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.plusCode)?.GetAttribute("aria-label")?.Replace("Plus\u00A0code:", "").Trim();

                if (plusCode != null)
                {
                    (longPlusCode, geoloc, country) = GetCoordinatesFromPlusCode(driver, plusCode);
                    driver.GetToPage(request.Url);
                }
                #endregion

                #region Score
                float? parsedScore = null;
                float? addressScore = null;

                if (ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score) != null && float.TryParse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label")?.Replace("étoiles", "")?.Trim(), out float parsedScoreValue))
                    parsedScore = parsedScoreValue;

                score ??= parsedScore ?? float.Parse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelScore)?.Text ?? "0");

                if (score == 0 && ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score)?.GetAttribute("aria-label") is string ariaLabel && float.TryParse(ariaLabel.AsSpan(0, Math.Min(3, ariaLabel.Length)), out float parsedScore2))
                    score = parsedScore2;

                if (ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.nbReviews) is WebElement nbReviewsElement)
                {
                    string? label = nbReviewsElement.GetAttribute("aria-label")?.Replace("avis", "").Replace(" ", "").Trim();
                    if (label == null && score != null)
                        label = nbReviewsElement.ComputedAccessibleLabel.Replace("avis", "").Replace(" ", "").Trim();
                    if (label != null && int.TryParse(Regex.Replace(label, @"\s", ""), out int parsedReviews))
                        reviews = parsedReviews;
                }
                #endregion

                #region Address Api
                if (googleAddress != null) {
                    AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(googleAddress);
                    if (addressResponse != null) {
                        lon = (float?)(addressResponse.Features[0]?.Geometry?.Coordinates[0]);
                        lat = (float?)(addressResponse.Features[0]?.Geometry?.Coordinates[1]);
                        city = addressResponse.Features[0]?.Properties?.City;
                        postCode = addressResponse.Features[0]?.Properties?.Postcode;
                        cityCode = addressResponse.Features[0]?.Properties?.CityCode;
                        address = addressResponse.Features[0]?.Properties?.Street;
                        addressType = addressResponse.Features[0]?.Properties?.PropertyType;
                        idBan = addressResponse.Features[0]?.Properties?.Id;
                        streetNumber = addressResponse.Features[0]?.Properties?.HouseNumber;
                        addressScore = (float?)addressResponse.Features[0]?.Properties?.Score;
                    }
                }
                #endregion

                request.IdEtab ??= ToolBox.ComputeMd5Hash(name + googleAddress);
                request.Guid ??= Guid.NewGuid().ToString("N");
                DbBusinessProfile dbBusinessProfile = new(request.IdEtab, request.Guid, name, category, googleAddress, address, postCode, city, cityCode, lat, lon, idBan, addressType, streetNumber, addressScore, tel, website, longPlusCode ?? plusCode, DateTime.UtcNow, status, img, country, geoloc);
                DbBusinessScore? dbBusinessScore = new(request.IdEtab, score, reviews);
                return (dbBusinessProfile, dbBusinessScore);
            }
            catch (Exception e) {
                throw new Exception($"Couldn't get business infos (profile or score) with id etab = [{request.IdEtab}] and guid = [{request.Guid}] and url = [{request.Url}]", e);
            }
        }
        /// <summary>
        /// Get Google Plus Code coordinates.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="plusCode"></param>
        /// <returns>Coordinates from Plus Code</returns>
        public static (string?, string?, string?) GetCoordinatesFromPlusCode(SeleniumDriver driver, string plusCode)
        {
            try {
                driver.GetToPage("https://plus.codes/" + plusCode);
                ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.expand).Click();
                Thread.Sleep(1000);
                string? longPlusCode = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.longPlusCode)?.Text;
                string? geoloc = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.coordinates)?.Text;
                string? country = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.shortPlusCodeLocality).GetAttribute("innerHTML");
                country = country[(country.LastIndexOf(',') + 1)..].Trim();
                return (longPlusCode, geoloc, country);
            }
            catch (Exception e) {
                throw new Exception($"Couldn't get plus code infos for = [{plusCode}]", e);
            }
        }
        #endregion

        #region Reviews
        /// <summary>
        /// Getting reviews for business.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="dateLimit"></param>
        /// <param name="driver"></param>
        public static List<DbBusinessReview>? GetReviews(string idEtab, DateTime? dateLimit, SeleniumDriver driver) {

            try {
                WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(2));
                IWebElement toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)));
                toReviewPage.Click();
            } catch (Exception e) {
                throw new Exception("Couldn't get to review pages", e);
            }

            Thread.Sleep(2000);

            // Sorting reviews.
            SortReviews(driver.WebDriver);

            Thread.Sleep(1000);

            // Getting reviews.
            ReadOnlyCollection<IWebElement>? reviews = GetWebElements(driver.WebDriver, dateLimit);
            List<DbBusinessReview>? businessReviews = new();

            if (reviews != null)
            {
                foreach (IWebElement review in reviews)
                {
                    try
                    {
                        DbBusinessReview? businessReview = GetReviewFromGooglePage(review, idEtab, dateLimit);
                        if (businessReview == null)
                            continue;
                        businessReviews.Add(businessReview);
                    } catch (Exception e)
                    {
                        Log.Error($"Couldn't treat a review : {e.Message}. Error : {e.StackTrace}", e);
                    }
                }
            }
            return businessReviews;
        }
        /// <summary>
        /// Scroll to get all review list.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="dateLimit"></param>
        /// <returns>Review list</returns>
        private static ReadOnlyCollection<IWebElement>? GetWebElements(IWebDriver driver, DateTime? dateLimit) {
            try {
                ReadOnlyCollection<IWebElement>? reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

                if (reviewList == null || reviewList.Count == 0) {
                    return null;
                }

                IWebElement? scrollingPanel = ToolBox.FindElementSafe(driver, XPathReview.scrollingPanel);
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
                    if (reviewGoogleDate != null) {
                        realDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);
                        if (realDate < dateLimit) {
                            break;
                        }
                    }
                }
                return reviewList;
            } catch (Exception e) {
                if (e.Message.Contains("javascript error: Cannot read properties of null (reading 'parentNode')")) {
                    return null;
                } else {
                    throw new Exception($"Error getting reviews from the Google page : {e.Message}", e);
                }
            }
        }
        /// <summary>
        /// Getting all reviews
        /// </summary>
        /// <param name="reviewWebElement"></param>
        /// <param name="idEtab"></param>
        /// <param name="dateLimit"></param>
        /// <returns>Business Review</returns>
        private static DbBusinessReview? GetReviewFromGooglePage(IWebElement reviewWebElement, string idEtab, DateTime? dateLimit) {
            try {
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
                if (reviewText != null && reviewText.EndsWith("Plus")) {
                    try {
                        var test = ToolBox.FindElementSafe(reviewWebElement, XPathReview.plusButton);
                        test.Click();
                        Thread.Sleep(1000);
                        reviewText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.text)?.Text?.Replace("\n", "").Replace("(Traduit par google)", "").Trim();
                    } catch {
                        reviewText = reviewText.TrimEnd("Plus".ToCharArray());
                    }
                }

                bool localGuide = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)) && ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews).Text.Contains('·');
                GoogleUser user = new(userName, userNbReviews, localGuide);

                bool replied = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyText));

                return new DbBusinessReview(idEtab, idReview, user, reviewScore, reviewText, reviewGoogleDate, reviewDate, replied, DateTime.UtcNow);
            } catch (Exception) {
                throw new Exception("Couldn't get review info from a review element");
            }
        }
        /// <summary>
        /// Sort reviews in ascending date order.
        /// </summary>
        /// <param name="driver"></param>
        private static void SortReviews(IWebDriver driver) {
            try {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));

                IWebElement sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews)));
                sortButton.Click();

                IWebElement sortButton2 = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews2)));
                sortButton2.Click();
            } catch(Exception e) {
                throw new Exception("Couldn't sort reviews", e);
            }
        }
        #endregion
    }
}