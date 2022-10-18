using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Sdk.Core.XPath;
using GMS.Url.Agent.Model;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace GMS.Url.Agent {

    /// <summary>
    /// Url Service.
    /// </summary>
    public class UrlService {

        #region Local

        /// <summary>
        /// Start the Service.
        /// </summary>
        /// <param name="request"></param>
        public static void Start(UrlAgentRequest request) {
            DbLib dbLib = new();
            SeleniumDriver driver = new(DriverType.CHROME);
            try {
                driver.GetToPage(request.TextSearch);
                GetUrls(driver.WebDriver, request.TextSearch, dbLib);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

            driver.WebDriver.Quit();
            // Disconnect from DB.
            dbLib.DisconnectFromDB();
        }

        /// <summary>
        /// Getting url into DB from Google Page.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="textSearch"></param>
        /// <param name="isTest"></param>
        public static void GetUrls(IWebDriver driver, string textSearch, DbLib dbLib) {
            // Variables initialization.
            UrlState urlState = UrlState.NEW;
            string name;
            string url;

            // Scrolling to the end of the page to get all businesses loaded.
            ScrollIntoBusinessUrls(driver);

            // Getting url of every businesses.
            if (!ToolBox.Exists(ToolBox.FindElementsSafe(driver, XPathUrl.businessList)))
                return;
            
            ReadOnlyCollection<IWebElement>? businessList = ToolBox.FindElementsSafe(driver, XPathUrl.businessList);
            foreach (IWebElement business in businessList) {
                name = business.Text.Split('\n')[0].Replace("\r", "");
                if (!ToolBox.Exists(ToolBox.FindElementSafe(business, By.XPath(".//a[contains(@aria-label, '" + name + "')]"))))
                    break;
                url = ToolBox.FindElementSafe(business, By.XPath(".//a[contains(@aria-label, '" + name + "')]")).GetAttribute("href").Replace("?authuser=0&hl=fr&rclk=1", "");
                DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, DateTime.UtcNow, urlState, textSearch, DateTime.UtcNow, ToolBox.ComputeMd5Hash(url));

                try {
                    if (!dbLib.CheckBusinessUrlExist(businessUrl.UrlEncoded)) {
                        dbLib.InsertBusinessUrl(businessUrl);
                        System.Diagnostics.Debug.WriteLine("New url added in DB.");
                    } else
                        System.Diagnostics.Debug.WriteLine("Url exists in DB.");
                } catch(Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Scroll into business list.
        /// </summary>
        /// <param name="driver"></param>
        public static void ScrollIntoBusinessUrls(IWebDriver driver) {
            if (!ToolBox.Exists(ToolBox.FindElementSafe(driver, XPathUrl.body)))
                return;

            IWebElement? body = ToolBox.FindElementSafe(driver, XPathUrl.body);
            do {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", body);
                Thread.Sleep(1000);
            }
            while (!ToolBox.Exists(ToolBox.FindElementSafe(driver, XPathUrl.endOfList)));
        }
        #endregion
    }
}