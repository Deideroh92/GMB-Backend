using GMS.Sdk.Core;
using GMS.Sdk.Core.Types.Models;
using OpenQA.Selenium;
using Serilog;
using System.Collections.ObjectModel;
using System.Web;

namespace GMS.Url.Api
{
    /// <summary>
    /// Url Service.
    /// </summary>
    public class UrlService {

        /// <summary>
        /// Initiate the getting url process.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="urlToPage"></param>
        public static List<string>? GetUrlsFromGooglePage(SeleniumDriver driver, string urlToPage) {

            List<string> urls = new();

            driver.GetToPage(urlToPage);

            ReadOnlyCollection<IWebElement>? businessList = ScrollIntoBusinessUrls(driver.WebDriver);

            // Single page
            if (businessList == null && ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.name)?.GetAttribute("aria-label")?.Trim() != null) {
                urls.Add(driver.WebDriver.Url);
                return urls;
            }

            foreach (IWebElement business in businessList) {
                try {
                    string name = business.Text.Split('\n')[0].Replace("\r", "");
                    string? url = ToolBox.FindElementSafe(business, new List<By> { By.XPath(".//a[contains(@aria-label, \"" + name + "\")]") })?.GetAttribute("href").Replace("?authuser=0&hl=fr&rclk=1", "");

                    if (string.IsNullOrWhiteSpace(url))
                        continue;
                    urls.Add(url);
                } catch (Exception e) {
                    Log.Error(e, "An exception occurred while collection a business url: {Message}");
                }
            }
            return urls;
        }

        /// <summary>
        /// Scrolling through the page to get all businesses.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns>A list of all businesses that we could gather.</returns>
        private static ReadOnlyCollection<IWebElement>? ScrollIntoBusinessUrls(IWebDriver driver) {
            try {
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
            } catch (Exception) {
                throw new Exception("Couldn't scroll into business urls");
            }
        }
    }
}
