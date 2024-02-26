using GMB.BusinessService.Api.Models;
using GMB.Scanner.Agent.Core;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace GMB.BusinessService.Api.Controller
{
    /// <summary>
    /// Business Service Controller.
    /// </summary>
    [ApiController]
    [Route("api/business-service")]
    public class BusinessController : ControllerBase
    {

        #region Profile & Score

        #region Create
        /// <summary>
        /// Create new Business.
        /// </summary>
        /// <param name="business"></param>
        [HttpPost("bp/create")]
        [Authorize]
        public ActionResult<GenericResponse> CreateNewBusiness([FromBody] Business business)
        {
            try
            {
                using DbLib db = new();

                if (business.PlaceId == null)
                    return GenericResponse.Exception("No Place ID, can't create the business profile.");

                if (business.PlaceUrl == null)
                    return GenericResponse.Exception("No URL specified, can't create the business profile.");

                if (db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(business.PlaceUrl)))
                    return GenericResponse.Exception("URL already exists in DB.");

                if (db.CheckBusinessProfileExistByIdEtab(business.IdEtab))
                    return GenericResponse.Exception("Business Profile (ID ETAB) already exists in DD.");

                if (db.CheckBusinessProfileExistByPlaceId(business.PlaceId))
                    return GenericResponse.Exception("Business Profile (PLACE ID) already exists in DB.");

                string guid = Guid.NewGuid().ToString("N");
                db.CreateBusinessUrl(new DbBusinessUrl(guid, business.PlaceUrl, "platform"));

                business.FirstGuid = guid;

                DbBusinessProfile bp = new(business);

                db.CreateBusinessProfile(bp);

                if (business.Score != null && business.NbReviews != null)
                    db.CreateBusinessScore(new(bp.IdEtab, business.Score, business.NbReviews));

                return new GenericResponse(bp.Id, "Business Profile successfully created in DB.");
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"Error creating business : [{e.Message}]");
            }
        }
        /// <summary>
        /// Create new Business Profile list.
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("bp/create-list")]
        [Authorize]
        public ActionResult<CreateBusinessListResponse> CreateNewBusinessList([FromBody] CreateBusinessListRequest request)
        {
            try
            {
                using DbLib db = new();

                List<string> errorList = [];
                List<string?> idEtabList = [];

                foreach (Business business in request.BusinessList)
                {
                    if (business.PlaceUrl == null)
                    {
                        errorList.Add($"ID_ETAB: [{business.IdEtab}] - PLACE_ID : [{business.PlaceId}]" + " does not have place URL");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (db.CheckBusinessProfileExistByIdEtab(business.IdEtab))
                    {
                        errorList.Add($"ID_ETAB: [{business.IdEtab}] - PLACE_ID : [{business.PlaceId}]" + $" ID_ETAB exist in our DB");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (business.PlaceId != null && db.CheckBusinessProfileExistByPlaceId(business.PlaceId))
                    {
                        errorList.Add($"ID_ETAB: [{business.IdEtab}] - PLACE_ID : [{business.PlaceId}]" + $" PLACE_ID exist in our DB");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (business.PlaceUrl != null && db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(business.PlaceUrl)))
                    {
                        errorList.Add($"ID_ETAB: [{business.IdEtab}] - PLACE_ID : [{business.PlaceId}]" + $" does have an existing URL [{business.PlaceUrl}] in DB");
                        idEtabList.Add(null);
                        continue;
                    }

                    string guid = Guid.NewGuid().ToString("N");
                    business.FirstGuid = guid;

                    DbBusinessProfile bp = new(business);

                    db.CreateBusinessUrl(new DbBusinessUrl(guid, bp.PlaceUrl ?? "manually", "platform"));
                    db.CreateBusinessProfile(bp);
                    db.CreateBusinessScore(new(business.IdEtab, business.Score, business.NbReviews));
                    idEtabList.Add(bp.IdEtab);
                }

                errorList.Add(request.BusinessList.Count - errorList.Count + "/" + request.BusinessList.Count + " businesses treated.");

                return new CreateBusinessListResponse(errorList, idEtabList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return CreateBusinessListResponse.Exception($"Error creating business list: [{e.Message}]");
            }
        }
        /// <summary>
        /// Create Business Score.
        /// </summary>
        /// <param name="businessScore"></param>
        [HttpPost("bs/create")]
        [Authorize]
        public ActionResult<GenericResponse> CreateNewBusinessScore([FromBody] DbBusinessScore businessScore)
        {
            try
            {
                using DbLib db = new();
                db.CreateBusinessScore(businessScore);
                return new GenericResponse();
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"Error creating business score : [{e.Message}]");
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Get BP by idEtab.
        /// </summary>
        /// <param name="idEtab"></param>
        [HttpPost("bp/id-etab")]
        [Authorize]
        public ActionResult<GetBusinessResponse> GetBusinessProfileByIdEtab([FromBody] string idEtab)
        {
            try
            {
                using DbLib db = new();
                string id = idEtab.Trim();
                DbBusinessProfile? bp = db.GetBusinessByIdEtab(id);

                if (bp == null)
                    return new GetBusinessResponse(null);

                DbBusinessScore? bs = db.GetBusinessScoreByIdEtab(id);

                Business business = new(bp, bs);

                return new GetBusinessResponse(business);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessResponse.Exception($"Error getting business by id etab : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP smooth list by id etab list.
        /// </summary>
        /// <param name="idEtabList"></param>
        [HttpPost("bp/id-etab-list")]
        [Authorize]
        public ActionResult<GetBusinessListResponse> GetBusinessListByIdEtab([FromBody] List<string> idEtabList)
        {
            try
            {
                using DbLib db = new();
                List<Business?>? bpList = [];

                foreach (var idEtab in idEtabList)
                {
                    string id = idEtab.Trim();
                    DbBusinessProfile? bp = db.GetBusinessByIdEtab(id);

                    if (bp == null)
                        break;

                    DbBusinessScore? bs = db.GetBusinessScoreByIdEtab(id);

                    Business? business = new(bp, bs);
                    bpList.Add(business);
                }

                return new GetBusinessListResponse(bpList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessListResponse.Exception($"Error getting business list by id etab list : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP by place id.
        /// </summary>
        /// <param name="placeId"></param>
        [HttpPost("bp/place-id")]
        [Authorize]
        public ActionResult<GetBusinessResponse> GetBusinessByPlaceId([FromBody] string placeId)
        {
            try
            {
                using DbLib db = new();
                string id = placeId.Trim();

                DbBusinessProfile? bp = db.GetBusinessByPlaceId(id);

                if (bp == null)
                    return new GetBusinessResponse(null);

                DbBusinessScore? bs = db.GetBusinessScoreByIdEtab(id);

                Business? business = new(bp, bs);

                return new GetBusinessResponse(business);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessResponse.Exception($"Error getting business by place id : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP smooth list by place id list.
        /// </summary>
        /// <param name="placeIdList"></param>
        [HttpPost("bp/place-id-list")]
        [Authorize]
        public ActionResult<GetBusinessListResponse> GetBusinessListByPlaceId([FromBody] List<string> placeIdList)
        {
            try
            {
                using DbLib db = new();
                List<Business?>? bpList = [];

                foreach (var placeId in placeIdList)
                {
                    string id = placeId.Trim();
                    DbBusinessProfile? bp = db.GetBusinessByPlaceId(id);

                    if (bp == null)
                        continue;

                    DbBusinessScore? bs = db.GetBusinessScoreByIdEtab(bp.IdEtab);

                    Business business = new(bp, bs);

                    bpList.Add(business);
                }

                return new GetBusinessListResponse(bpList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessListResponse.Exception($"Error getting business list by place id list : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP by url.
        /// </summary>
        /// <param name="url"></param>
        [HttpPost("bp/url")]
        [Authorize]
        public async Task<ActionResult<GetBusinessResponse>> GetBusinessByUrlAsync([FromBody] string url)
        {
            try
            {
                using DbLib db = new();

                DbBusinessProfile? bp = db.GetBusinessByUrl(url);

                if (bp != null)
                {
                    DbBusinessScore? bs = db.GetBusinessScoreByIdEtab(bp.IdEtab);
                    Business? business = new(bp, bs);
                    return new GetBusinessResponse(business);
                }

                ScannerFunctions scannerFunction = new();
                SeleniumDriver driver = new();

                GetBusinessProfileRequest request = new(url);
                (DbBusinessProfile? bp2, DbBusinessScore? bs2) = await ScannerFunctions.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);

                DbBusinessProfile? bpInDb = db.GetBusinessByIdEtab(bp2.IdEtab);

                if (bpInDb != null)
                    return new GetBusinessResponse(new(bpInDb, bs2));
                else
                    return new GetBusinessResponse(null);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessResponse.Exception($"Error getting business by url : [{url}]. Error : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP list by url list.
        /// </summary>
        /// <param name="urlList"></param>
        [HttpPost("bp/url-list")]
        [Authorize]
        public async Task<ActionResult<GetBusinessListResponse>> GetBusinessListByUrlAsync([FromBody] List<string> urlList)
        {
            try
            {
                using DbLib db = new();
                List<Business?>? bpList = [];

                SeleniumDriver driver = new();
                ScannerFunctions scannerFunction = new();
                int i = 0;

                string outputFilePath = "file2.csv";
                using StreamWriter writer = new(outputFilePath, true);

                

                foreach (string url in urlList)
                {
                    DbBusinessProfile? bp = db.GetBusinessByUrl(url);

                    if (bp != null)
                    {
                        Business b = new(bp, db.GetBusinessScoreByIdEtab(bp.IdEtab))
                        {
                            Id = url
                        };
                        bpList.Add(b);
                        await writer.WriteAsync(b.Id + ";" + b.IdEtab + ";" + b.PlaceId + Environment.NewLine);
                        continue;
                    }

                    if (i == 50)
                    {
                        driver.Dispose();
                        driver = new();
                        i = 0;
                    }   

                    try
                    {
                        GetBusinessProfileRequest request = new(url);
                        (DbBusinessProfile? bp2, DbBusinessScore? bs) = await ScannerFunctions.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null, false);

                        DbBusinessProfile? bpInDb = db.GetBusinessByIdEtab(bp2.IdEtab);

                        if (bpInDb != null)
                        {
                            Business b = new(bpInDb, db.GetBusinessScoreByIdEtab(bpInDb.IdEtab))
                            {
                                Id = url
                            };
                            bpList.Add(b);
                            await writer.WriteAsync(b.Id + ";" + b.IdEtab + ";" + b.PlaceId + Environment.NewLine);
                        }
                        else
                        {
                            bpList.Add(null);
                            await writer.WriteAsync(url + ";" + "nothing found" + Environment.NewLine);
                        }
                            
                        
                        i++;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                        bpList.Add(null);
                        await writer.WriteAsync(url + ";" + "nothing found" + Environment.NewLine);
                    }
                }

                driver.Dispose();
                return new GetBusinessListResponse(bpList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessListResponse.Exception($"Error getting business list by url list. Error : [{e.Message}]");

            }
        }
        /// <summary>
        /// Get IDs ETAB by given Place IDs.
        /// </summary>
        /// <param name="placeIdList"></param>
        /// <returns>List of id Etab or empty if not found</returns>
        [HttpPost("id-etab/place-id-list")]
        [Authorize]
        public ActionResult<GetIdEtabResponse> GetIdEtabsByPlaceIds([FromBody] List<string> placeIdList)
        {
            try
            {
                using DbLib db = new();
                List<string> idsEtab = [];

                foreach (string placeId in placeIdList)
                {
                    string? id = db.GetIdEtabByPlaceId(placeId);
                    if (id != null)
                        idsEtab.Add(id);
                    else
                        idsEtab.Add(placeId);
                }

                return new GetIdEtabResponse(idsEtab);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetIdEtabResponse.Exception($"Error getting id etab list by place id list : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get ID ETAB by given Place ID.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Id etab or empty if not found</returns>
        [HttpPost("id-etab/place-id")]
        [Authorize]
        public ActionResult<GetIdEtabResponse> GetIdEtabByPlaceId([FromBody] string placeId)
        {
            try
            {
                using DbLib db = new();
                List<string> idsEtab = [];

                string? id = db.GetIdEtabByPlaceId(placeId);

                if (id != null)
                    idsEtab.Add(id);

                return new GetIdEtabResponse(idsEtab);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetIdEtabResponse.Exception($"Error getting id etab by place id : [{e.Message}]");
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update BP.
        /// </summary>
        /// <param name="business"></param>
        [HttpPut("bp/update")]
        [Authorize]
        public ActionResult<GenericResponse> UpdateBusinessProfile([FromBody] Business business)
        {
            try
            {
                using DbLib db = new();
                db.UpdateBusinessProfileFromWeb(business);

                return new GenericResponse();
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"Error updating business profile : [{e.Message}]");
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete business profile (with score, reviews feeling and reviews).
        /// </summary>
        /// <param name="idEtab"></param>
        [HttpPost("bp/delete")]
        [Authorize]
        public ActionResult<GenericResponse> DeleteBusinessProfile([FromBody] string idEtab)
        {
            try
            {
                DbLib db = new();

                DbBusinessProfile? bp = db.GetBusinessByIdEtab(idEtab);

                if (bp == null)
                    GenericResponse.Exception("Business Profile doesn't exist");

                List<DbBusinessReview> businessReviews = db.GetBusinessReviewsList(idEtab);
                foreach (DbBusinessReview review in businessReviews)
                {
                    db.DeleteBusinessReviewsFeeling(review.IdReview);
                }
                db.DeleteBusinessReviews(idEtab);
                db.DeleteBusinessScore(idEtab);
                db.DeleteBusinessProfile(idEtab);
                db.DeleteBusinessUrlByGuid(bp.FirstGuid);

                return new GenericResponse(null, $"Deleted BP with idEtab = [{idEtab}]");

            } catch (Exception e)
            {
                Log.Error(e, $"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"An exception occurred while deleting BP with idEtab = [{idEtab}] : {e.Message}");
            }
        }
        /// <summary>
        /// Delete business profile list (with score, reviews feeling and reviews).
        /// </summary>
        /// <param name="idEtabList"></param>
        [HttpPost("bp/delete-list")]
        [Authorize]
        public ActionResult<GenericResponse> DeleteBusinessProfileList([FromBody] List<string> idEtabList)
        {
            try
            {
                DbLib db = new();
                List<DbBusinessProfile> bpList = [];

                foreach(string idEtab in idEtabList)
                {
                    DbBusinessProfile? bp = db.GetBusinessByIdEtab(idEtab);

                    if (bp == null)
                        return GenericResponse.Exception($"Business Profile with idEtab=[{idEtab}] doesn't exist");
                       
                    bpList.Add(bp);
                }

                foreach(DbBusinessProfile bp in bpList)
                {
                    List<DbBusinessReview> businessReviews = db.GetBusinessReviewsList(bp.IdEtab);
                    foreach (DbBusinessReview review in businessReviews)
                    {
                        db.DeleteBusinessReviewsFeeling(review.IdReview);
                    }
                    db.DeleteBusinessReviews(bp.IdEtab);
                    db.DeleteBusinessScore(bp.IdEtab);
                    db.DeleteBusinessProfile(bp.IdEtab);
                    db.DeleteBusinessUrlByGuid(bp.FirstGuid);
                }

                return new GenericResponse(null, $"BP list deleted sucessfully!");

            } catch (Exception e)
            {
                Log.Error(e, $"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"An exception occurred while deleting BP list : {e.Message}");
            }
        }
        #endregion

        #endregion

        #region Url
        /// <summary>
        /// Create Url.
        /// </summary>
        /// <param name="url"></param>
        [HttpPost("url/create")]
        [Authorize]
        public ActionResult<GenericResponse> CreateUrl([FromBody] string url, UrlState urlState = UrlState.NEW)
        {
            try
            {
                using DbLib db = new();
                string? urlEncoded = ToolBox.ComputeMd5Hash(url);
                DbBusinessUrl? businessUrl = db.GetBusinessUrlByUrlEncoded(urlEncoded);
                if (businessUrl == null)
                {
                    businessUrl = new(Guid.NewGuid().ToString("N"), url, "manually", urlState);
                    db.CreateBusinessUrl(businessUrl);
                }
                return new GenericResponse(businessUrl.Id, "Url created successfully.");
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while creating url: [{url}].");
                return GenericResponse.Exception($"An exception occurred while creating Url = [{url}] : {e.Message}");
            }

        }
        #endregion
    }
}