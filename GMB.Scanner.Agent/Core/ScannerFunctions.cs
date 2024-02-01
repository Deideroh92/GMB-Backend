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
using System.Text.RegularExpressions;

namespace GMB.Scanner.Agent.Core
{
    public class ScannerFunctions
    {
        static void Main() { }

        #region Profile & Score
        /// <summary>
        /// Getting all business profile's infos.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="request"></param>
        /// <returns>Business Profile and a Business Score if any</returns>
        public async Task<(DbBusinessProfile?, DbBusinessScore?)> GetBusinessProfileAndScoreFromGooglePageAsync(SeleniumDriver driver, GetBusinessProfileRequest request, DbBusinessProfile? business, bool getPlusCode = true)
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
                } catch (WebDriverTimeoutException)
                {
                    return (null, null);
                }

                string? name = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim();
                if (name == null)
                    return (null, null);
                else
                    Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),\s-]+|\s{2,}", "");

                string? category = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.category)?.Text?.Replace("·", "").Trim();

                if (category == null && ToolBox.FindElementSafe(driver.WebDriver, [By.XPath("//div[text() = 'VÉRIFIER LA DISPONIBILITÉ']")])?.Text == "VÉRIFIER LA DISPONIBILITÉ")
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
                            dbBusinessProfile.Lon = addressResponse.Features[0]?.Geometry?.Coordinates[0];
                            dbBusinessProfile.Lat = addressResponse.Features[0]?.Geometry?.Coordinates[1];
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
        public static List<DbBusinessReview>? GetReviews(string idEtab, DateTime? dateLimit, SeleniumDriver driver)
        {

            try
            {
                WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(10));
                IWebElement toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@role='tab' and contains(@aria-label, 'Avis')]")));
                toReviewPage.Click();
            } catch (WebDriverTimeoutException)
            {
                return null;
            } catch (Exception e)
            {
                throw new Exception("Couldn't get to review pages", e);
            }

            Thread.Sleep(2000);

            // Sorting reviews.
            SortReviews(driver.WebDriver);

            Thread.Sleep(1000);

            // Getting reviews.
            ReadOnlyCollection<IWebElement>? reviews = GetWebElements(driver.WebDriver, dateLimit);
            List<DbBusinessReview>? businessReviews = [];

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
                            } catch
                            {
                                continue;
                            }
                        }
                        index++;
                    }
                }

                return reviewList;
            } catch (Exception e)
            {
                if (e.Message.Contains("javascript error: Cannot read properties of null (reading 'parentNode')"))
                {
                    return null;
                } else
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
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(5));

                IWebElement sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews)));
                sortButton.Click();

                IWebElement sortButton2 = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver, XPathReview.sortReviews2)));
                sortButton2.Click();
            } catch (Exception e)
            {
                throw new Exception("Couldn't sort reviews", e);
            }
        }

        public async Task<bool> WeeklyTestAsync()
        {
            ScannerFunctions scanner = new();
            SeleniumDriver driver = new();

            #region Mairie de Paris
            GetBusinessProfileRequest request = new("https://www.google.fr/maps/place/Mairie+de+Paris/@48.8660828,2.3108754,13z/data=!4m10!1m2!2m1!1smairie+de+paris!3m6!1s0x47e66e23b4333db3:0xbc314dec89c4971!8m2!3d48.8641075!4d2.3421539!15sCg9tYWlyaWUgZGUgcGFyaXNaESIPbWFpcmllIGRlIHBhcmlzkgEJY2l0eV9oYWxsmgEjQ2haRFNVaE5NRzluUzBWSlEwRm5TVVEyYzA5MWNrbFJFQUXgAQA!16s%2Fg%2F11c6pn36ph?hl=fr&entry=ttu");
            (DbBusinessProfile? profile, DbBusinessScore? score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Mairie de Paris" ||
                (profile.GoogleAddress != "40 Rue du Louvre, 75001 Paris" && profile.GoogleAddress != "40 Rue du Louvre, 75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.StreetNumber != "40" ||
                profile.Category != "Hôtel de ville" ||
                profile.Website != "https://www.paris.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNbLMIrgq3do3iVaTPocWsR6mluAc1wNNKW-62h=w426-h240-k-no" ||
                profile.PlusCode != "8FW4V87R+JV" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Louvre
            request = new("https://www.google.fr/maps/place/Mus%C3%A9e+du+Louvre/@48.8606111,2.337644,17z/data=!3m1!4b1!4m6!3m5!1s0x47e671d877937b0f:0xb975fcfa192f84d4!8m2!3d48.8606111!4d2.337644!16zL20vMDRnZHI?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Musée du Louvre" ||
                (profile.GoogleAddress != "75001 Paris" && profile.GoogleAddress != "75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.Category != "Musée d'art" ||
                profile.Website != "https://www.louvre.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipM_ApMgFfAP8CP2ZHJUOb13K7P_SqSkW9sh9MFY=w408-h272-k-no" ||
                profile.PlusCode != "8FW4V86Q+63" ||
                profile.Tel != "01 40 20 53 17" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Gare Montparnasse
            request = new("https://www.google.fr/maps/place/Gare+Montparnasse/@48.8411382,2.3205261,17z/data=!3m1!4b1!4m6!3m5!1s0x47e67034ac4d4559:0xe467cc61460dc234!8m2!3d48.8411382!4d2.3205261!16zL20vMDE2anY4?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Gare Montparnasse" ||
                (profile.GoogleAddress != "17 Bd de Vaugirard, 75015 Paris" && profile.GoogleAddress != "17 Bd de Vaugirard, 75015 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75115" ||
                profile.StreetNumber != "17" ||
                profile.Country != "France" ||
                profile.Category != "Station de transit" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipOvFa1JfgCp9tJGtq0DWviVwgyl5TKMcR_6CEvF=w426-h240-k-no" ||
                profile.PlusCode != "8FW4R8RC+F6" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Gare de Bordeaux
            request = new("https://www.google.fr/maps/place/Bordeaux+Saint-Jean/@44.8332874,-0.6118573,14z/data=!4m10!1m2!2m1!1sgare+de+bordeaux!3m6!1s0xd552648d40fc247:0x2bc94166a0a4eed6!8m2!3d44.8264574!4d-0.5560982!15sChBnYXJlIGRlIGJvcmRlYXV4kgENdHJhaW5fc3RhdGlvbuABAA!16s%2Fg%2F1pxyc0w63?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Bordeaux Saint-Jean" ||
                (profile.GoogleAddress != "Rue Charles Domercq, 33800 Bordeaux" && profile.GoogleAddress != "Rue Charles Domercq, 33800 Bordeaux, France") ||
                profile.City != "Bordeaux" ||
                profile.Country != "France" ||
                profile.Category != "Gare" ||
                profile.Website != "http://www.gares-sncf.com/fr/gare/frboj/bordeaux-saint-jean" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNnUmG5q3sauygk9_d62NoPVFHDP2q77pdFDylN=w426-h240-k-no" ||
                profile.PlusCode != "8CPXRCGV+HH" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Musée de l'Orangerie
            request = new("https://www.google.fr/maps/place/Mus%C3%A9e+de+l'Orangerie/@48.8637884,2.3226724,17z/data=!3m1!4b1!4m6!3m5!1s0x47e66e2eeaaaaaa3:0xdc3fd08aa701960a!8m2!3d48.8637884!4d2.3226724!16zL20vMGR0M21s?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Musée de l'Orangerie" ||
                (profile.GoogleAddress != "Jardin des Tuileries, 75001 Paris" && profile.GoogleAddress != "Jardin des Tuileries, 75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.Category != "Musée d'art" ||
                profile.Website != "https://www.musee-orangerie.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipONXsHpu5d_TUKzMzGYv4nQECCvQvNYQA7iwMJz=w408-h270-k-no" ||
                profile.PlusCode != "8FW4V87F+G3" ||
                profile.Tel != "01 44 50 43 00" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Tour Eiffel
            request = new("https://www.google.fr/maps/place/Tour+Eiffel/@48.8583701,2.2944813,17z/data=!3m1!4b1!4m6!3m5!1s0x47e66e2964e34e2d:0x8ddca9ee380ef7e0!8m2!3d48.8583701!4d2.2944813!16zL20vMDJqODE?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Tour Eiffel" ||
                (profile.GoogleAddress != "Champ de Mars, 5 Av. Anatole France, 75007 Paris" && profile.GoogleAddress != "Champ de Mars, 5 Av. Anatole France, 75007 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75107" ||
                profile.Country != "France" ||
                profile.Category != "Site historique" ||
                profile.Website != "https://www.toureiffel.paris/fr" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipOSojyYuemoPo1TwH7J6mFC35Y89oKXMHgLIK7-=w408-h468-k-no" ||
                profile.PlusCode != "8FW4V75V+8Q" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Hôpital Necker
            request = new("https://www.google.fr/maps/place/H%C3%B4pital+Necker+AP-HP/@48.8452199,2.3157461,17z/data=!3m1!4b1!4m6!3m5!1s0x47e6703221308f89:0x57a7e5b303e7d9!8m2!3d48.8452199!4d2.3157461!16s%2Fm%2F03gzggj?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Hôpital Necker AP-HP" ||
                (profile.GoogleAddress != "149 Rue de Sèvres, 75015 Paris" && profile.GoogleAddress != "149 Rue de Sèvres, 75015 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75115" ||
                profile.Country != "France" ||
                profile.Category != "Hôpital pour enfants" ||
                profile.StreetNumber != "149" ||
                profile.Website != "http://www.hopital-necker.aphp.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipMnIqT5LZxbYjW8uj7PooQPlFrhQa_8HloZMmvz=w426-h240-k-no" ||
                profile.PlusCode != "8FW4R8W8+37" ||
                profile.Tel != "01 44 49 40 00" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Le Parc des Princes
            request = new("https://www.google.fr/maps/place/Le+Parc+des+Princes/@48.8414356,2.2530484,17z/data=!3m1!4b1!4m6!3m5!1s0x47e67ac09948a18d:0xdd2450406cef2c5c!8m2!3d48.8414356!4d2.2530484!16zL20vMDM5NXNs?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Le Parc des Princes" ||
                (profile.GoogleAddress != "24 Rue du Commandant Guilbaud, 75016 Paris" && profile.GoogleAddress != "24 Rue du Commandant Guilbaud, 75016 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75116" ||
                profile.Country != "France" ||
                profile.StreetNumber != "24" ||
                profile.Category != "Stade" ||
                profile.Website != "http://www.psg.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNlF3ZAxh0AbfqGaAEHZKCuqbmDmVUbCEBoOdPa=w408-h254-k-no" ||
                profile.PlusCode != "8FW4R7R3+H6" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Banque de France
            request = new("https://www.google.fr/maps/place/Banque+de+France/@48.8642403,2.3395915,17z/data=!3m1!4b1!4m6!3m5!1s0x47e66e247d40c1cb:0xd9c85f8e2e769217!8m2!3d48.8642403!4d2.3395915!16s%2Fg%2F1w2yt5rt?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Banque de France" ||
                (profile.GoogleAddress != "31 Rue Croix des Petits Champs, 75001 Paris" && profile.GoogleAddress != "31 Rue Croix des Petits Champs, 75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.StreetNumber != "31" ||
                profile.Category != "Banque" ||
                profile.Website != "https://www.banque-france.fr/la-banque-de-france/missions/protection-du-consommateur/surendettement.html" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipMUP0ngLIPjME6aQ2uynmPAMv5Sd9ldDFj9QcX2=w408-h272-k-no" ||
                profile.PlusCode != "8FW4V87Q+MR" ||
                profile.Tel != "01 42 92 42 92" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Roland-Garros
            request = new("https://www.google.fr/maps/place/Stade+Roland-Garros/@48.8459632,2.2525486,18z/data=!4m10!1m2!2m1!1sroland+garros!3m6!1s0x47e67ac59c999975:0x9d40606b40c66989!8m2!3d48.8459632!4d2.2538361!15sCg1yb2xhbmQgZ2Fycm9zWg8iDXJvbGFuZCBnYXJyb3OSAQ5zcG9ydHNfY29tcGxleOABAA!16zL20vMDlodHQy?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Stade Roland-Garros" ||
                (profile.GoogleAddress != "2 Av. Gordon Bennett, 75016 Paris" && profile.GoogleAddress != "2 Av. Gordon Bennett, 75016 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75116" ||
                profile.Country != "France" ||
                profile.StreetNumber != "2" ||
                profile.Category != "Complexe sportif" ||
                profile.Website != "http://www.rolandgarros.com/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipMjz4HbqL38hD7PWlS7FvCP39ko48rW75Z83djk=w408-h272-k-no" ||
                profile.PlusCode != "8FW4R7W3+9G" ||
                profile.Tel != "01 47 43 48 00" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            #region Maxim's
            request = new("https://www.google.fr/maps/place/Maxim's/@48.8674428,2.3032712,15z/data=!4m10!1m2!2m1!1smaxim's!3m6!1s0x47e66fcd4e473877:0xcf18f93e84c578c5!8m2!3d48.8674428!4d2.3223256!15sCgdtYXhpbSdzWgkiB21heGltJ3OSAQpyZXN0YXVyYW504AEA!16zL20vMDl6a3Ix?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Maxim's" ||
                (profile.GoogleAddress != "3 Rue Royale, 75008 Paris" && profile.GoogleAddress != "3 Rue Royale, 75008 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75108" ||
                profile.Country != "France" ||
                profile.StreetNumber != "3" ||
                profile.Category != "Restaurant" ||
                profile.Website != "https://restaurant-maxims.com/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNXcE3vwR3faD97mBv7BmuA_G2A2O0a07QpCT8_=w408-h272-k-no" ||
                profile.PlusCode != "8FW4V88C+XW" ||
                profile.Tel != "01 42 65 27 94" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                return false;
            #endregion

            driver.Dispose();

            return true;
        }
        #endregion

        #region Url
        /// <summary>
        /// Initiate the getting url process.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="urlToPage"></param>
        public List<string>? GetUrlsFromGooglePage(SeleniumDriver driver, string urlToPage)
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
