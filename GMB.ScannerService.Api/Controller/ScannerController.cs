using GMB.Scanner.Agent.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.ScannerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using GMB.Sdk.Core;
using System.Diagnostics;
using GMB.Sdk.Core.Types.BusinessService;
using GMB.Sdk.Core.Types.Models;
using GMB.PlaceService.Api.Core;

namespace GMB.ScannerService.Api.Controller
{
    [ApiController]
    [Route("api/scanner-service")]
    public class ScannerController() : ControllerBase
    {
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

                var testResult = await ScannerFunctions.ScannerTest();

                if (!testResult.Success)
                    return GenericResponse.Exception($"XPATH was modified, can't scan anything !");

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

                int nbThreads = 1;

                foreach (var chunk in businessList.Chunk(businessList.Count / nbThreads))
                {
                    Task newThread = Task.Run(async () =>
                    {
                        ScannerBusinessParameters scannerRequest = new(request.OperationType, request.GetReviews, new List<BusinessAgent>(chunk), request.ReviewsDate, request.UpdateProcessingState);
                        await Scanner.Agent.Scanner.BusinessScanner(scannerRequest).ConfigureAwait(false);
                    });
                    tasks.Add(newThread);
                    Thread.Sleep(15000);
                }
                await Task.WhenAll(tasks);
                return new GenericResponse(1, "Scanner launched successfully.");
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while launching scanner.");
                return GenericResponse.Exception($"An exception occurred while launching scanner. : {e.Message}");
            }
        }

        /// <summary>
        /// Start Url Scanner.
        /// </summary>
        [HttpPost("scanner/url")]
        [Authorize(Policy = "DevelopmentPolicy")]
        public ActionResult<GenericResponse> StartUrlScanner()
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
                return new GenericResponse(1, "Scanner launched successfully.");
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while starting url scanner.");
                return GenericResponse.Exception($"An exception occurred while starting url scanner : {e.Message}");
            }
        }

        /// <summary>
        /// Start Scanner Test.
        /// </summary>
        [HttpPost("scanner/test")]
        [Authorize(Policy = "DevelopmentPolicy")]
        public async Task<ActionResult<GenericResponse>> StartTestAsync()
        {
            try
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                var testResult = await ScannerFunctions.ScannerTest();
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                string message = testResult.Message + " Process took " + elapsedTime.ToString() + " to execute.";
                ToolBox.SendEmail(message);

                return new GenericResponse(1, "Scanner test finished.");
            }
            catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while starting scanner test.");
                return GenericResponse.Exception($"An exception occurred while starting scanner test : {e.Message}");
            }
        }

        /// <summary>
        /// Start Scanner Test.
        /// </summary>
        [HttpPost("scanner/sticker")]
        [Authorize(Policy = "DevelopmentPolicy")]
        public async Task<ActionResult<GetStickerListResponse>> StartStickerScanner([FromBody] StickerScannerRequest request)
        {
            try
            {
                var testResult = await ScannerFunctions.ScannerTest();

                if (!testResult.Success)
                    return GetStickerListResponse.Exception($"XPATH was modified, can't scan anything !");

                List<Sticker> stickers = [];

                if (request.Type == StickerType.PLACE_ID)
                {
                    DbLib dbLib = new();

                    foreach(StickerFileRowData record in request.File)
                    {
                        DbBusinessProfile? bp = dbLib.GetBusinessByPlaceId(record.Data);

                        if (bp == null)
                        {
                            Place? place = await PlaceService.Api.Core.PlaceService.GetPlaceByPlaceId(record.Data);
                            if (place == null)
                            {
                                stickers.Add(new(record.Id, null, null, null));
                                continue;
                            }  
                            bp = ToolBox.PlaceToBP(place);
                        }
                    }
                }

                return new GetStickerListResponse();
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while starting scanner test.");
                return GetStickerListResponse.Exception($"An exception occurred while starting scanner test : {e.Message}");
            }
        }
    }
}
