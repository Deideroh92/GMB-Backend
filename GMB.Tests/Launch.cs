using GMB.BusinessService.Api.Controller;
using GMB.ScannerService.Api.Controller;
using GMB.ScannerService.Api.Services;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.ScannerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GMB.Tests
{
    [TestClass]
    public class Launch
    {
        #region Url
        /// <summary>
        /// Launch URL Scanner
        /// </summary>
        [TestMethod]
        public void ThreadsUrlScraper()
        {
            string[] categories = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "Categories.txt"));
            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "IleDeFrance.txt"));
            string[] cp = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "CpList.txt"));

            List<string> locations = new(cp);
            List<Task> tasks = [];

            int maxConcurrentThreads = 1;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            foreach (string search in categories)
            {
                ScannerUrlParameters request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Trim().Replace(' ', '+'));
                Task newThread = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Wait until there's an available slot to run
                    try
                    {
                        Scanner.Agent.Scanner.UrlScanner(request);
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
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Sdk.Core\Files\Custom.txt";
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
                db.UpdateBusinessProfileProcessingState(idEtab, 10);
            }
        }
        /// <summary>
        /// Getting ID etab and place ID from url list.
        /// </summary>
        [TestMethod]
        public async Task GetBPFromUrlListAsync()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Sdk.Core\Files\Custom2.txt";
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

            BusinessController controller = new();

            List<Task> tasks = [];

            ActionResult<GetBusinessListResponse> response = await controller.GetBusinessListByUrlAsync(values);

            string outputFilePath = "file.csv";
            using StreamWriter writer = new(outputFilePath, true);
            foreach (Business? business in response.Value.BusinessList)
            {
                if (business != null)
                    await writer.WriteAsync(business.Id + ";" + business.IdEtab + ";" + business.PlaceId + Environment.NewLine);
                else
                    await writer.WriteAsync(business.Id + ";" + business.IdEtab + ";" + business.PlaceId + "not found" + Environment.NewLine);
            }
        }

        /// <summary>
        /// Starting Scanner Test.
        /// </summary>
        [TestMethod]
        public async Task LaunchScannerTest()
        {
            ScannerController scannerController = new();
            await scannerController.StartTestAsync();
            return;
        }

        /// <summary>
        /// Starting Business Scanner.
        /// </summary>
        [TestMethod]
        public void LaunchBusinessScanner()
        {
            AuthorizationPolicyService policy = new();
            ScannerController scannerController = new();
            BusinessScannerRequest request = new(1000, 1, Operation.PROCESSING_STATE, true, DateTime.UtcNow.AddMonths(-2), false, false, null, null, null, UrlState.NEW, true);

            Task.Run(() => scannerController.StartBusinessScannerAsync(request)).Wait();
            return;
        }

        /// <summary>
        /// Starting Url Scanner.
        /// </summary>
        [TestMethod]
        public void LaunchUrlScanner()
        {
            AuthorizationPolicyService policy = new();
            ScannerController scannerController = new();

            Task.Run(() => scannerController.StartUrlScanner()).Wait();
            return;
        }
        #endregion
    }
}