using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace GMS.Sdk.Core.SeleniumDriver {
    #region Enum

    public enum DriverType {
        CHROME,
        FIREFOX,
        EDGE
    }
    #endregion

    public class SeleniumDriver {
        public IWebDriver WebDriver { get; set; }
        public DriverType DriverType { get; set; }

        #region Local

        /// <summary>
        /// Create an instance of Selenium Driver.
        /// </summary>
        /// <param name="driverType"></param>
        /// <param name="url"></param>
        /// <exception cref="Exception"></exception>
        public SeleniumDriver(DriverType driverType = DriverType.CHROME) {
            try {
                switch (driverType) {
                    case DriverType.FIREFOX:
                        new DriverManager().SetUpDriver(new FirefoxConfig());
                        WebDriver = new FirefoxDriver();
                        break;
                    case DriverType.EDGE:
                        new DriverManager().SetUpDriver(new EdgeConfig());
                        WebDriver = new EdgeDriver();
                        break;
                    case DriverType.CHROME:
                    default:
                        ChromeOptions chromeOptions = new();
                        //chromeOptions.AddArguments("-headless");
                        chromeOptions.AddArguments("--lang=fr");
                        new DriverManager().SetUpDriver(new ChromeConfig());
                        WebDriver = new ChromeDriver(chromeOptions);
                        break;
                }
            } catch (Exception) {
                throw new Exception("Failed initializing driver");
            }
            DriverType = driverType;
        }

        /// <summary>
        /// Accepting the cookies from the google cookie page.
        /// </summary>
        public void AcceptCookies() {
            if (!ToolBox.ToolBox.Exists(ToolBox.ToolBox.FindElementSafe(WebDriver, XPath.XPathDriver.businessList)))
                return;

            // Locating button and scrolling to it.
            IWebElement? acceptButton = ToolBox.ToolBox.FindElementSafe(WebDriver, XPath.XPathDriver.businessList);
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", acceptButton);
            Thread.Sleep(1500);

            // Clicking on accept cookies button.
            acceptButton.Click();
            Thread.Sleep(5000);
        }

        /// <summary>
        /// Navigate to url.
        /// </summary>
        /// <param name="url"></param>
        public void GetToPage(string url) {
            try {
                // Navigate to page
                WebDriver.Navigate().GoToUrl(url);
                Thread.Sleep(2000);
                AcceptCookies();
            } catch (Exception) {
                System.Diagnostics.Debug.WriteLine("Couldn't get to page.");
            }
        }
        #endregion
    }
}
