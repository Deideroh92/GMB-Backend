using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Url.Api;
using GMB.Url.Api.Models;
using GMB.Business.Api.API;

namespace GMB.Tests
{
    [TestClass]
    public class UnitTestDaily {

        #region Business Profile & Score
        /// <summary>
        /// Checking infos got from business google page
        /// </summary>
        [TestMethod]
        public async Task BusinessProfileAndScoreTest() {
            using SeleniumDriver driver = new(false);
            string url;
            DbBusinessProfile? profile;
            DbBusinessScore? score;

            // Mairie de Paris 1er
            url = "https://www.google.com/maps/place/Mairie+de+Paris/@48.8280552,2.1798986,12z/data=!4m10!1m2!2m1!1smairie+de+paris!3m6!1s0x47e66e23b4333db3:0xbc314dec89c4971!8m2!3d48.8641075!4d2.3421539!15sCg9tYWlyaWUgZGUgcGFyaXNaESIPbWFpcmllIGRlIHBhcmlzkgEJY2l0eV9oYWxsmgEjQ2haRFNVaE5NRzluUzBWSlEwRm5TVVEyYzA5MWNrbFJFQUXgAQA!16s%2Fg%2F11c6pn36ph?entry=ttu";
            (profile, score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url, null, null));
            Assert.IsNotNull(profile);
            Assert.IsTrue(profile.Name == "Mairie de Paris");
            Assert.IsTrue(profile.GoogleAddress == "40 Rue du Louvre, 75001 Paris");
            Assert.IsTrue(profile.Website == "https://www.paris.fr/");
            Assert.IsTrue(profile.Category == "Hôtel de ville");
            Assert.IsNotNull(score);
            Assert.IsNotNull(score.Score);
            Assert.IsNotNull(score.NbReviews);

            // McDonald's Champs Elysées
            url = "https://www.google.com/maps/place/McDonald's/@48.8660941,2.2928945,14z/data=!3m1!5s0x47e66fcea0d27c15:0x4fb4c9dbeb3c5271!4m10!1m2!2m1!1smcdonald!3m6!1s0x47e66fea26bafdc7:0x21ea7aaf1fb2b3e3!8m2!3d48.8731057!4d2.2992183!15sCghtY2RvbmFsZCIDiAEBWgoiCG1jZG9uYWxkkgEUZmFzdF9mb29kX3Jlc3RhdXJhbnTgAQA!16s%2Fg%2F1hd_88rdh?entry=ttu";
            (profile, score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url, null, null));
            Assert.IsNotNull(profile);
            Assert.IsTrue(profile.Name == "McDonald's");
            Assert.IsTrue(profile.GoogleAddress == "Av. des Champs-Élysées 140, 75008 Paris");
            Assert.IsTrue(profile.Website == "http://www.restaurants.mcdonalds.fr/mcdonalds-paris-champs-elysees");
            Assert.IsTrue(profile.Category == "Restauration rapide");
            Assert.IsTrue(profile.Tel.Replace(" ", "") == "0153772100");
            Assert.IsNotNull(score);
            Assert.IsNotNull(score.Score);
            Assert.IsNotNull(score.NbReviews);

            // Banque de France Bastille
            url = "https://www.google.com/maps/place/Banque+de+France/@48.8592581,2.3115198,14z/data=!4m10!1m2!2m1!1sbanque+de+france!3m6!1s0x47e67201b2c7a491:0x8debf13abf947b93!8m2!3d48.8535035!4d2.3680957!15sChBiYW5xdWUgZGUgZnJhbmNlIgOIAQGSARVmaW5hbmNpYWxfaW5zdGl0dXRpb27gAQA!16s%2Fg%2F1tf1n45r?entry=ttu";
            (profile, score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url, null, null));
            Assert.IsNotNull(profile);
            Assert.IsTrue(profile.Name == "Banque de France");
            Assert.IsTrue(profile.GoogleAddress == "CS 41834, 3 bis Pl. de la Bastille, 75183 Paris");
            Assert.IsTrue(profile.Website == "https://www.banque-france.fr/succursales/bastille-antenne-economique");
            Assert.IsTrue(profile.Category == "Institution financière");
            Assert.IsNotNull(score);
            Assert.IsNotNull(score.Score);
            Assert.IsNotNull(score.NbReviews);

            return;
        }
        #endregion

        #region Reviews
        /// <summary>
        /// Checking path to reviews
        /// </summary>
        [TestMethod]
        public void BusinessReviewsPathTest() {
            using SeleniumDriver driver = new(false);
            string url;
            IWebElement toReviewPage;
            WebDriverWait wait = new(driver.WebDriver, TimeSpan.FromSeconds(2));
            IWebElement sortButton;
            IWebElement sortButton2;

            // Mairie de Paris 1er
            url = "https://www.google.com/maps/place/Mairie+de+Paris/@48.8280552,2.1798986,12z/data=!4m10!1m2!2m1!1smairie+de+paris!3m6!1s0x47e66e23b4333db3:0xbc314dec89c4971!8m2!3d48.8641075!4d2.3421539!15sCg9tYWlyaWUgZGUgcGFyaXOSAQljaXR5X2hhbGzgAQA!16s%2Fg%2F11c6pn36ph?entry=ttu";
            driver.GetToPage(url);
            // Click on review page
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)));
            toReviewPage.Click();
            // Sort button 1
            sortButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.sortReviews)));
            sortButton.Click();
            // Sort button 2
            sortButton2 = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.sortReviews2)));
            sortButton2.Click();


            // McDonald's Champs Elysées
            url = "https://www.google.com/maps/place/McDonald's/@48.8660941,2.2928945,14z/data=!3m1!5s0x47e66fcea0d27c15:0x4fb4c9dbeb3c5271!4m10!1m2!2m1!1smcdonald!3m6!1s0x47e66fea26bafdc7:0x21ea7aaf1fb2b3e3!8m2!3d48.8731057!4d2.2992183!15sCghtY2RvbmFsZCIDiAEBWgoiCG1jZG9uYWxkkgEUZmFzdF9mb29kX3Jlc3RhdXJhbnTgAQA!16s%2Fg%2F1hd_88rdh?entry=ttu";
            driver.GetToPage(url);
            // Click on review page
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)));
            toReviewPage.Click();
            // Sort button 1
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.sortReviews)));
            sortButton.Click();
            // Sort button 2
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.sortReviews2)));
            sortButton2.Click();


            // Banque de France Bastille
            url = "https://www.google.com/maps/place/Banque+de+France/@48.8592581,2.3115198,14z/data=!4m10!1m2!2m1!1sbanque+de+france!3m6!1s0x47e67201b2c7a491:0x8debf13abf947b93!8m2!3d48.8535035!4d2.3680957!15sChBiYW5xdWUgZGUgZnJhbmNlIgOIAQGSARVmaW5hbmNpYWxfaW5zdGl0dXRpb27gAQA!16s%2Fg%2F1tf1n45r?entry=ttu";
            driver.GetToPage(url);
            // Click on review page
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.toReviewsPage)));
            toReviewPage.Click();
            // Sort button 1
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.sortReviews)));
            sortButton.Click();
            // Sort button 2
            toReviewPage = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(driver.WebDriver, XPathReview.sortReviews2)));
            sortButton2.Click();

            return;
        }

        #endregion

        #region Url
        /// <summary>
        /// Test if we can get a list of urls
        /// </summary>
        [TestMethod]
        public void GetUrlListTest() {
            List<string>? urls = new();
            List<string> locations = new() { "paris" };
            UrlRequest request = new(locations, "banque");
            using SeleniumDriver driver = new(false);

            foreach (string location in locations) {
                string textSearch = request.TextSearch + "+" + location;
                string url = "https://www.google.com/maps/search/" + textSearch;
                List <string>? urlGathered = UrlService.GetUrlsFromGooglePage(driver, url);
                if (urlGathered != null)
                    urls = Enumerable.Concat(urls, urlGathered).ToList();
            }
            
            Assert.IsNotNull(urls);
            Assert.IsTrue(urls.Count > 10);
        }
        #endregion
    }
}