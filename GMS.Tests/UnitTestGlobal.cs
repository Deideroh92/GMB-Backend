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

        public static readonly string pathUrlFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\url.txt";
        public static readonly string pathLogFile = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Logs\Business-Agent\log" + DateTime.Today.ToString() + ".txt";

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

        /// <summary>
        /// Getting google infos of Mairie du 1er arrondissement
        /// </summary>
        [TestMethod]
        public void TestBusinessProfile() {
            DbBusinessProfile MairieDu1er = new("123", "123", "Mairie du 1ᵉʳ arrondissement", "Hôtel de ville", "4 Pl. du Louvre, 75001 Paris", "01 44 50 75 01", "https://mairiepariscentre.paris.fr/", null ,null, null, null);

            SeleniumDriver driver = new();
            
            (DbBusinessProfile? business, DbBusinessScore? businessScore) = BusinessService.GetBusinessProfileAndScoreFromGooglePage(driver, "https://www.google.com/maps/place/Mairie+du+1%E1%B5%89%CA%B3+arrondissement/@48.8566099,2.3195451,14z/data=!4m9!1m2!2m1!1smairie+de+paris!3m5!1s0x47e66e216da9fe39:0xd083ed96cb779914!8m2!3d48.860046!4d2.341252!15sCg9tYWlyaWUgZGUgcGFyaXOSAQljaXR5X2hhbGzgAQA", "123");

            Assert.IsTrue(MairieDu1er.Equals(business));
        }

        public void StartAgent(List<DbBusinessAgent> urlList, Operation operation) {
            BusinessAgentRequest request = new(operation, true, null, urlList, DateTime.UtcNow.AddYears(-1));
            BusinessService.Start(request);
        }

        public List<DbBusinessAgent> GetBusinessAgentListFromCategory(string category, int entries) {
            DbLib db = new();
            List<DbBusinessAgent> businessList = db.GetBusinessAgentListByCategory(category, entries);
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
            int nbEntries = 38021;
            string category = "AGENCE IMMOBILIERE";
            List<Task> tasks = new();
            List<DbBusinessAgent> list = GetBusinessAgentListFromCategory(category, nbEntries);
            foreach (var chunk in list.Chunk(nbEntries / nbThreads)) {
                Task newThread = Task.Run(delegate { StartAgent(new List<DbBusinessAgent>(chunk), Operation.CATEGORY); });
                tasks.Add(newThread);
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void ThreadsUrlList() {
            int nbThreads = 8;
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
            }
            Task.WaitAll(tasks.ToArray());
            return;
        }

        [TestMethod]
        public void ThreadsUrlState() {
            int nbThreads = 8;
            UrlState state = UrlState.NEW;
            int nbEntries = 100;
            List<Task> tasks = new();
            List<DbBusinessAgent> list = GetBusinessAgentListFromUrlState(UrlState.NEW, nbEntries);
            foreach (var chunk in list.Chunk(list.Count / nbThreads)) {
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