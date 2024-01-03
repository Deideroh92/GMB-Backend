using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace GMB.Sdk.Core.Types.Models
{

    public class SeleniumDriver : IDisposable
    {
        public IWebDriver WebDriver { get; set; }

        #region Local
        /// <summary>
        /// Create an instance of Selenium Driver.
        /// </summary>
        public SeleniumDriver(bool headless = true)
        {
            try
            {
                ChromeOptions chromeOptions = new();
                chromeOptions.AddArguments("--headless=new");
                chromeOptions.AddArguments("--lang=fr");
                if (headless)
                {
                    chromeOptions.AddArguments("--disable-gpu");
                    chromeOptions.AddArguments("--no-sandbox");
                    chromeOptions.AddArguments("--disable-dev-shm-usage");
                }
                new DriverManager().SetUpDriver(new ChromeConfig());
                WebDriver = new ChromeDriver(chromeOptions);
            } catch (Exception)
            {
                throw new Exception("Failed initializing driver");
            }
        }

        /// <summary>
        /// Accepting the cookies from the google cookie page.
        /// </summary>
        public void AcceptCookies()
        {
            // Locating button and scrolling to it.
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", ToolBox.FindElementSafe(WebDriver, new List<By>(XPathDriver.acceptCookies)));

            // Clicking on accept cookies button.
            WebDriverWait wait = new(WebDriver, TimeSpan.FromSeconds(2));
            IWebElement acceptButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(WebDriver, XPathDriver.acceptCookies)));
            acceptButton.Click();
            Thread.Sleep(3000);
        }

        /// <summary>
        /// Navigate to url.
        /// </summary>
        /// <param name="url"></param>
        public void GetToPage(string url)
        {
            try
            {
                // Navigate to page
                WebDriver.Navigate().GoToUrl(url);
                Thread.Sleep(2000);
                AcceptCookies();
            } catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Couldn't get to page.");
            }
        }
        /// <summary>
        /// Checking if driver is still responding.
        /// </summary>
        /// <returns>True if it does, else false.</returns>
        public bool IsDriverAlive()
        {
            try
            {
                // Execute a simple command to check if the driver is still responsive
                return WebDriver.Title != null;
            } catch (WebDriverException)
            {
                // WebDriverException is thrown if the driver is not responsive
                return false;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WebDriver?.Quit();
                WebDriver?.Dispose();
            }
        }
        #endregion
    }
}
