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
using Sdk.Core.Types.Api;
using System.Drawing;

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
                SeleniumDriver driver = new();
                var testResult = await ScannerFunctions.ScannerTest(driver);
                driver.Dispose();

                if (!testResult.Success)
                    return GenericResponse.Exception($"XPATH was modified, can't scan anything !");

                switch (request.OperationType)
                {
                    case Operation.PROCESSING_STATE:
                        GetBusinessListRequest businessListRequest = new(request.Entries, request.Processing, request.Brand, request.Category, request.CategoryFamily, request.IsNetwork, request.IsIndependant);
                        businessList = db.GetBusinessAgentList(businessListRequest);
                        break;
                    case Operation.URL_STATE:
                        businessList = db.GetBusinessAgentListByUrlState(request.UrlState, request.Entries, request.Processing);
                        break;
                }

                int nbThreads = 8;

                if (businessList.Count < 10)
                    nbThreads = 1;
                

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
        public async Task<ActionResult<GenericResponse>> StartUrlScanner()
        {
            try
            {
                //string basePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "ReferentialFiles");

                string[] categories = System.IO.File.ReadAllLines("ReferentialFiles\\Categories.txt");
                /*string[] dept = System.IO.File.ReadAllLines(Path.Combine(basePath, "DeptList.txt"));
                string[] idf = System.IO.File.ReadAllLines(Path.Combine(basePath, "IleDeFrance.txt"));
                string[] cp = System.IO.File.ReadAllLines(Path.Combine(basePath, "CpList.txt"));*/
                string[] towns = System.IO.File.ReadAllLines("ReferentialFiles\\TownList.txt");

                List<string> locations = new(towns);
                List<Task> tasks = [];

                int maxConcurrentThreads = 6;
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
                    Thread.Sleep(15000);
                }

                await Task.WhenAll(tasks);
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
                SeleniumDriver driver = new();
                var testResult = await ScannerFunctions.ScannerTest(driver);
                driver.Dispose();
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                string message = testResult.Message + " Process took " + elapsedTime.ToString() + " to execute.";
                ToolBox.SendEmail(message, "Scanner daily test result");

                return new GenericResponse(1, "Scanner test finished.");
            }
            catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while starting scanner test.");
                return GenericResponse.Exception($"An exception occurred while starting scanner test : {e.Message}");
            }
        }

        /// <summary>
        /// Start Scanner Stickers.
        /// </summary>
        [HttpPost("scanner/sticker")]
        [Authorize(Policy = "DevelopmentPolicy")]
        public async Task<ActionResult<GetStickerListResponse>> StartStickerScannerAsync([FromBody] StickerScannerRequest request)
        {
            try
            {
                GetStickerListResponse response = new([]);

                DbLib dbLib = new();

                SeleniumDriver driver = new();

                foreach (StickerFileRowData record in request.File)
                {
                    try
                    {
                        DbSticker? sticker = dbLib.GetStickerByPlaceId(record.Data, request.Year);

                        // Sticker found
                        if (sticker != null)
                        {
                            response.StickerList?.Add(new(record.Id, sticker));
                            continue;
                        }

                        DbBusinessProfile? bp = dbLib.GetBusinessByPlaceId(record.Data);

                        // BP not in our DB
                        if (bp == null)
                        {
                            Place? place = await PlaceService.Api.Service.PlaceService.GetPlaceByPlaceId(record.Data, request.Lang);

                            // Place not found on Google API
                            if (place == null)
                            {
                                response.StickerList?.Add(new(record.Id, null));
                                continue;
                            }

                            bp = ToolBox.PlaceToBP(place);

                            // No URL, so we cannot get info
                            if (bp?.PlaceUrl == null)
                            {
                                response.StickerList?.Add(new(record.Id, null));
                                continue;
                            }

                            dbLib.CreateBusinessProfile(bp);
                        }

                        (DbBusinessProfile? scannedBp, DbBusinessScore? bs) = await ScannerFunctions.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(bp.PlaceUrl, bp.FirstGuid, bp.IdEtab), bp);

                        if (scannedBp != null)
                            dbLib.UpdateBusinessProfile(scannedBp);

                        if (bs == null || bs.Score == null || bs.NbReviews == null)
                        {
                            response.StickerList?.Add(new(record.Id, null));
                            continue;
                        } else
                            dbLib.CreateBusinessScore(bs!);

                        BusinessAgent ba = new(bp.PlaceUrl, bp.PlaceId);
                        ScannerStickerParameters parameters = new(ba, request.Year);

                        sticker = Scanner.Agent.Scanner.StickerScanner(parameters, driver);

                        if (sticker == null || sticker.Score == 0)
                        {
                            response.StickerList?.Add(new(record.Id, null));
                            continue;
                        }

                        // TODO : Create the sticker
                        //Bitmap drawnSitcker = ToolBox.CreateSticker()

                        dbLib.CreateSticker(sticker);
                        response.StickerList?.Add(new(record.Id, sticker));
                    } catch (Exception e)
                    {
                        Log.Error(e, $"An exception occurred while getting sticker for place id =[{record.Data}].");
                        dbLib.CreateError(record.Data, request.Year, $"STACKTRACE: [{e.Message}] - MESSAGE [{e.Message}]");
                        driver.Dispose();
                        driver = new();
                        continue;
                    }
                }
                try
                {
                    driver.Dispose();
                } catch (Exception)
                {
                }
                return response;
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while starting scanner sticker.");
                return GetStickerListResponse.Exception($"An exception occurred while starting scanner sticker : {e.Message}");
            }
        }
    }
}
