using GMS.BusinessProfile.Agent.Model;
using GMS.Business.Agent;
using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Url.Agent;
using GMS.Url.Agent.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using GMS.Sdk.Core.XPath;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace GMS.Tests {
    [TestClass]
    public class UnitTestGlobal {

        public static readonly string pathUrlFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathLogFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Logs\Business-Agent\log-" + DateTime.Today.ToString("MM-dd-yyyy") + ".txt";

        #region All
        [TestMethod]
        public void SandBox() {
            DbLib dbLib = new();
            string work = Directory.GetCurrentDirectory();
            string test = Directory.GetParent(work).Parent.Parent.FullName;
            
            return;
        }
        #endregion

        #region ToolBox
        [TestMethod]
        public void HashToMd5() {
            string messageEncoded = ToolBox.ComputeMd5Hash("https://www.google.com/maps/search/EMILIE+ALBINET+conseilli%C3%A8re+immobili%C3%A8re+SAFTI+VARENNES,+VILLEBRUMIER,+SAINT+NAUPHARY,+NOHIC,+ORGUEIL");
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
        #endregion

        #region BusinessAgent

        /// <summary>
        /// Getting google infos of Mairie du 1er arrondissement
        /// </summary>
        [TestMethod]
        public void ExportHotel() {
            List<DbBusinessAgent> list = GetBusinessAgentListFromUrlState(UrlState.NEW, 23918);
            SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            using StreamWriter sw2 = File.AppendText(@"C:\Users\maxim\Desktop\hotel.txt");
            sw2.WriteLine("NAME$$CATEGORY$$ADRESS$$TEL$$OPTIONS");
            foreach (DbBusinessAgent elem in list) {
                try {
                    (DbBusinessProfile? business, DbBusinessScore? businessScore) = BusinessService.GetBusinessProfileAndScoreFromGooglePage(driver, elem.Url, "123");
                    ReadOnlyCollection<IWebElement?> optionsOn = ToolBox.FindElementsSafe(driver.WebDriver, XPathProfile.optionsOn);
                    List<string> optionsOnList = new();
                    foreach (IWebElement element in optionsOn) {
                        optionsOnList.Add(element.GetAttribute("aria-label").Replace("L'option ", "").Replace(" est disponible", ""));
                    }
                    sw2.WriteLine(business.Name + "$$" + business.Category + "$$" + business.Adress + "$$" + business.Tel + "$$" + string.Join(",", optionsOnList));
                } catch (Exception) { }
            }
        }

        public void StartAgent(List<DbBusinessAgent> urlList, Operation operation, int? threadNumber = null) {
            BusinessAgentRequest request = new(operation, true, null, urlList, DateTime.UtcNow.AddMonths(-1));
            BusinessService.Start(request, threadNumber);
        }

        public List<DbBusinessAgent> GetBusinessAgentListNetworkByActivity(string activity, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListNetworkByActivity(activity, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public List<DbBusinessAgent> GetBusinessAgentListNetworkByCategory(string category, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListNetworkByCategory(category, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public List<DbBusinessAgent> GetBusinessAgentListNetworkByBrand(string brand, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListNetworkByBrand(brand, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public List<DbBusinessAgent> GetBusinessAgentListNetworkBySectory(string sector, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListNetworkBySector(sector, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public List<DbBusinessAgent> GetBusinessAgentListNetwork(int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListNetwork(entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public List<DbBusinessAgent> GetBusinessAgentListFromUrlState(UrlState urlState, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListByUrlState(urlState, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public (List<DbBusinessAgent>, List<string>) GetBusinessAgentListFromUrlFile(string[] urlList) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = new();
            List<string> urlNotFound = new();

            foreach (string url in urlList) {
                string urlEncoded = ToolBox.ComputeMd5Hash(url);
                DbBusinessAgent? business = db.GetBusinessAgentByUrlEncoded(urlEncoded);
                if (business != null)
                    businessList.Add(business);
                else {
                    urlNotFound.Add(url);
                }
            }
            
            db.DisconnectFromDB();
            return (businessList, urlNotFound);
        }

        #region THREADS
        /// <summary>
        /// STARTING APP
        /// </summary>
        [TestMethod]
        public void ThreadsCategory() {
            int nbThreads = 8;
            int nbEntries = 100;
            List<Task> tasks = new();

            //string category = "AGENCE IMMOBILIERE";
            //List<DbBusinessAgent> list = GetBusinessAgentListNetworkFromCategory(category, nbEntries);

            List<DbBusinessAgent> list = GetBusinessAgentListNetwork(nbEntries);
            int threadNumber = 0;
            foreach (var chunk in list.Chunk(nbEntries / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.CATEGORY, threadNumber); });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void ThreadsUrlList() {
            int nbThreads = 1;
            string[] urlList = File.ReadAllLines(pathUrlFile);
            List<Task> tasks = new();

            (List<DbBusinessAgent> bussinessList, List<string> urlNotFound) = GetBusinessAgentListFromUrlFile(urlList);
            foreach (string url in urlNotFound) {
                using StreamWriter sw = File.AppendText(pathLogFile);
                sw.WriteLine(url + "\n");
            }

            using StreamWriter sw2 = File.AppendText(pathLogFile);
            sw2.WriteLine("\n\nStarting selenium process !\n\n");

            int threadNumber = 0;
            foreach (var chunk in bussinessList.Chunk(bussinessList.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.FILE, threadNumber); });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void ThreadsUrlState() {
            int nbThreads = 1;
            UrlState state = UrlState.NEW;
            int nbEntries = 111;
            List<Task> tasks = new();
            List<DbBusinessAgent> list = GetBusinessAgentListFromUrlState(UrlState.NEW, nbEntries);

            int threadNumber = 0;
            foreach (var chunk in list.Chunk(list.Count / nbThreads)) {
                threadNumber++;
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.URL_STATE, threadNumber); });
                tasks.Add(newThread);
                Thread.Sleep(2000);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }
        #endregion
        #endregion
    }
}