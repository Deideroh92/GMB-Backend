using GMS.Sdk.Core.SeleniumDriver;

namespace GMS.UrlFinder.Agent
{
    public enum XPATH
    {

    }

    /// <summary>
    /// Url Finder Service
    /// </summary>
    public class UrlFinderService
    {
        public static void getUrl()
        {
            SeleniumDriver driver = new(DriverType.CHROME);
            string textSearch = "bred+92100";

            driver.WebDriver.Navigate().GoToUrl("https://www.google.com/maps/search/" + textSearch);
            Thread.Sleep(2000000);
        }
    }
}