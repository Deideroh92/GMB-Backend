using OpenQA.Selenium;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Url.Api;
using GMB.Business.Api.Models;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Url.Api.Models;
using Serilog;
using GMB.Business.Api.API;
using GMB.BusinessService.Api.Controllers;

namespace GMB.Tests
{
    [TestClass]
    public class Launch {

        public static readonly string pathUrlFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathUrlKnownFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\urlKnown.txt";
        public static readonly string pathUnknownBusinessFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\unknown_url.txt";
        private static readonly string logsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Tests\logs";

        #region Url
        /// <summary>
        /// Launch URL Scanner
        /// </summary>
        [TestMethod]
        public void ThreadsUrlScraper() {
            string[] categories = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "Categories.txt"));
            string[] textSearch = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "UrlTextSearch.txt"));
            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "IleDeFrance.txt"));
            string[] cp = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "CpList.txt"));
            string[] customLocations = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "CustomLocations.txt"));

            List<string> locations = new(cp);
            List<Task> tasks = new();

            int maxConcurrentThreads = 1;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            foreach (string search in categories) {
                UrlRequest request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Trim().Replace(' ', '+'));
                Task newThread = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Wait until there's an available slot to run
                    try
                    {
                        UrlController.Scanner(request);
                    } finally
                    {
                        semaphore.Release(); // Release the slot when the task is done
                    }
                });
                tasks.Add(newThread);
            }

            Task.WaitAll(tasks.ToArray());
            return;
        }

        /// <summary>
        /// Create a BU in DB.
        /// </summary>
        [TestMethod]
        public void CreateUrl() {
            string url = "";
            UrlController.CreateUrl(url);
        }
        #endregion

        #region Business
        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public void SetProcessingFromIdEtabFile()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\urls.txt";
            using DbLib db = new();

            List<string> values = new();

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
        public async Task ExportHotelAsync() {

            // CONFIG
            int nbEntries = 1;
            UrlState urlState = UrlState.NEW;

            using DbLib db = new();
            List<BusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, nbEntries);

            using SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            using StreamWriter sw2 = File.AppendText(@"C:\Users\maxim\Desktop\hotel.txt");
            sw2.WriteLine("NAME$$CATEGORY$$ADRESS$$TEL$$OPTIONS");
            foreach (BusinessAgent elem in businessList) {
                try {
                    GetBusinessProfileRequest request = new(elem.Url, null, null);
                    (DbBusinessProfile? business, DbBusinessScore? businessScore) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
                    var optionsOn = ToolBox.FindElementsSafe(driver.WebDriver, XPathProfile.optionsOn);
                    List<string> optionsOnList = new();
                    foreach (IWebElement element in optionsOn) {
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
        public async Task ThreadsBusinessScraper() {
            List<BusinessAgent> businessList = new();
            List<Task> tasks = new();
            using DbLib db = new();
            int threadNumber = 0;

            int entries = 40000;
            int processing = 1;
            Operation operationType = Operation.OTHER;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddMonths(-12);
            BusinessController controller = new();

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            switch (operationType) {
                case Operation.OTHER:
                    string? brand = null;
                    string? category = null;
                    CategoryFamily? categoryFamily = null;
                    bool isNetwork = true;
                    bool isIndependant = false;
                    GetBusinessListRequest request = new(entries, processing, brand, category, categoryFamily, isNetwork, isIndependant);
                    businessList = db.GetBusinessAgentList(request);
                    break;
                case Operation.FILE:
                    bool isUrlKnownFile = false;
                    bool isUrlFile = true;
                    string[]? urlList = File.ReadAllLines(pathUrlFile);

                    if (isUrlKnownFile) {
                        foreach (string url in urlList) {
                            BusinessAgent? business = db.GetBusinessAgentByUrlEncoded(ToolBox.ComputeMd5Hash(url));

                            if (business == null) {
                                Log.Error(url);
                                continue;
                            }

                            businessList.Add(business);
                        }
                    } else {
                        foreach (string url in urlList) {
                            if (!isUrlFile)
                                businessList.Add(new BusinessAgent(null, "https://www.google.fr/maps/search/" + url.ToLower()));
                            else
                                businessList.Add(new BusinessAgent(null, url));
                        }
                    }
                    break;
                case Operation.URL_STATE:
                    UrlState urlState = UrlState.NEW;
                    businessList = db.GetBusinessAgentListByUrlState(urlState, entries);
                    break;

                default: break;
            }

            int nbThreads = 8;

            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads))
            {
                threadNumber++;
                Task newThread = Task.Run(async () =>
                {
                    BusinessAgentRequest request = new(operationType, getReviews, new List<BusinessAgent>(chunk), reviewsDate);
                    await controller.Scanner(request).ConfigureAwait(false);
                });
                tasks.Add(newThread);
            }
            await Task.WhenAll(tasks);
            return;
        }
        #endregion
    }
}