using OpenQA.Selenium;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMS.Sdk.Core;
using GMS.Sdk.Core.Types.Models;
using GMS.Sdk.Core.Types.Database.Manager;
using GMS.Url.Api;
using GMS.Business.Api;
using GMS.Business.Api.Models;
using GMS.Sdk.Core.Types.Database.Models;
using GMS.Url.Api.Models;
using GMS.Business.Api.Controllers;

namespace GMS.Tests
{
    [TestClass]
    public class Launch {

        public static readonly string pathUrlKnownFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathUnknownBusinessFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\unknown_url.txt";
        public static readonly string pathLogFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Logs\Business-Agent\log-" + DateTime.Today.ToString("MM-dd-yyyy") + ".txt";

        #region Url
        /// <summary>
        /// Launch URL Scraper
        /// </summary>
        [TestMethod]
        public void ThreadsUrlScraper() {
            List<string> textSearch = new()
            {
                "Station essence de carburants alternatifs", "Station-service", "Aire d'autoroute", "Aire de pique-nique", "Aire de repos", "Aire de restauration"
            };

            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core\\Files", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core\\Files", "IleDeFrance.txt"));
            string[] cp = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core\\Files", "CpList.txt"));
            List<string> locations = new(dept);
            List<Task> tasks = new();

            foreach (string search in textSearch) {
                UrlRequest request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Replace(' ', '+'));
                Task newThread = Task.Run(delegate { UrlController.Scraper(request); });
                tasks.Add(newThread);
                Thread.Sleep(2000);
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
                    (DbBusinessProfile? business, DbBusinessScore? businessScore) = await BusinessService.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request);
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
        /// Get google info by given url.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetInfosByUrlAsync() {
            string url = "https://www.google.com/maps/place/MONOPRIX/@46.3401195,2.6014815,17z/data=!4m16!1m9!3m8!1s0x47f0a7e780134f53:0x3dbab3dce9a2e639!2sMONOPRIX!8m2!3d46.3401195!4d2.6014815!9m1!1b1!16s%2Fg%2F1ts3kk0r!3m5!1s0x47f0a7e780134f53:0x3dbab3dce9a2e639!8m2!3d46.3401195!4d2.6014815!16s%2Fg%2F1ts3kk0r";
            Operation opertationType = Operation.FILE;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddMonths(-1);
            List<BusinessAgent> business = new() {
                new BusinessAgent(null, url.ToLower(), "e38c646bf09ccde19bb7002ba4b5ba69")
            };
            BusinessAgentRequest request = new(opertationType, getReviews, business, reviewsDate);
            await BusinessController.Scraper(request, 1);
        }
        /// <summary>
        /// Starting Scraper.
        /// </summary>
        [TestMethod]
        public async Task ThreadsBusinessScraper() {
            List<BusinessAgent> businessList = new();
            List<Task> tasks = new();
            using DbLib db = new();
            int nbThreads = 1;
            int threadNumber = 0;

            int entries = 109;
            int processing = 1;
            Operation operationType = Operation.OTHER;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddYears(-1);

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
                    string[]? urlList = File.ReadAllLines(pathUrlKnownFile);

                    if (isUrlKnownFile) {
                        foreach (string url in urlList) {
                            BusinessAgent? business = db.GetBusinessByUrlEncoded(ToolBox.ComputeMd5Hash(url));

                            if (business == null) {
                                using StreamWriter sw = File.AppendText(pathLogFile);
                                sw.WriteLine(url + "\n");
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

            using StreamWriter sw2 = File.AppendText(pathLogFile);
            sw2.WriteLine("\n\nStarting selenium process !\n\n");

            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(async () =>
                {
                    BusinessAgentRequest request = new(operationType, getReviews, new List<BusinessAgent>(chunk), reviewsDate);
                    await BusinessController.Scraper(request, threadNumber).ConfigureAwait(false);
                });
                tasks.Add(newThread);
            }
            await Task.WhenAll(tasks);
            return;
        }
        #endregion
    }
}