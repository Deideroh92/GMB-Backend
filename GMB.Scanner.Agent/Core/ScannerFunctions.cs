using GMB.BusinessService.Api.Models;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;

namespace GMB.Scanner.Agent.Core
{
    public class TestResult(bool success, string? message = null)
    {
        public bool Success { get; set; } = success;
        public string? Message { get; set; } = message;
    }

    public class ScannerFunctions
    {
        static void Main() { }

        private static readonly char[] separator = [' ', '\t'];

        #region Profile & Score
        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="request"></param>
        /// <param name="business"></param>
        /// <param name="getPlusCode"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        public static async Task<(DbBusinessProfile?, DbBusinessScore?)> GetBusinessProfileAndScoreFromGooglePageAsync(SeleniumDriver driver, GetBusinessProfileRequest request, DbBusinessProfile? business, bool getPlusCode = true, bool getPhotos = false)
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
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@role='main' and @aria-label]")));
                } catch (Exception)
                {
                    return (null, null);
                }

                string? name = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim();
                if (name == null)
                    return (null, null);
                else
                    Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),\s-]+|\s{2,}", "");

                string? locatedIn = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.locatedIn)?.GetAttribute("aria-label")?.Trim();

                string? category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category)?.Text?.Replace("·", "").Trim();

                if (category == null && ToolBox.FindElementSafe(driver.WebDriver, [By.XPath("//div[text() = 'VÉRIFIER LA DISPONIBILITÉ']")])?.Text == "VÉRIFIER LA DISPONIBILITÉ")
                    category = "Hébergement";

                string? googleAddress = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.adress)?.GetAttribute("aria-label")?.Replace("Adresse:", "")?.Trim();
                string? img = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.img)?.GetAttribute("src")?.Trim();
                string? tel = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.tel)?.GetAttribute("aria-label")?.Replace("Numéro de téléphone:", "")?.Trim();
                string? website = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.website)?.GetAttribute("href")?.Trim();
                
                bool hasOpeningHours = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.hasOpeningHours) != null;

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

                if (ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score) != null)
                {
                    string? test = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label");
                    string? scoreString = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.score).GetAttribute("aria-label")?.Replace("étoiles", "")?.Trim();

                    if (scoreString != null)
                    {
                        // Replace commas with dots (or dots with commas)
                        scoreString = scoreString.Replace(',', '.');

                        if (double.TryParse(scoreString, out double parsedScoreValue))
                        {
                            parsedScore = parsedScoreValue;
                        }
                    }
                }

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
                DbBusinessProfile dbBusinessProfile = new(placeId, request.IdEtab, request.Guid, name, category, googleAddress, business?.Address, business?.PostCode, business?.City, business?.CityCode, business?.Lat, business?.Lon, business?.IdBan, business?.AddressType, business?.StreetNumber, business?.AddressScore, tel, website, business?.PlusCode, DateTime.UtcNow, status, img, country, null, geoloc, 0, null, null, locatedIn, hasOpeningHours);
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

                    #region Address details
                    if (dbBusinessProfile.GoogleAddress != null)
                    {
                        AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(dbBusinessProfile.GoogleAddress);
                        if (addressResponse != null)
                        {
                            dbBusinessProfile.Lon = addressResponse.Features[0]?.Geometry?.Coordinates[0];
                            dbBusinessProfile.Lat = addressResponse.Features[0]?.Geometry?.Coordinates[1];
                            dbBusinessProfile.City = addressResponse.Features[0]?.Properties?.City;
                            dbBusinessProfile.PostCode = addressResponse.Features[0]?.Properties?.Postcode;
                            dbBusinessProfile.CityCode = addressResponse.Features[0]?.Properties?.CityCode;
                            dbBusinessProfile.Address = addressResponse.Features[0]?.Properties?.HouseNumber + " " + addressResponse.Features[0]?.Properties?.Street;
                            dbBusinessProfile.AddressType = addressResponse.Features[0]?.Properties?.PropertyType;
                            dbBusinessProfile.IdBan = addressResponse.Features[0]?.Properties?.Id;
                            dbBusinessProfile.StreetNumber = addressResponse.Features[0]?.Properties?.HouseNumber;
                            dbBusinessProfile.AddressScore = (float?)addressResponse.Features[0]?.Properties?.Score;
                        }

                        if (dbBusinessProfile.PostCode == null || !dbBusinessProfile.GoogleAddress.Contains(dbBusinessProfile.PostCode))
                        {
                            string pattern = @"(\d{4,})";
                            Match match = Regex.Match(dbBusinessProfile.GoogleAddress, pattern, RegexOptions.RightToLeft);

                            if (match.Success && dbBusinessProfile.GoogleAddress.Contains(match.Value))
                                dbBusinessProfile.PostCode = match.Value;

                            if ((dbBusinessProfile.City == null || !dbBusinessProfile.GoogleAddress.Contains(dbBusinessProfile.City)) && match.Value != null)
                            {
                                int index = dbBusinessProfile.GoogleAddress.LastIndexOf(match.Value);
                                // Check if the consecutive digits were found in the input string
                                if (index != -1 && index + match.Value.Length < dbBusinessProfile.GoogleAddress.Length)
                                    // Return the substring after consecutive digits
                                    dbBusinessProfile.City = dbBusinessProfile.GoogleAddress[(index + match.Value.Length)..].Trim();
                            }
                        }

                        if (dbBusinessProfile.Country == null || !dbBusinessProfile.GoogleAddress.Contains(dbBusinessProfile.Country))
                        {
                            string[] words = dbBusinessProfile.GoogleAddress.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "CountryList.txt");
                            string countries = File.ReadAllText(filePath);

                            if (words.Length > 0 && countries.Contains(words[^1]) && dbBusinessProfile.GoogleAddress.Contains(words[^1]))
                            {
                                dbBusinessProfile.Country = words[^1];
                            }
                        }

                        if (dbBusinessProfile.Address == null || !dbBusinessProfile.GoogleAddress.Contains(dbBusinessProfile.Address))
                        {
                            string address = dbBusinessProfile.GoogleAddress;

                            if (dbBusinessProfile.Country != null)
                                address = address.Replace(", " + dbBusinessProfile.Country, "");
                            if (dbBusinessProfile.City != null)
                                address = address.Replace(dbBusinessProfile.City, "");
                            if (dbBusinessProfile.PostCode != null)
                                address = address.Replace(dbBusinessProfile.PostCode, "");
                            address = address.Trim().TrimEnd(',').Trim();
                            dbBusinessProfile.Address = address;
                        }
                    }
                    #endregion
                }

                return (dbBusinessProfile, dbBusinessScore);
            } catch (Exception e)
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
            } catch
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
        /// <param name="isHotel"></param>
        /// <param name="nbLimit"></param>
        public static List<DbBusinessReview>? GetReviews(string idEtab, DateTime? dateLimit, SeleniumDriver driver, bool isHotel = false, int? nbLimit = null)
        {

            try
            {
                WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(10));
                IWebElement toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@role='tab' and contains(@aria-label, 'Avis')]")));
                toReviewPage.Click();
            } catch (Exception e)
            {
                throw new Exception("Couldn't get to review pages", e);
            }

            Thread.Sleep(2000);

            // Sorting reviews.
            try
            {
                if (isHotel)
                    SortHotelReviews(driver.WebDriver);
                else SortReviews(driver.WebDriver);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't sort Reviews.", e);
            }


            Thread.Sleep(2000);

            // Getting reviews.
            ReadOnlyCollection<IWebElement>? reviews = GetWebElements(driver.WebDriver, dateLimit, nbLimit);

            if (reviews == null || reviews.Count == 0)
                return null;

            List<DbBusinessReview>? businessReviews = [];

            string? visitDateOldest = null;

            if (reviews != null)
            {
                foreach (IWebElement review in reviews)
                {
                    try
                    {
                        if (businessReviews.Count >= nbLimit)
                            return businessReviews;

                        DbBusinessReview? businessReview = GetReviewFromGooglePage(review, idEtab, dateLimit, nbLimit != null);

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
        /// <param name="nbLimit"></param>
        /// <returns>Review list</returns>
        private static ReadOnlyCollection<IWebElement>? GetWebElements(IWebDriver driver, DateTime? dateLimit, int? nbLimit)
        {
            try
            {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@data-review-id and @aria-label]")));
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

                while (reviewListLength != reviewList.Count || reviewList.Count < nbLimit)
                {
                    try
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight", parent);
                        Thread.Sleep(2000);
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    reviewListLength = reviewList.Count;
                    reviewList = ToolBox.FindElementsSafe(driver, XPathReview.reviewList);

                    if (reviewList == null)
                    {
                        break;
                    }

                    if (nbLimit != null && reviewList.Count > nbLimit)
                    {
                        break;
                    }

                    if (nbLimit == null)
                    {
                        reviewGoogleDate = ToolBox.FindElementSafe(reviewList.Last(), XPathReview.googleDate)?.Text?.Replace(" sur\r\nGoogle", "").Trim();
                        if (reviewGoogleDate != null)
                        {
                            realDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);
                            if (realDate < dateLimit)
                            {
                                break;
                            }
                        }
                    }
                }

                for (int i = index; i < reviewList.Count; i++)
                {
                    IWebElement item = reviewList[i];
                    try
                    {
                        ToolBox.FindElementSafe(item, XPathReview.plusButton).Click();
                    } catch
                    {
                        continue;
                    }
                    index++;
                }

                return reviewList;
            } catch (Exception e)
            {
                throw new Exception($"Error getting reviews from the Google page : {e.Message}", e);
            }
        }
        /// <summary>
        /// Getting all reviews
        /// </summary>
        /// <param name="reviewWebElement"></param>
        /// <param name="idEtab"></param>
        /// <param name="dateLimit"></param>
        /// <param name="hasNbLimit"></param>
        /// <returns>Business Review</returns>
        private static DbBusinessReview? GetReviewFromGooglePage(IWebElement reviewWebElement, string idEtab, DateTime? dateLimit, bool hasNbLimit = false)
        {
            try
            {
                int hotelScore = 0;
                string idReview = reviewWebElement.GetAttribute("data-review-id")?.Trim() ?? throw new Exception("No review id");

                string? reviewGoogleDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.googleDate)?.Text?.Replace(" sur\r\nGoogle", "").Trim();

                string? visitDate = ToolBox.FindElementSafe(reviewWebElement, XPathReview.visitDate)?.Text;
                if (visitDate != null && !visitDate.Contains("Visité en"))
                    visitDate = null;

                DateTime reviewDate;

                if (reviewGoogleDate.Contains("an") && visitDate != null) reviewDate = ToolBox.ComputeDateFromVisitDate(visitDate);
                else reviewDate = ToolBox.ComputeDateFromGoogleDate(reviewGoogleDate);

                if ((reviewGoogleDate == null || dateLimit.HasValue && reviewDate < dateLimit.Value) && !hasNbLimit)
                    return null;

                string userName = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userName)?.GetAttribute("aria-label")?.Replace("Photo de", "").Trim() ?? "";

                if (!int.TryParse(ToolBox.FindElementSafe(reviewWebElement, XPathReview.score)?.GetAttribute("aria-label")?.Replace("étoile", "").Replace("s", "").Trim(), out int reviewScore) && !int.TryParse(ToolBox.FindElementSafe(reviewWebElement, XPathReview.hotelScore)?.Text?.FirstOrDefault().ToString(), out hotelScore))
                    throw new Exception();

                int userNbReviews = 1;
                string? userNbReviewsText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.userNbReviews)?.Text;
                
                if (userNbReviewsText != null)
                {
                    string pattern = @"\b(\d+)\s*avis\b";

                    // Use Regex.Match to find the first match in the input string
                    Match match = Regex.Match(userNbReviewsText, pattern);

                    if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedUserNbReviews))
                    {
                        userNbReviews = parsedUserNbReviews;
                    }
                }

                string? reviewText = ToolBox.FindElementSafe(reviewWebElement, XPathReview.text)?.Text?.Replace("\n", " ").Replace("\r", " ").Replace("(Traduit par google)", "").Trim();

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

                return new DbBusinessReview(idEtab, ToolBox.ComputeMd5Hash(idEtab + idReview), idReview, user, reviewScore > 0 ? reviewScore : hotelScore, reviewText, reviewGoogleDate, reviewDate, replied, DateTime.UtcNow, reviewReplyDate, reviewReplyGoogleDate, visitDate);
            } catch (Exception)
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
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));

                IWebElement sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews)));
                sortButton.Click();

                IWebElement sortButton2 = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews2)));
                sortButton2.Click();
            } catch (Exception e)
            {
                throw new Exception("Couldn't sort reviews", e);
            }
        }

        /// <summary>
        /// Sort hotel reviews in ascending date order filtering only Google reviews.
        /// </summary>
        /// <param name="driver"></param>
        private static void SortHotelReviews(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));

                IWebElement sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.hotelSortGoogleReviews)));
                sortButton.Click();

                Thread.Sleep(1000);

                sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.hotelSortPress)));
                sortButton.Click();

                Thread.Sleep(2000);

                sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.hotelSortReviews)));
                sortButton.Click();

                sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.hotelSortPress)));
                sortButton.Click();
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't sort reviews", e);
            }
        }

        public static async Task<TestResult> ScannerTest(SeleniumDriver driver)
        {
            ScannerFunctions scanner = new();
            DateTime reviewLimit = DateTime.UtcNow.AddDays(-15);

            #region Mairie de Paris
            GetBusinessProfileRequest request = new("https://www.google.fr/maps/place/Mairie+de+Paris/@48.8660828,2.3108754,13z/data=!4m10!1m2!2m1!1smairie+de+paris!3m6!1s0x47e66e23b4333db3:0xbc314dec89c4971!8m2!3d48.8641075!4d2.3421539!15sCg9tYWlyaWUgZGUgcGFyaXNaESIPbWFpcmllIGRlIHBhcmlzkgEJY2l0eV9oYWxsmgEjQ2haRFNVaE5NRzluUzBWSlEwRm5TVVEyYzA5MWNrbFJFQUXgAQA!16s%2Fg%2F11c6pn36ph?hl=fr&entry=ttu");
            (DbBusinessProfile? profile, DbBusinessScore? score) = await GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Hôtel de Ville" ||
                (profile.GoogleAddress != "29 Rue de Rivoli, 75004 Paris" && profile.GoogleAddress != "29 Rue de Rivoli, 75004 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75104" ||
                profile.Country != "France" ||
                profile.StreetNumber != "29" ||
                profile.Category != "Hôtel de ville" ||
                profile.Website != "https://www.paris.fr/" ||
                profile.PictureUrl == null ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PlusCode != "8FW4V942+JR" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return new(false, "Mairie de Paris - Business Profile error !");
            
            driver.GetToPage(request.Url);
            List<DbBusinessReview>? reviews = GetReviews(profile.IdEtab, reviewLimit, driver, false, 30);

            if (reviews == null)
                return new(false, "Mairie de Paris - Reviews empty !");

            foreach (DbBusinessReview review in reviews)
            {
                if (review.IdReview == null || review.Score < 1 || review.Score > 5 || review.ReviewGoogleDate == null)
                    return new(false, "Mairie de Paris - Review loop error !");
            }

            if (!reviews.Any(review => review != null && review.User.LocalGuide) || !reviews.Any(review => review != null && review.User.Name != null) || !reviews.Any(review => review != null && review.User.NbReviews > 1) || reviews.Any(review => review != null && review.ReviewReply != null) || !reviews.Any(review => review.VisitDate != null))
                return new(false, "Mairie de Paris - Review global error !");
            #endregion

            #region Louvre
            request = new("https://www.google.fr/maps/place/Mus%C3%A9e+du+Louvre/@48.8606111,2.337644,17z/data=!3m1!4b1!4m6!3m5!1s0x47e671d877937b0f:0xb975fcfa192f84d4!8m2!3d48.8606111!4d2.337644!16zL20vMDRnZHI?hl=fr&entry=ttu");
            (profile, score) = await GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Musée du Louvre" ||
                (profile.GoogleAddress != "75001 Paris" && profile.GoogleAddress != "75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.Category != "Musée d'art" ||
                profile.Website != "https://www.louvre.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl == null ||
                profile.PlusCode != "8FW4V86Q+63" ||
                (profile.Tel != "01 40 20 53 17" && profile.Tel != "+33 1 40 20 53 17") ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return new(false, "Louvre - Business Profile error !");

            driver.GetToPage(request.Url);
            reviews = GetReviews(profile.IdEtab, reviewLimit, driver, false, 30);

            if (reviews == null)
                return new(false, "Louvre - Reviews empty !");

            foreach (DbBusinessReview review in reviews)
            {
                if (review.IdReview == null || review.Score < 1 || review.Score > 5 || review.ReviewGoogleDate == null)
                    return new(false, "Louvre - Review loop error !");
            }

            if (!reviews.Any(review => review != null && review.User.LocalGuide) || !reviews.Any(review => review != null && review.User.Name != null) || !reviews.Any(review => review != null && review.User.NbReviews > 1) || reviews.Any(review => review != null && review.ReviewReply != null) || !reviews.Any(review => review.VisitDate != null))
                return new(false, "Louvre - Reviews global error !");
            #endregion

            #region Hôpital Necker
            request = new("https://www.google.fr/maps/place/H%C3%B4pital+Necker+AP-HP/@48.8452199,2.3157461,17z/data=!3m1!4b1!4m6!3m5!1s0x47e6703221308f89:0x57a7e5b303e7d9!8m2!3d48.8452199!4d2.3157461!16s%2Fm%2F03gzggj?hl=fr&entry=ttu");
            (profile, score) = await GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Hôpital Necker Enfants malades - AP-HP" ||
                (profile.GoogleAddress != "149 Rue de Sèvres, 75015 Paris" && profile.GoogleAddress != "149 Rue de Sèvres, 75015 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75115" ||
                profile.Country != "France" ||
                profile.Category != "Hôpital pour enfants" ||
                profile.StreetNumber != "149" ||
                profile.Website != "http://www.hopital-necker.aphp.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl == null ||
                profile.PlusCode != "8FW4R8W8+H9" ||
                (profile.Tel != "01 44 49 40 00" && profile.Tel != "+33 1 44 49 40 00") ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return new(false, "Hôpital Necker - Business Profile error !");

            driver.GetToPage(request.Url);
            reviews = GetReviews(profile.IdEtab, reviewLimit, driver, false, 30);

            if (reviews == null)
                return new(false, "Hôpital Necker - Reviews empty !");

            foreach (DbBusinessReview review in reviews)
            {
                if (review.IdReview == null || review.Score < 1 || review.Score > 5 || review.ReviewGoogleDate == null)
                    return new(false, "Hôpital Necker - Review loop error !");
            }

            if (!reviews.Any(review => review != null && review.User.LocalGuide) || !reviews.Any(review => review != null && review.User.Name != null) || !reviews.Any(review => review != null && review.User.NbReviews > 1) || reviews.Any(review => review != null && review.ReviewReply != null) || !reviews.Any(review => review.VisitDate != null))
                return new(false, "Hôpital Necker - Reviews global error !");
            #endregion

            #region Maxim's
            request = new("https://www.google.fr/maps/place/Maxim's/@48.8674428,2.3032712,15z/data=!4m10!1m2!2m1!1smaxim's!3m6!1s0x47e66fcd4e473877:0xcf18f93e84c578c5!8m2!3d48.8674428!4d2.3223256!15sCgdtYXhpbSdzWgkiB21heGltJ3OSAQpyZXN0YXVyYW504AEA!16zL20vMDl6a3Ix?hl=fr&entry=ttu");
            (profile, score) = await GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Maxim's" ||
                (profile.GoogleAddress != "3 Rue Royale, 75008 Paris" && profile.GoogleAddress != "3 Rue Royale, 75008 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75108" ||
                profile.Country != "France" ||
                profile.StreetNumber != "3" ||
                profile.Category != "Restaurant français" ||
                profile.Website != "https://restaurant-maxims.com/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl == null ||
                profile.PlusCode != "8FW4V88C+XW" ||
                (profile.Tel != "01 42 65 27 94" && profile.Tel != "+33 1 42 65 27 94") ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return new(false, "Maxim's - Business Profile error !");

            driver.GetToPage(request.Url);
            reviews = GetReviews(profile.IdEtab, reviewLimit, driver, false, 30);

            if (reviews == null)
                return new(false, "Maxim's - Reviews empty !");

            foreach (DbBusinessReview review in reviews)
            {
                if (review.IdReview == null || review.Score < 1 || review.Score > 5 || review.ReviewGoogleDate == null)
                    return new(false, "Maxim's - Review loop error !");
            }

            if (!reviews.Any(review => review != null && review.User.LocalGuide) || !reviews.Any(review => review != null && review.User.Name != null) || !reviews.Any(review => review != null && review.User.NbReviews > 1) || reviews.Any(review => review != null && review.ReviewReply != null) || !reviews.Any(review => review.VisitDate != null))
                return new(false, "Maxim's - Reviews global error !");
            #endregion

            #region Lasserre
            request = new("https://www.google.com/maps/place/Lasserre/@48.8697972,2.2942567,15z/data=!3m1!5s0x47e66fdac6120255:0xb3e6ad148f54b824!4m13!1m5!2m4!1srestaurant+gestronomique+paris!5m2!5m1!1s2024-02-22!3m6!1s0x47e66fdac63417f3:0xcf3ba46dec23641b!8m2!3d48.8663494!4d2.3099175!15sCh5yZXN0YXVyYW50IGdhc3Ryb25vbWlxdWUgcGFyaXMiA6ABAVogIh5yZXN0YXVyYW50IGdhc3Ryb25vbWlxdWUgcGFyaXOSARZmaW5lX2RpbmluZ19yZXN0YXVyYW50mgEkQ2hkRFNVaE5NRzluUzBWSlEwRm5TVU5HYlMxNWVGOW5SUkFC4AEA!16s%2Fg%2F1227jzq5?entry=ttu");
            (profile, score) = await GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Lasserre" ||
                (profile.GoogleAddress != "17 Av. Franklin Delano Roosevelt, 75008 Paris" && profile.GoogleAddress != "17 Av. Franklin Delano Roosevelt, 75008 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75108" ||
                profile.Country != "France" ||
                profile.StreetNumber != "17" ||
                profile.Category != "Restaurant gastronomique" ||
                profile.Website != "http://www.restaurant-lasserre.com/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl == null ||
                profile.PlusCode != "8FW4V885+GX" ||
                (profile.Tel != "01 43 59 02 13" && profile.Tel != "+33 1 43 59 02 13") ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return new(false, "Lasserre - Business Profile error !");

            driver.GetToPage(request.Url);
            reviews = GetReviews(profile.IdEtab, reviewLimit, driver, false, 30);

            if (reviews == null)
                return new(false, "Lasserre - Reviews empty !");

            foreach (DbBusinessReview review in reviews)
            {
                if (review.IdReview == null || review.Score < 1 || review.Score > 5 || review.ReviewGoogleDate == null)
                    return new(false, "Lasserre - Review loop error !");
            }

            if (!reviews.Any(review => review != null && review.User.LocalGuide) || !reviews.Any(review => review != null && review.User.Name != null) || !reviews.Any(review => review != null && review.User.NbReviews > 1) || reviews.Any(review => review != null && review.ReviewReply != null) || !reviews.Any(review => review.VisitDate != null))
                return new(false, "Lasserre - Reviews global error !");
            #endregion


            return new(true, "Test executed successfully.");
        }
        #endregion

        #region Url
        /// <summary>
        /// Initiate the getting url process.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="urlToPage"></param>
        public static List<string>? GetUrlsFromGooglePage(SeleniumDriver driver, string urlToPage)
        {

            List<string> urls = [];

            driver.GetToPage(urlToPage);

            ReadOnlyCollection<IWebElement>? businessList = ScrollIntoBusinessUrls(driver.WebDriver);

            // Single page
            if (businessList == null && ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim() != null)
            {
                urls.Add(driver.WebDriver.Url);
                return urls;
            }

            if (businessList == null)
                return null;

            foreach (IWebElement business in businessList)
            {
                try
                {
                    string name = business.Text.Split('\n')[0].Replace("\r", "");
                    string? url = ToolBox.FindElementSafe(business, [By.XPath(".//a[contains(@aria-label, \"" + name.Replace('\"', '\'') + "\")]")])?.GetAttribute("href").Replace("?authuser=0&hl=fr&rclk=1", "");

                    if (string.IsNullOrWhiteSpace(url))
                        continue;
                    urls.Add(url);
                } catch (Exception e)
                {
                    Log.Error(e, $"An exception occurred while collection a business url: {e.Message}");
                }
            }
            return urls;
        }

        /// <summary>
        /// Scrolling through the page to get all businesses.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns>A list of all businesses that we could gather.</returns>
        private static ReadOnlyCollection<IWebElement>? ScrollIntoBusinessUrls(IWebDriver driver)
        {
            try
            {
                IWebElement? body = ToolBox.FindElementSafe(driver, XPathUrl.body);
                ReadOnlyCollection<IWebElement>? businessList = ToolBox.FindElementsSafe(driver, XPathUrl.businessList);

                if (body == null || businessList == null)
                    return null;

                int? length;
                const int waitTimeMilliseconds = 1000;

                do
                {
                    length = businessList?.Count;
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                    Thread.Sleep(waitTimeMilliseconds);
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                    Thread.Sleep(waitTimeMilliseconds);
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                    Thread.Sleep(waitTimeMilliseconds);
                    businessList = ToolBox.FindElementsSafe(driver, XPathUrl.businessList);
                }
                while (length != businessList?.Count);

                return businessList;
            } catch (Exception)
            {
                throw new Exception("Couldn't scroll into business urls");
            }
        }
        #endregion
    }
}
