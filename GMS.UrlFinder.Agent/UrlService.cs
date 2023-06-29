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
    public class UrlService {
        /// <summary>
        /// Start the Service.
        /// </summary>
        /// <param name="request"></param>
        public static void Start(UrlAgentRequest request) {
            using DbLib dbLib = new();
            using SeleniumDriver driver = new(DriverType.CHROME);
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

        private static void GetUrls(IWebDriver driver, string textSearch, DbLib dbLib) {
            UrlState urlState = UrlState.NEW;
            string name;
            string url;

            ReadOnlyCollection<IWebElement>? businessList = ScrollIntoBusinessUrls(driver);
            if (businessList == null)
                return;

            foreach (IWebElement business in businessList) {
                name = business.Text.Split('\n')[0].Replace("\r", "");
                if (!ToolBox.Exists(ToolBox.FindElementSafe(business, new List<By>((IEnumerable<By>)By.XPath(".//a[contains(@aria-label, '" + name + "')]")))))
                    continue;

                url = ToolBox.FindElementSafe(business, new List<By>((IEnumerable<By>)By.XPath(".//a[contains(@aria-label, '" + name + "')]"))).GetAttribute("href").Replace("?authuser=0&hl=fr&rclk=1", "");

                try {
                    if (!dbLib.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(url))) {
                        DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, DateTime.UtcNow, urlState, textSearch, DateTime.UtcNow, ToolBox.ComputeMd5Hash(url));
                        dbLib.CreateBusinessUrl(businessUrl);
                        Console.WriteLine("New url added in DB.");
                    } else {
                        Console.WriteLine("Url exists in DB.");
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private static ReadOnlyCollection<IWebElement>? ScrollIntoBusinessUrls(IWebDriver driver) {
            if (!ToolBox.Exists(ToolBox.FindElementSafe(driver, XPathUrl.body)))
                return null;

            if (!ToolBox.Exists(ToolBox.FindElementsSafe(driver, XPathUrl.businessList)))
                return null;

            IWebElement? body = ToolBox.FindElementSafe(driver, XPathUrl.body);
            ReadOnlyCollection<IWebElement>? businessList = ToolBox.FindElementsSafe(driver, XPathUrl.businessList);
            int? length;

            do {
                length = businessList?.Count();
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                Thread.Sleep(1000);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                Thread.Sleep(1000);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                Thread.Sleep(1000);
                businessList = ToolBox.FindElementsSafe(driver, XPathUrl.businessList);
            }
            while (length != businessList?.Count());

            return businessList;
        }
    }
}
