using GMB.BusinessService.Api.Controller;
using GMB.BusinessService.Api.Models;
using GMB.Scanner.Agent.Core;
using GMB.Scanner.Agent.Models;
using GMB.ScannerService.Api.Controller;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.Globalization;

namespace GMB.Tests
{
    [TestClass]
    public class Launch
    {

        public static readonly string pathUrlFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathUrlKnownFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\urlKnown.txt";
        public static readonly string pathUnknownBusinessFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\unknown_url.txt";
        private static readonly string logsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Tests\logs";

        #region Url
        /// <summary>
        /// Launch URL Scanner
        /// </summary>
        [TestMethod]
        public void ThreadsUrlScraper()
        {
            string[] categories = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "Categories.txt"));
            string[] textSearch = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "UrlTextSearch.txt"));
            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "IleDeFrance.txt"));
            string[] cp = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "CpList.txt"));
            string[] customLocations = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "CustomLocations.txt"));

            List<string> locations = new(cp);
            List<Task> tasks = [];

            int maxConcurrentThreads = 1;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            foreach (string search in categories)
            {
                ScannerUrlRequest request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Trim().Replace(' ', '+'));
                Task newThread = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Wait until there's an available slot to run
                    try
                    {
                        Scanner.Agent.Scanner.ScannerUrl(request);
                    } finally
                    {
                        semaphore.Release(); // Release the slot when the task is done
                    }
                });
                tasks.Add(newThread);
            }

            Task.WaitAll([.. tasks]);
            return;
        }

        /// <summary>
        /// Create a BU in DB.
        /// </summary>
        [TestMethod]
        public void CreateUrl()
        {
            string url = "";
            BusinessController controller = new();
            controller.CreateUrl(url);
        }
        #endregion

        #region Business
        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public void SetProcessingFromIdEtabFile()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\Custom.txt";
            using DbLib db = new();

            List<string> values = [];

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        values.Add(line);
                }
            }

            foreach (string idEtab in values)
            {
                db.UpdateBusinessProfileProcessingState(idEtab, 1);
            }
        }
        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public void GetIdEtabByPlaceId()
        {
            string[] placeIds = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "urls.txt"));
            using DbLib db = new();
            DbBusinessProfile? bp = null;
            using StreamWriter sw2 = File.AppendText(@"C:\Users\maxim\Desktop\test.txt");

            foreach (var id in placeIds)
            {
                bp = db.GetBusinessByPlaceId(id);
                if (bp != null)
                    sw2.WriteLine(bp.IdEtab);
                else
                    sw2.WriteLine(id);
            }
        }
        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public async Task ExportHotelAsync()
        {

            // CONFIG
            int nbEntries = 1;
            UrlState urlState = UrlState.NEW;

            using DbLib db = new();
            List<BusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, nbEntries);
            ScannerFunctions toolbox = new();

            using SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            using StreamWriter sw2 = File.AppendText(@"C:\Users\maxim\Desktop\hotel.txt");
            sw2.WriteLine("NAME$$CATEGORY$$ADRESS$$TEL$$OPTIONS");
            foreach (BusinessAgent elem in businessList)
            {
                try
                {
                    GetBusinessProfileRequest request = new(elem.Url, null, null);
                    (DbBusinessProfile? business, DbBusinessScore? businessScore) = await toolbox.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
                    var optionsOn = ToolBox.FindElementsSafe(driver.WebDriver, XPathProfile.optionsOn);
                    List<string> optionsOnList = [];
                    foreach (IWebElement element in optionsOn)
                    {
                        optionsOnList.Add(element.GetAttribute("aria-label").Replace("L'option ", "").Replace(" est disponible", ""));
                    }
                    sw2.WriteLine(business.Name + "$$" + business.Category + "$$" + business.GoogleAddress + "$$" + business.Tel + "$$" + string.Join(",", optionsOnList));
                } catch (Exception) { }
            }
        }
        /// <summary>
        /// Starting Scanner.
        /// </summary>
        [TestMethod]
        public async void LaunchBusinessScanner()
        {
            ScannerController controller = new();
            BusinessScannerRequest request = new(100000, 1, Operation.PROCESSING_STATE, true, DateTime.UtcNow.AddMonths(2), true);

            await controller.StartBusinessScannerAsync(request);
            return;
        }
        #endregion
    }
}