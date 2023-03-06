using GMS.BusinessProfile.Agent.Model;
using GMS.Business.Agent;
using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Url.Agent;
using GMS.Url.Agent.Model;
using GMS.Sdk.Core.XPath;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Globalization;

namespace GMS.Tests {
    [TestClass]
    public class UnitTestGlobal {

        public static readonly string pathUrlKnownFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathUnknownBusinessFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\unknown_url.txt";
        public static readonly string pathLogFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Logs\Business-Agent\log-" + DateTime.Today.ToString("MM-dd-yyyy") + ".txt";

        #region All
        [TestMethod]
        public void SandBox() {
            DbLib dbLib = new();
            string work = Directory.GetCurrentDirectory();
            string test = Directory.GetParent(work).Parent.Parent.FullName;
            
            return;
        }

        [TestMethod]
        public void GetXPathfromPage() {
            string url = "https://www.google.com/maps/place/BRED-Banque+Populaire/@48.8280761,2.2411834,15z/data=!4m10!1m2!2m1!1sbanque!3m6!1s0x47e67af2357c45ab:0x1b7baec714122e5b!8m2!3d48.8255006!4d2.2479565!15sCgZiYW5xdWWSAQRiYW5r4AEA!16s%2Fg%2F1wf37y2x";
            SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            driver.GetToPage(url);

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test))) {
                string value = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test).GetAttribute("src").Trim();
                Console.WriteLine(value);
            }

            driver.WebDriver.Quit();
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
        #endregion

        #region UrlFinder
        [TestMethod]
        public void TestUrlFinderService() {
            List<string> textSearch = new()
            {
                "hotel", "camping"
            };

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

            //string[] cp = File.ReadAllLines(path + @"\GMS.Sdk.Core\ToolBox\CpList.txt");
            //List<string> locations = new(cp);

            string[] dept = File.ReadAllLines(path + @"\GMS.Sdk.Core\ToolBox\DeptList.txt");
            List<string> locations = new(dept);

            /*
            List<string> locations = new()
            {
                "97600", "97200", "97300", "97500", "97100", "97600", "98600", "98700"
            };*/

            List<Task> tasks = new();

            foreach (string search in textSearch) {
                Task newThread = Task.Run(delegate { StartSearch(search, locations); });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }

            Task.WaitAll(tasks.ToArray());
            return;
        }

        public void StartSearch(string textSearch, List<string> locations) {
            foreach (string location in locations) {
                string searchString = textSearch.Replace(' ', '+') + " " + location.Replace(';', ' ');
                UrlAgentRequest request = new(searchString);
                UrlService.Start(request);
            }
        }

        [TestMethod]
        public void InsertNewUrl() {
            DbLib db = new();
            string url = "https://www.google.fr/maps/place/Lidl/@45.6945776,4.8226045,17z/data=!3m1!4b1!4m5!3m4!1s0x47f4e9aedb97e42b:0xdfb4d943672c4bd8!8m2!3d45.6945748!4d4.8247889?hl=fr";
            DateTime date = DateTime.Now;
            DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, date, UrlState.NEW, "manually", date, ToolBox.ComputeMd5Hash(url));
            db.CreateBusinessUrl(businessUrl);
        }
        #endregion

        #region BusinessAgent

        /// <summary>
        /// Exporting Hotels info
        /// </summary>
        [TestMethod]
        public void ExportHotel() {

            // CONFIG
            int nbEntries = 1;
            UrlState urlState = UrlState.NEW;

            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, nbEntries);
            db.DisconnectFromDB();

            SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            using StreamWriter sw2 = File.AppendText(@"C:\Users\maxim\Desktop\hotel.txt");
            sw2.WriteLine("NAME$$CATEGORY$$ADRESS$$TEL$$OPTIONS");
            foreach (DbBusinessAgent elem in businessList) {
                try {
                    (DbBusinessProfile? business, DbBusinessScore? businessScore) = BusinessService.GetBusinessProfileAndScoreFromGooglePage(driver, elem.Url, null, null, true);
                    ReadOnlyCollection<IWebElement?> optionsOn = ToolBox.FindElementsSafe(driver.WebDriver, XPathProfile.optionsOn);
                    List<string> optionsOnList = new();
                    foreach (IWebElement element in optionsOn) {
                        optionsOnList.Add(element.GetAttribute("aria-label").Replace("L'option ", "").Replace(" est disponible", ""));
                    }
                    sw2.WriteLine(business.Name + "$$" + business.Category + "$$" + business.Adress + "$$" + business.Tel + "$$" + string.Join(",", optionsOnList));
                } catch (Exception) { }
            }
        }

        /// <summary>
        /// STARTING APP
        /// </summary>
        [TestMethod]
        public void ThreadsCategory() {

            // CONFIG
            int nbThreads = 1;
            int nbEntries = 1;
            string? sector = null;
            int processing = 2;
            Operation opertationType = Operation.CATEGORY;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddYears(-1);

            List<DbBusinessAgent> businessList = new();
            List <Task> tasks = new();
            
            DbLib db = new();
            if (sector == null) businessList = db.GetBusinessAgentListNetwork(nbEntries, processing);
            else businessList = db.GetBusinessAgentListNetworkBySector(sector, nbEntries);
            db.DisconnectFromDB();

            

            int threadNumber = 0;
            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(delegate {
                    BusinessAgentRequest request = new(opertationType, getReviews, new List<DbBusinessAgent>(chunk), reviewsDate);
                    BusinessService.Start(request, threadNumber);
                });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void ThreadsFile() {

            // CONFIG
            int nbThreads = 1;
            Operation opertationType = Operation.FILE;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddYears(-1);
            bool isUrlKnownFile = false;
            bool isUrlFile = true;
            string[] urlList = File.ReadAllLines(pathUrlKnownFile);

            List<Task> tasks = new();
            List<DbBusinessAgent> businessList = new();

            if (isUrlKnownFile) {
                DbLib db = new();
                foreach (string url in urlList) {
                    DbBusinessAgent? business = db.GetBusinessAgentByUrlEncoded(ToolBox.ComputeMd5Hash(url));

                    if (business == null) {
                        using StreamWriter sw = File.AppendText(pathLogFile);
                        sw.WriteLine(url + "\n");
                        continue;
                    }

                    businessList.Add(business);
                }
                db.DisconnectFromDB();
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
                Task newThread = Task.Run(delegate {
                    BusinessAgentRequest request = new(opertationType, getReviews, new List<DbBusinessAgent>(chunk), reviewsDate);
                    BusinessService.Start(request, threadNumber);
                });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void ThreadsUrlState() {

            // CONFIG
            int nbThreads = 8;
            int nbEntries = 111;
            UrlState urlState = UrlState.NEW;
            Operation opertationType = Operation.URL_STATE;
            bool getReviews = true;
            DateTime reviewsDate = DateTime.UtcNow.AddMonths(-1);

            List<Task> tasks = new();
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, nbEntries);
            db.DisconnectFromDB();

            int threadNumber = 0;
            foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(delegate {
                    BusinessAgentRequest request = new(opertationType, getReviews, new List<DbBusinessAgent>(chunk), reviewsDate);
                    BusinessService.Start(request, threadNumber);
                });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }
        #endregion
    }
}