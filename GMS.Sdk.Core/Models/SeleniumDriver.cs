﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace GMS.Sdk.Core.Models
{
    #region Enum

    public enum DriverType {
        CHROME,
        FIREFOX,
        EDGE
    }

    #endregion

    public class SeleniumDriver : IDisposable {
        public IWebDriver WebDriver { get; set; }
        public DriverType DriverType { get; set; }

        #region Local

        /// <summary>
        /// Create an instance of Selenium Driver.
        /// </summary>
        /// <param name="driverType"></param>
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
                        chromeOptions.AddArguments("--headless=new");
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

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                WebDriver?.Quit();
                WebDriver?.Dispose();
            }
        }

        ~SeleniumDriver() {
            Dispose(false);
        }

        #endregion
    }
}