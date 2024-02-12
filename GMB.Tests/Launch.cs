using GMB.BusinessService.Api.Controller;
using GMB.Scanner.Agent.Core;
using GMB.Scanner.Agent.Models;
using GMB.ScannerService.Api.Controller;
using GMB.ScannerService.Api.Services;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
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
        /// Weekly testing for XPATH.
        /// </summary>
        [TestMethod]
        public async Task WeeklyTestAsync()
        {
            ScannerFunctions scanner = new();
            SeleniumDriver driver = new();

            _ = await scanner.WeeklyTestAsync();

            driver.Dispose();
        }
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
                db.UpdateBusinessProfileProcessingState(idEtab, 9);
            }
        }
        /// <summary>
        /// Getting ID etab and place ID from url list.
        /// </summary>
        [TestMethod]
        public async Task GetBPFromUrlListAsync()
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

            BusinessController controller = new();

            ActionResult<GetBusinessListResponse> response = await controller.GetBusinessListByUrlAsync(values);

            string outputFilePath = "file.csv";
            using StreamWriter writer = new(outputFilePath);

            foreach(Business? business in response.Value.BusinessList)
            {
                if (business != null)
                    writer.Write(business.IdEtab + ";" + business.PlaceId);
            }
        }
        /// <summary>
        /// Starting Scanner.
        /// </summary>
        [TestMethod]
        public void LaunchBusinessScanner()
        {
            AuthorizationPolicyService policy = new();
            ScannerController scannerController = new(policy);
            BusinessScannerRequest request = new(100000, 13, Operation.PROCESSING_STATE, true, DateTime.UtcNow.AddMonths(-12));

            Task.Run(() => scannerController.StartBusinessScannerAsync(request)).Wait();
            return;
        }
        #endregion
    }
}