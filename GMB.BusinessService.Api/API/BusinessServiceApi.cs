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
using GMB.Sdk.Core.Types.Api;

namespace GMB.Business.Api.API
{
    /// <summary>
    /// Business Service.
    /// </summary>
    public class BusinessServiceApi
    {

        public static readonly string API_KEY = "AIzaSyCKhxq-6XXvHZ8bHqDnsYb9v-sbEMl4A6E";

        #region Profile & Score
        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="request"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> GetBusinessProfileAndScoreFromGooglePageAsync(SeleniumDriver driver, GetBusinessProfileRequest request, DbBusinessProfile? business, bool getPlusCode = true)
        {
            // Initialization of all variables
            int? reviews = null;
            double? score = null;
            string? geoloc = null;
            string? country = null;
            string? placeId = null;
            BusinessStatus status = BusinessStatus.OPERATIONAL;

            driver.GetToPage(request.Url);

            WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(10));

            try
            {
                // Business Name
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@role='main' and @aria-label]")));
                string ? name = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim();
                if (name == null)
                    return (null, null);
                else
                    Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),\s-]+|\s{2,}", "");

                string? category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category)?.Text?.Replace("·", "").Trim();

                if (category == null && ToolBox.FindElementSafe(driver.WebDriver, new() { By.XPath("//div[text() = 'VÉRIFIER LA DISPONIBILITÉ']") })?.Text == "VÉRIFIER LA DISPONIBILITÉ")
                    category = "Hébergement";

                string? googleAddress = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress)?.GetAttribute("aria-label")?.Replace("Adresse:", "")?.Trim();
                string? img = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.img)?.GetAttribute("src")?.Trim();
                string? tel = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel)?.GetAttribute("aria-label")?.Replace("Numéro de téléphone:", "")?.Trim();
                string? website = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website)?.GetAttribute("href")?.Trim();

                // Business Status
                string? status_tmp = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.status)?.Text.Trim();
                if (status_tmp != null)
                {
                    if (status_tmp.Contains("Fermé définitivement") || status_tmp.Contains("Définitivement fermé"))
                        status = BusinessStatus.CLOSED_PERMANENTLY;
                    if (status_tmp.Contains("Fermé temporairement"))
                        status = BusinessStatus.CLOSED_TEMPORARILY;
                }

                #region Score
                double? parsedScore = null;

                if (ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score) != null && double.TryParse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label")?.Replace("étoiles", "")?.Trim(), out double parsedScoreValue))
                    parsedScore = parsedScoreValue;

                score ??= parsedScore ?? double.Parse(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hotelScore)?.Text ?? "0");

                if (score == 0 && ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score)?.GetAttribute("aria-label") is string ariaLabel && double.TryParse(ariaLabel.AsSpan(0, Math.Min(3, ariaLabel.Length)), out double parsedScore2))
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

                #region Place ID
                if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.placeId)))
                {
                    var scriptTag = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.placeId)?.GetAttribute("innerHTML");
                    int index = scriptTag.IndexOf("reviews?placeid");

                    if (index != -1)
                    {
                        string substring = scriptTag[(index + 22)..];
                        placeId = substring[..substring.IndexOf('\\')];
                    }
                }
                #endregion

                request.IdEtab ??= ToolBox.ComputeMd5Hash(name + googleAddress);
                request.Guid ??= Guid.NewGuid().ToString("N");
                DbBusinessProfile dbBusinessProfile = new(placeId, request.IdEtab, request.Guid, name, category, googleAddress, business?.Address, business?.PostCode, business?.City, business?.CityCode, business?.Lat, business?.Lon, business?.IdBan, business?.AddressType, business?.StreetNumber, business?.AddressScore, tel, website, business?.PlusCode, DateTime.UtcNow, status, img, country, null, geoloc);
                DbBusinessScore? dbBusinessScore = new(request.IdEtab, score, reviews);

                if (business == null || !dbBusinessProfile.AdressEquals(business) || !business.Equals(dbBusinessProfile) || business.Country == null)
                {
                    #region PlusCode
                    if (getPlusCode)
                    {
                        string? plusCode = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.plusCode)?.GetAttribute("aria-label")?.Replace("Plus\u00A0code:", "").Trim();

                        if (plusCode != null)
                            (dbBusinessProfile.PlusCode, dbBusinessProfile.Geoloc, dbBusinessProfile.Country) = GetCoordinatesFromPlusCode(driver, plusCode);
                    }
                    #endregion

                    #region Address Api
                    if (googleAddress != null)
                    {
                        AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(googleAddress);
                        if (addressResponse != null)
                        {
                            dbBusinessProfile.Lon = (double?)addressResponse.Features[0]?.Geometry?.Coordinates[0];
                            dbBusinessProfile.Lat = (double?)addressResponse.Features[0]?.Geometry?.Coordinates[1];
                            dbBusinessProfile.City = addressResponse.Features[0]?.Properties?.City;
                            dbBusinessProfile.PostCode = addressResponse.Features[0]?.Properties?.Postcode;
                            dbBusinessProfile.CityCode = addressResponse.Features[0]?.Properties?.CityCode;
                            dbBusinessProfile.Address = addressResponse.Features[0]?.Properties?.Street;
                            dbBusinessProfile.AddressType = addressResponse.Features[0]?.Properties?.PropertyType;
                            dbBusinessProfile.IdBan = addressResponse.Features[0]?.Properties?.Id;
                            dbBusinessProfile.StreetNumber = addressResponse.Features[0]?.Properties?.HouseNumber;
                            dbBusinessProfile.AddressScore = (float?)addressResponse.Features[0]?.Properties?.Score;
                        }
                    }
                    #endregion
                }

                return (dbBusinessProfile, dbBusinessScore);
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't get business infos (profile or score).", e);
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
            try
            {
                driver.GetToPage("https://plus.codes/" + plusCode);
                ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.expand).Click();
                Thread.Sleep(1000);
                string? longPlusCode = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.longPlusCode)?.Text;
                string? geoloc = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.coordinates)?.Text;
                string? country = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.shortPlusCodeLocality).GetAttribute("innerHTML");
                country = country[(country.LastIndexOf(',') + 1)..].Trim();
                return (longPlusCode, geoloc, country);
            }
            catch
            {
                return (plusCode, null, null);
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
        public static List<DbBusinessReview>? GetReviews(string idEtab, DateTime? dateLimit, SeleniumDriver driver)
        {

            try
            {
                WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(10));
                IWebElement toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@role='tab' and contains(@aria-label, 'Avis')]")));
                toReviewPage.Click();
            }
            catch (Exception e)
            {
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
                    }
                    catch (Exception e)
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
        private static ReadOnlyCollection<IWebElement>? GetWebElements(IWebDriver driver, DateTime? dateLimit)
        {
            try
            {
                ReadOnlyCollection<IWebElement>? reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

                if (reviewList == null || reviewList.Count == 0)
                {
                    return null;
                }

                IWebElement? scrollingPanel = ToolBox.FindElementSafe(driver, XPathReview.scrollingPanel);
                IWebElement parent = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].parentNode.parentNode.parentNode;", scrollingPanel);

                int reviewListLength = 0;
                string? reviewGoogleDate;
                DateTime realDate;
                int index = 0;

                while (reviewListLength != reviewList.Count)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", parent);
                    Thread.Sleep(2000);
                    reviewListLength = reviewList.Count;
                    reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

                    if (reviewList == null)
                    {
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

                    for (int i = index; i < reviewList.Count; i++)
                    {
                        IWebElement item = reviewList[i];
                        string? reviewText = ToolBox.FindElementSafe(item, XPathReview.text)?.Text?.Replace("\n", "").Replace("(Traduit par google)", "").Trim();
                        if (reviewText != null && reviewText.EndsWith("Plus") && ToolBox.Exists(ToolBox.FindElementSafe(item, XPathReview.plusButton)))
                        {
                            try
                            {
                                ToolBox.FindElementSafe(item, XPathReview.plusButton).Click();
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        index++;
                    }

                }

                return reviewList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("javascript error: Cannot read properties of null (reading 'parentNode')"))
                {
                    return null;
                }
                else
                {
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
        private static DbBusinessReview? GetReviewFromGooglePage(IWebElement reviewWebElement, string idEtab, DateTime? dateLimit)
        {
            try
            {
                string idReview = reviewWebElement.GetAttribute("data-review-id")?.Trim() ?? throw new Exception("No review id");

                string? reviewGoogleDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.googleDate)?.Text?.Trim();
                DateTime reviewDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);

                if (reviewGoogleDate == null || dateLimit.HasValue && reviewDate < dateLimit.Value)
                    return null;

                string userName = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userName)?.GetAttribute("aria-label")?.Replace("Photo de", "").Trim() ?? "";

                int reviewScore = ToolBox.FindElementsSafe(reviewWebElement, XPathReview.score).Count;

                int userNbReviews = 1;
                string? userNbReviewsText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)?.Text?.Replace("avis", "").Replace("·", "").Replace("Local Guide", "").Replace(" ", "").Trim();
                if (!string.IsNullOrEmpty(userNbReviewsText) && int.TryParse(userNbReviewsText, out int parsedUserNbReviews))
                {
                    userNbReviews = parsedUserNbReviews;
                }

                string? reviewText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.text)?.Text?.Replace("\n", "").Replace("(Traduit par google)", "").Trim();

                bool localGuide = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)) && ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews).Text.Contains('·');
                GoogleUser user = new(userName, userNbReviews, localGuide);

                bool replied = ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyText));

                string? reviewReplyGoogleDate = null;
                DateTime? reviewReplyDate = null;
                if (replied && ToolBox.Exists(ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyGoogleDate)))
                {
                    reviewReplyGoogleDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.replyGoogleDate).Text;
                    reviewReplyDate = ToolBox.ComputeDateFromGoogleDate(reviewReplyGoogleDate);
                }

                return new DbBusinessReview(idEtab, ToolBox.ComputeMd5Hash(idEtab + idReview), idReview, user, reviewScore, reviewText, reviewGoogleDate, reviewDate, replied, DateTime.UtcNow, reviewReplyDate, reviewReplyGoogleDate);
            }
            catch (Exception)
            {
                throw new Exception("Couldn't get review info from a review element");
            }
        }
        /// <summary>
        /// Sort reviews in ascending date order.
        /// </summary>
        /// <param name="driver"></param>
        private static void SortReviews(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));

                IWebElement sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews)));
                sortButton.Click();

                IWebElement sortButton2 = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews2)));
                sortButton2.Click();
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't sort reviews", e);
            }
        }
        #endregion
    }
}