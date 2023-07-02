using GMS.Business.Agent;
using GMS.Url.Agent;
using GMS.Url.Agent.Model;
using OpenQA.Selenium;
using System.Globalization;
using GMS.Sdk.Core.Models;
using GMS.Sdk.Core.DbModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMS.Sdk.Core;
using System.Text.RegularExpressions;

namespace GMS.Tests
{
    [TestClass]
    public class UnitTestGlobal {

        public static readonly string pathUrlKnownFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathUnknownBusinessFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\unknown_url.txt";
        public static readonly string pathLogFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Logs\Business-Agent\log-" + DateTime.Today.ToString("MM-dd-yyyy") + ".txt";

        #region All
        [TestMethod]
        public void SandBox() {
            string? name = "123 pare-brise";
            name = Regex.Replace(name, @"[^0-9a-zA-Zçàéè'(),\s-]+|\s{2,}", "");


            return;
        }

        [TestMethod]
        public void GetXPathfromPage() {
            string url = "https://www.google.com/maps/place/BRED-Banque+Populaire/@48.8280761,2.2411834,15z/data=!4m10!1m2!2m1!1sbanque!3m6!1s0x47e67af2357c45ab:0x1b7baec714122e5b!8m2!3d48.8255006!4d2.2479565!15sCgZiYW5xdWWSAQRiYW5r4AEA!16s%2Fg%2F1wf37y2x";
            using SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            driver.GetToPage(url);

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test))) {
                string value = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test).GetAttribute("src").Trim();
                Console.WriteLine(value);
            }

        }

        [TestMethod]
        public async Task ApiCallForAdress() {

            string adress = "zone d'activité Moulin Pleysse, D653, 46800 Montcuq";
            AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(adress);
            Assert.IsNotNull(addressResponse);

            var x = addressResponse.Features[0].Geometry.Coordinates[0];
            var y = addressResponse.Features[0].Geometry.Coordinates[0];
            var town = addressResponse.Features[0].Properties.City;
            var postalCode = addressResponse.Features[0].Properties.Postcode;
            var street = addressResponse.Features[0].Properties.Street;
            var houseNumber = addressResponse.Features[0].Properties.HouseNumber;

            Assert.IsNotNull(x);
            Assert.IsNotNull(y);
            Assert.IsTrue(town.ToLower() == "boulogne-billancourt");
            Assert.IsTrue(postalCode == "92100");
            Assert.IsTrue(street.ToLower() == "boulevard jean jaurès");
            Assert.IsTrue(houseNumber == "25");
        }
        #endregion

        #region ToolBox
        [TestMethod]
        public void HashToMd5() {
            string messageEncoded = ToolBox.ComputeMd5Hash("Eurorepar Garage Barbeyron" + "23 Le Bourg, 33330 Saint-Christophe-des-Bardes");
            return;
        }

        [TestMethod]
        public void HashFileToMd5() {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\test.txt";
            string endFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\test_out.txt";
            string[] etabs = File.ReadAllLines(filePath);
            using StreamWriter sw = File.AppendText(endFilePath);
            foreach (string etab in etabs) {
                string[] line = etab.Split("\t");
                sw.WriteLine(etab + "\t" + ToolBox.ComputeMd5Hash(line[1] + line[4]));
            }
            return;
        }

        [TestMethod]
        public void ComputeDateFromGoogleDate() {
            string googleDate = "il y a 2 jours";
            DateTime date = ToolBox.ComputeDateFromGoogleDate(googleDate);
            return;
        }

        [TestMethod]
        public void ChangeUrlStateByGivenUrlFile() {
            string[] urlList = File.ReadAllLines(pathUrlKnownFile);
            DbLib db = new();
            int count = 0;

            foreach (string url in urlList) {
                try {
                    string? guid = db.GetBusinessUrlGuidByUrlEncoded(ToolBox.ComputeMd5Hash(url));
                    if (guid != null) {
                        db.UpdateBusinessUrlState(guid, UrlState.NEW);
                    } else
                        count++;
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                }
            }
            return;
        }
        #endregion

        #region UrlFinder
        [TestMethod]
        public void TestUrlFinderService() {
            List<string> textSearch = new()
            {
                "123 pare brise Saint Martin d'hères", "123 pare brise Beauvais"
            };

            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core\\Files", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core\\Files", "IleDeFrance.txt"));
            List<string> locations = new(dept);
            List<Task> tasks = new();

            foreach (string search in textSearch) {
                UrlFinderRequest request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Replace(' ', '+'));
                Task newThread = Task.Run(delegate { UrlFinderService.Start(request); });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }

            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void InsertNewUrl() {
            DbLib db = new();
            string url = "https://www.google.fr/maps/place/Lidl/@45.6945776,4.8226045,17z/data=!3m1!4b1!4m5!3m4!1s0x47f4e9aedb97e42b:0xdfb4d943672c4bd8!8m2!3d45.6945748!4d4.8247889?hl=fr";
            DateTime date = DateTime.Now;
            DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, date, "manually", date, ToolBox.ComputeMd5Hash(url));
            db.CreateBusinessUrl(businessUrl);
        }
        #endregion

        #region BusinessAgent

        [TestMethod]
        public void GetInfosByUrl() {
            string url = "https://www.google.com/maps/place/MONOPRIX/@46.3401195,2.6014815,17z/data=!4m16!1m9!3m8!1s0x47f0a7e780134f53:0x3dbab3dce9a2e639!2sMONOPRIX!8m2!3d46.3401195!4d2.6014815!9m1!1b1!16s%2Fg%2F1ts3kk0r!3m5!1s0x47f0a7e780134f53:0x3dbab3dce9a2e639!8m2!3d46.3401195!4d2.6014815!16s%2Fg%2F1ts3kk0r";
            Operation opertationType = Operation.FILE;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddMonths(-1);
            List<DbBusinessAgent> business = new() {
                new DbBusinessAgent(null, url.ToLower(), "e38c646bf09ccde19bb7002ba4b5ba69")
            };
            BusinessAgentRequest request = new(opertationType, getReviews, business, reviewsDate);
            _ = BusinessService.StartAsync(request, 1);
        }

        /// <summary>
        /// Exporting Hotels info
        /// </summary>
        [TestMethod]
        public async Task ExportHotelAsync() {

            // CONFIG
            int nbEntries = 1;
            UrlState urlState = UrlState.NEW;

            using DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, nbEntries);

            using SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            using StreamWriter sw2 = File.AppendText(@"C:\Users\maxim\Desktop\hotel.txt");
            sw2.WriteLine("NAME$$CATEGORY$$ADRESS$$TEL$$OPTIONS");
            foreach (DbBusinessAgent elem in businessList) {
                try {
                    (DbBusinessProfile? business, DbBusinessScore? businessScore) = await BusinessService.GetBusinessProfileAndScoreFromGooglePageAsync(driver, elem.Url, null, null);
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
        /// STARTING APP
        /// </summary>
        [TestMethod]
        public async Task ThreadsCategory() {

            // CONFIG
            int nbThreads = 1;
            int nbEntries = 10;
            string? sector = null;
            int processing = 1;
            Operation opertationType = Operation.CATEGORY;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddMonths(-7);

            List<DbBusinessAgent> businessList = new();
            List<Task> tasks = new();

            using DbLib db = new();
            if (sector == null) businessList = db.GetBusinessListNetwork(nbEntries, processing);
            else businessList = db.GetBusinessListNetworkBySector(sector, nbEntries);

            int threadNumber = 0;
            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(async () =>
                {
                    BusinessAgentRequest request = new(opertationType, getReviews, new List<DbBusinessAgent>(chunk), reviewsDate);
                    await BusinessService.StartAsync(request, threadNumber).ConfigureAwait(false);
                });
                tasks.Add(newThread);
            }

            await Task.WhenAll(tasks);
            return;

        }

        [TestMethod]
        public async Task ThreadsFileAsync() {

            // CONFIG
            int nbThreads = 8;
            Operation opertationType = Operation.FILE;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddYears(-1);
            bool isUrlKnownFile = false;
            bool isUrlFile = true;
            string[] urlList = File.ReadAllLines(pathUrlKnownFile);

            List<Task> tasks = new();
            List<DbBusinessAgent> businessList = new();

            if (isUrlKnownFile) {
                using DbLib db = new();
                foreach (string url in urlList) {
                    DbBusinessAgent? business = db.GetBusinessByUrlEncoded(ToolBox.ComputeMd5Hash(url));

                    if (business == null) {
                        using StreamWriter sw = File.AppendText(pathLogFile);
                        sw.WriteLine(url + "\n");
                        continue;
                    }

                    businessList.Add(business);
                }
            } else {
                foreach (string url in urlList) {
                    if (!isUrlFile) businessList.Add(new DbBusinessAgent(null, "https://www.google.fr/maps/search/" + url.ToLower()));
                    else businessList.Add(new DbBusinessAgent(null, url));
                }
            }
            

            using StreamWriter sw2 = File.AppendText(pathLogFile);
            sw2.WriteLine("\n\nStarting selenium process !\n\n");

            int threadNumber = 0;
            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(async () =>
                {
                    BusinessAgentRequest request = new(opertationType, getReviews, new List<DbBusinessAgent>(chunk), reviewsDate);
                    await BusinessService.StartAsync(request, threadNumber).ConfigureAwait(false);
                });
                tasks.Add(newThread);
            }
            await Task.WhenAll(tasks);
            return;
        }

        [TestMethod]
        public async Task ThreadsUrlStateAsync() {

            // CONFIG
            int nbThreads = 8;
            int nbEntries = 111;
            UrlState urlState = UrlState.NEW;
            Operation opertationType = Operation.URL_STATE;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddMonths(-7);

            List<Task> tasks = new();
            using DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, nbEntries);

            int threadNumber = 0;
            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(async () =>
                {
                    BusinessAgentRequest request = new(opertationType, getReviews, new List<DbBusinessAgent>(chunk), reviewsDate);
                    await BusinessService.StartAsync(request, threadNumber).ConfigureAwait(false);
                });
                tasks.Add(newThread);
            }
            await Task.WhenAll(tasks);
            return;
        }
        #endregion
    }
}