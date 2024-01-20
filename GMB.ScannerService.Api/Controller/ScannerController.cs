using GMB.Scanner.Agent.Models;
using GMB.ScannerService.Api.Services;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace GMB.ScannerService.Api.Controller
{
    [ApiController]
    [Route("api/scanner-service")]
    public class ScannerController(AuthorizationPolicyService policyService) : ControllerBase
    {
        private readonly AuthorizationPolicyService _policyService = policyService;

        /// <summary>
        /// Start Business Scanner
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("scanner/business")]
        [Authorize(Policy = "DevelopmentPolicy")]
        public async Task<ActionResult<GenericResponse>> StartBusinessScannerAsync([FromBody] BusinessScannerRequest request)
        {
            try
            {
                List<BusinessAgent> businessList = [];
                List<Task> tasks = [];
                using DbLib db = new();
                int threadNumber = 0;

                switch (request.OperationType)
                {
                    case Operation.PROCESSING_STATE:
                        GetBusinessListRequest businessListRequest = new(request.Entries, request.Processing, request.Brand, request.Category, request.CategoryFamily, request.IsNetwork, request.IsIndependant);
                        businessList = db.GetBusinessAgentList(businessListRequest);
                        break;
                    case Operation.URL_STATE:
                        businessList = db.GetBusinessAgentListByUrlState(request.UrlState, request.Entries);
                        break;
                }

                int nbThreads = 8;

                foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads))
                {
                    threadNumber++;
                    Task newThread = Task.Run(async () =>
                    {
                        ScannerBusinessRequest scannerRequest = new(request.OperationType, request.GetReviews, new List<BusinessAgent>(chunk), request.ReviewsDate, request.Processing != 9);
                        await Scanner.Agent.Scanner.BusinessScanner(scannerRequest).ConfigureAwait(false);
                    });
                    tasks.Add(newThread);
                }
                await Task.WhenAll(tasks);
                return new GenericResponse(1, "Scanner launched sucessfully.");
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while launching scanner.");
                return GenericResponse.Exception($"An exception occurred while launching scanner. : {e.Message}");
            }
        }

        /// <summary>
        /// Create Url.
        /// </summary>
        /// <param name="url"></param>
        [HttpPost("scanner/url")]
        [Authorize]
        public ActionResult<GenericResponse> StartUrlScanner([FromBody] string url, UrlState urlState = UrlState.NEW)
        {
            try
            {
                string basePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles");

                string[] categories = System.IO.File.ReadAllLines(Path.Combine(basePath, "Categories.txt"));
                string[] textSearch = System.IO.File.ReadAllLines(Path.Combine(basePath, "UrlTextSearch.txt"));
                string[] dept = System.IO.File.ReadAllLines(Path.Combine(basePath, "DeptList.txt"));
                string[] idf = System.IO.File.ReadAllLines(Path.Combine(basePath, "IleDeFrance.txt"));
                string[] cp = System.IO.File.ReadAllLines(Path.Combine(basePath, "CpList.txt"));
                string[] customLocations = System.IO.File.ReadAllLines(Path.Combine(basePath, "CustomLocations.txt"));

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
                return new GenericResponse(1, "Scanner launched sucessfully.");
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while starting url scanner.");
                return GenericResponse.Exception($"An exception occurred while starting url scanner : {e.Message}");
            }
        }
    }
}
