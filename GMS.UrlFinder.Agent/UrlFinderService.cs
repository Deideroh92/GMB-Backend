using GMS.Sdk.Core;
using GMS.Sdk.Core.DbModels;
using GMS.Sdk.Core.Models;
using GMS.Url.Agent.Model;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace GMS.Url.Agent
{
    /// <summary>
    /// Url Service.
    /// </summary>
    public class UrlFinderService {
        /// <summary>
        /// Start the Service.
        /// </summary>
        /// <param name="request"></param>
        public static void Start(UrlFinderRequest request) {
            using DbLib dbLib = new();
            using SeleniumDriver driver = new();
            foreach (string location in request.Locations) {
                try {
                    string textSearch = request.TextSearch + "+" + location;
                    string url = "https://www.google.com/maps/search/" + textSearch;
                    driver.GetToPage(url);
                    GetUrls(driver.WebDriver, textSearch, dbLib);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }


        /// <summary>
        /// Initiate the getting url process.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="textSearch"></param>
        /// <param name="dbLib"></param>
        private static void GetUrls(IWebDriver driver, string textSearch, DbLib dbLib) {
            ReadOnlyCollection<IWebElement>? businessList = ScrollIntoBusinessUrls(driver);
            if (businessList == null && ToolBox.FindElementSafe(driver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim() != null) {
                ValidateUrl(driver.Url, dbLib, textSearch);
            }


            foreach (IWebElement business in businessList) {
                string name = business.Text.Split('\n')[0].Replace("\r", "");
                string? url = ToolBox.FindElementSafe(business, new List<By> { By.XPath(".//a[contains(@aria-label, '" + name + "')]") })?.GetAttribute("href").Replace("?authuser=0&hl=fr&rclk=1", "");

                if (string.IsNullOrWhiteSpace(url))
                    continue;
                ValidateUrl(url, dbLib, textSearch);
            }
        }

        /// <summary>
        /// Validate url : add or not to database.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dbLib"></param>
        /// <param name="textSearch"></param>
        private static void ValidateUrl(string url, DbLib dbLib, string textSearch) {
            try {
                if (!dbLib.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(url))) {
                    using DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, DateTime.UtcNow, textSearch, DateTime.UtcNow, ToolBox.ComputeMd5Hash(url));
                    dbLib.CreateBusinessUrl(businessUrl);
                }
            } catch (Exception ex) {
                Console.WriteLine($"Erreur lors de la création de l'URL de l'entreprise : {ex.Message}");
            }
        }

        /// <summary>
        /// Scrolling through the page to get all businesses.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns>A list of all businesses that we could gather.</returns>
        private static ReadOnlyCollection<IWebElement>? ScrollIntoBusinessUrls(IWebDriver driver) {
            IWebElement? body = ToolBox.FindElementSafe(driver, XPathUrl.body);
            ReadOnlyCollection<IWebElement>? businessList = ToolBox.FindElementsSafe(driver, XPathUrl.businessList);

            if (body == null || businessList == null)
                return null;

            int? length;
            const int waitTimeMilliseconds = 1000;

            do {
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
        }
    }
}
