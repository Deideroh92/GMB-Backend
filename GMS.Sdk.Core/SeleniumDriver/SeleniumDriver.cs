using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace GMS.Sdk.Core.SeleniumDriver
{
    #region Enum

    public enum DriverType
    {
        CHROME,
        FIREFOX,
        EDGE
    }
    #endregion

    public class SeleniumDriver
    {
        public IWebDriver WebDriver { get; set; }
        public DriverType DriverType { get; set; }

        #region Local

        /// <summary>
        /// Create an instance of Selenium Driver.
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="driverType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SeleniumDriver(DriverType driverType)
        {
            switch(driverType)
            {
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
                    new DriverManager().SetUpDriver(new ChromeConfig());
                    WebDriver = new ChromeDriver();
                    break;
            }
            
            DriverType = driverType;
        }

        public static void Start()
        {
        }
        #endregion
    }
}
