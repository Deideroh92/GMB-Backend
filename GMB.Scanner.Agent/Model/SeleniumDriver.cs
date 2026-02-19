using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace GMB.Sdk.Core.Types.Models
{

    public class SeleniumDriver : IDisposable
    {
        public IWebDriver WebDriver { get; set; }
        public string Id { get; set; }

        #region Local
        /// <summary>
        /// Create an instance of Selenium Driver.
        /// </summary>
        public SeleniumDriver(int? number = null)
        {
            try
            {
                Id = Guid.NewGuid().ToString("N");
                ChromeOptions chromeOptions = new();

                chromeOptions.AddArguments(
                    "--headless=new",
                    "--lang=fr",
                    "--accept-lang=fr-FR,fr",
                    "--window-size=1920,1200",

                    // Performance
                    "--disable-background-networking",
                    "--disable-background-timer-throttling",
                    "--disable-renderer-backgrounding",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-features=Translate,BackForwardCache",
                    "--disable-sync",
                    "--metrics-recording-only",
                    "--mute-audio",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                    "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                    "Chrome/127.0.0.1 Safari/537.36",

                    // Required by you
                    "scriptpid-" + Id
                );

                if (number != null)
                {
                    chromeOptions.AddArgument(@"--user-data-dir=C:\ChromeProfiles\Profile" + number);
                }

                WebDriver = new ChromeDriver(chromeOptions);
                var js = (IJavaScriptExecutor)WebDriver;
                js.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

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
            try
            {
                // Locating button and scrolling to it.
                ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", ToolBox.FindElementSafe(WebDriver, new List<By>(XPathDriver.acceptCookies)));

                // Clicking on accept cookies button.
                WebDriverWait wait = new(WebDriver, TimeSpan.FromSeconds(2));
                IWebElement acceptButton = wait.Until(ExpectedConditions.ElementToBeClickable(ToolBox.FindElementSafe(WebDriver, XPathDriver.acceptCookies)));
                acceptButton.Click();
                Thread.Sleep(3000);
            } catch (Exception)
            {
                Debug.WriteLine("No cookie page.");
            }
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
                Debug.WriteLine("Couldn't get to page.");
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
            WebDriver.Close();
            WebDriver?.Quit();
            //Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                KillProcesses();
                WebDriver?.Quit();
                WebDriver?.Dispose();
            }
        }

        protected virtual void KillProcesses()
        {
            try
            {
                // Find and kill Chrome processes by the given process ID
                var chromeProcesses = GetChromeProcessesByDriverId();

                foreach (var process in chromeProcesses)
                {
                    ManagementObjectSearcher commandLineSearcher = new("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
                    String commandLine = "";
                    foreach (ManagementObject commandLineObject in commandLineSearcher.Get().Cast<ManagementObject>())
                    {
                        commandLine += (string)commandLineObject["CommandLine"];
                    }

                    string script_pid_str = (new Regex("--scriptpid-(.+?) ")).Match(commandLine).Groups[1].Value;

                    if (script_pid_str == Id)
                        process.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing Chrome processes: {ex.Message}");
            }
        }

        static Process[] GetChromeProcessesByDriverId()
        {
            try
            {
                var chromeProcesses = Process.GetProcessesByName("chrome").ToArray();

                return chromeProcesses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Chrome processes: {ex.Message}");
                return [];
            }
        }
        #endregion
    }
}
