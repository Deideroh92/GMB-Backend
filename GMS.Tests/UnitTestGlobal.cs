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
                "hotel", "camping", "résidence", "station de ski", "hebergement","\r\n"
            };

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            string[] urlList = File.ReadAllLines(path + @"\GMS.Sdk.Core\ToolBox\CpList.txt");
            List<string> locations = new(urlList);

            /*
            List<string> locations = new()
            {
                "97600", "97200", "97300", "97500", "97100", "97600", "98600", "98700"
            };*/

            foreach (string search in textSearch) {
                foreach (string location in locations) {
                    string searchString = search.Replace(' ', '+') + '+' + location.Replace(' ', '+');
                    UrlAgentRequest request = new(searchString);
                    UrlService.Start(request);
                }
            }
        }
        #endregion

        #region BusinessAgent

        /// <summary>
        /// Getting google infos of Mairie du 1er arrondissement
        /// </summary>
        [TestMethod]
        public void TestBusinessProfile() {
            DbBusinessProfile MairieDu1er = new("123", "123", "Mairie du 1ᵉʳ arrondissement", "Hôtel de ville", "4 Pl. du Louvre, 75001 Paris", "01 44 50 75 01", "https://mairiepariscentre.paris.fr/", null ,null, null, null);

            SeleniumDriver driver = new();
            
            (DbBusinessProfile? business, DbBusinessScore? businessScore) = BusinessService.GetBusinessProfileAndScoreFromGooglePage(driver, "https://www.google.com/maps/place/H%C3%B4tel+Novotel+Paris+Pont-de-S%C3%A8vres/@48.830891,2.2163365,15z/data=!4m13!1m2!2m1!1shotel!3m9!1s0x47e67b04e1991d45:0xc614fbd8fc4f2280!5m2!4m1!1i2!8m2!3d48.826851!4d2.2212687!15sCgVob3RlbJIBBWhvdGVs4AEA!16s%2Fg%2F1v0lk2hl", "123");

            Assert.IsTrue(MairieDu1er.Equals(business));
        }

        public void StartAgent(List<DbBusinessAgent> urlList, Operation operation) {
            BusinessAgentRequest request = new(operation, true, null, urlList, DateTime.UtcNow.AddYears(-1));
            BusinessService.Start(request);
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
            int nbEntries = 137303;
            List<Task> tasks = new();

            //string category = "AGENCE IMMOBILIERE";
            //List<DbBusinessAgent> list = GetBusinessAgentListNetworkFromCategory(category, nbEntries);

            List<DbBusinessAgent> list = GetBusinessAgentListNetwork(nbEntries);
            foreach (var chunk in list.Chunk(nbEntries / nbThreads)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.CATEGORY); });
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

            foreach (var chunk in bussinessList.Chunk(bussinessList.Count / nbThreads)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.FILE); });
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
            foreach (var chunk in list.Chunk(list.Count / nbThreads)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.URL_STATE); });
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