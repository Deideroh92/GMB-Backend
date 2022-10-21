using GMS.BusinessProfile.Agent.Model;
using GMS.Business.Agent;
using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Url.Agent;
using GMS.Url.Agent.Model;

namespace GMS.Tests {
    [TestClass]
    public class UnitTestGlobal {

        public static readonly string pathUrlFile = @"";
        public static readonly string pathLogFile = @"";
        public static readonly string category = "AGENCE IMMOBILIERE";

        #region All
        [TestMethod]
        public void SandBox() {
            DbLib dbLib = new();
            List<string> sector = dbLib.GetSector();
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

        [TestMethod]
        public int CountBusinessesByCategory(string category = "AGENCE IMMOBILIERE") {
            DbLib dbLib = new();
            int count = dbLib.CountBusinessProfileByCategory(category);
            return count;
        }
        #endregion

        #region UrlFinder
        [TestMethod]
        public void TestUrlFinderService() {
            List<string> textSearch = new()
            {
                "bred", "credit agricole", "lcl"
            };

            List<string> locations = new()
            {
                "Paris", "53000"
            };
            foreach (string search in textSearch) {
                foreach (string location in locations) {
                    string searchString = search.Replace(' ', '+') + location.Replace(' ', '+');
                    UrlAgentRequest request = new(searchString, DriverType.CHROME);
                    UrlService.Start(request);
                }
            }
        }
        #endregion

        #region BusinessAgent
        [TestMethod]
        public void StartAgent(List<DbBusinessAgent> urlList, Operation operation) {
            BusinessAgentRequest request = new(operation, true, null, urlList, DateTime.UtcNow.AddYears(-1));
            BusinessService.Start(request, pathLogFile);
        }

        public List<DbBusinessAgent> GetBusinessListFromCategory(string category, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessList(category, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public List<DbBusinessAgent> GetBusinessListFromUrlState(UrlState urlState, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessList(urlState, entries);
            db.DisconnectFromDB();
            return businessList;
        }

        public (List<DbBusinessAgent>, List<string>) GetBusinessListFromUrlFile(string[] urlList) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = new();
            List<string> urlNotFound = new();

            foreach (string url in urlList) {
                string urlEncoded = ToolBox.ComputeMd5Hash(url);
                DbBusinessAgent? business = db.GetBusinessByUrlEncoded(urlEncoded);
                if (business != null)
                    businessList.Add(db.GetBusinessByUrlEncoded(urlEncoded));
                else {
                    urlNotFound.Add(url);
                }
            }
            
            db.DisconnectFromDB();
            return (businessList, urlNotFound);
        }

        [TestMethod]
        public void OneThreadCategory() {

            List<DbBusinessAgent> list = GetBusinessListFromCategory(category, 100);
            StartAgent(new List<DbBusinessAgent>(list), Operation.CATEGORY);
            return;
        }

        [TestMethod]
        public void OneThreadUrlList() {

            string[] urlList = File.ReadAllLines(pathUrlFile);
            (List<DbBusinessAgent> bussinessList, List<string> urlNotFound) = GetBusinessListFromUrlFile(urlList);
            StartAgent(new List<DbBusinessAgent>(bussinessList), Operation.FILE);
            return;
        }

        [TestMethod]
        public void OneThreadUrlState() {

            List<DbBusinessAgent> list = GetBusinessListFromUrlState(UrlState.NEW, 100);
            StartAgent(new List<DbBusinessAgent>(list), Operation.URL_STATE);
            return;
        }

        #region THREADS
        /// <summary>
        /// STARTING APP
        /// </summary>
        [TestMethod]
        public void FourThreadsCategory() {
            List<Task> tasks = new();
            int nb = 38021;
            List<DbBusinessAgent> list = GetBusinessListFromCategory(category, nb);
            foreach (var chunk in list.Chunk(nb/4)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.CATEGORY); });
                tasks.Add(newThread);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void FourThreadsUrlList() {
            string[] urlList = File.ReadAllLines(pathUrlFile);
            List<Task> tasks = new();

            (List<DbBusinessAgent> bussinessList, List<string> urlNotFound) = GetBusinessListFromUrlFile(urlList);
            foreach (string url in urlNotFound) {
                using StreamWriter sw = File.AppendText(pathLogFile);
                sw.WriteLine(url + "\n");
            }

            using StreamWriter sw2 = File.AppendText(pathLogFile);
            sw2.WriteLine("\n\nStarting selenium process !\n\n");

            foreach (var chunk in bussinessList.Chunk(bussinessList.Count / 4)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.FILE); });
                tasks.Add(newThread);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void FourThreadsUrlState() {
            List<Task> tasks = new();
            List<DbBusinessAgent> list = GetBusinessListFromUrlState(UrlState.NEW, 100);
            foreach (var chunk in list.Chunk(list.Count / 4)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.URL_STATE); });
                tasks.Add(newThread);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }
        #endregion
        #endregion
    }
}