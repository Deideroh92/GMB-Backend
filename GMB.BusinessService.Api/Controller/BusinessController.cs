using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
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
        /// Create Business Profile.
        /// </summary>
        /// <param name="businessProfile"></param>
        [HttpPost("bp/create")]
        [Authorize]
        public ActionResult<GenericResponse> CreateNewBusiness([FromBody] DbBusinessProfile businessProfile)
        {
            try
            {
                using DbLib db = new();

                if (businessProfile.PlaceId == null)
                    return GenericResponse.Exception("No Place ID, can't create the business profile.");

                if (businessProfile.PlaceUrl == null)
                    return GenericResponse.Exception("No URL specified, can't create the business profile.");

                if (db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(businessProfile.PlaceUrl)))
                    return GenericResponse.Exception("URL already exists in DB.");

                if (db.CheckBusinessProfileExistByIdEtab(businessProfile.IdEtab))
                    return GenericResponse.Exception("Business Profile (ID ETAB) already exists in DD.");

                if (db.CheckBusinessProfileExistByPlaceId(businessProfile.PlaceId))
                    return GenericResponse.Exception("Business Profile (PLACE ID) already exists in DB.");

                string guid = Guid.NewGuid().ToString("N");
                db.CreateBusinessUrl(new DbBusinessUrl(guid, businessProfile.PlaceUrl, "platform", ToolBox.ComputeMd5Hash(businessProfile.PlaceUrl)));

                businessProfile.FirstGuid = guid;

                db.CreateBusinessProfile(businessProfile);

                return new GenericResponse(businessProfile.Id, "Business Profile successfully created in DB.");
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"Error creating business : [{e.Message}]");
            }
        }
        /// <summary>
        /// Create Business Profile list.
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("bp/create-list")]
        [Authorize]
        public ActionResult<CreateBusinessResponse> CreateNewBusinessList([FromBody] CreateBusinessListRequest request)
        {
            try
            {
                using DbLib db = new();

                List<string> errorList = [];
                List<string?> idEtabList = [];

                foreach (DbBusinessProfile businessProfile in request.BusinessProfileList)
                {
                    if (businessProfile.PlaceUrl == null)
                    {
                        errorList.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + " does not have place URL");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (db.CheckBusinessProfileExistByIdEtab(businessProfile.IdEtab))
                    {
                        errorList.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + $" ID_ETAB exist in our DB");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (businessProfile.PlaceId != null && db.CheckBusinessProfileExistByPlaceId(businessProfile.PlaceId))
                    {
                        errorList.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + $" PLACE_ID exist in our DB");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (request.BusinessScoreList.Find(x => x.IdEtab == businessProfile.IdEtab) == null)
                    {
                        errorList.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + " does not have business score");
                        idEtabList.Add(null);
                        continue;
                    }

                    if (businessProfile.PlaceUrl != null && db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(businessProfile.PlaceUrl)))
                    {
                        errorList.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + $" does have an existing URL [{businessProfile.PlaceUrl}] in DB");
                        idEtabList.Add(null);
                        continue;
                    }

                    string guid = Guid.NewGuid().ToString("N");
                    businessProfile.FirstGuid = guid;

                    db.CreateBusinessUrl(new DbBusinessUrl(guid, businessProfile.PlaceUrl ?? "manually", "platform", ToolBox.ComputeMd5Hash(businessProfile.PlaceUrl ?? "manually")));
                    db.CreateBusinessProfile(businessProfile);
                    db.CreateBusinessScore(request.BusinessScoreList.Find(x => x.IdEtab == businessProfile.IdEtab)!);
                    idEtabList.Add(businessProfile.IdEtab);
                }

                errorList.Add(request.BusinessProfileList.Count - errorList.Count + "/" + request.BusinessProfileList.Count + " businesses treated.");

                return new CreateBusinessResponse(errorList, idEtabList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return CreateBusinessResponse.Exception($"Error creating businesses : [{e.Message}]");
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
        public ActionResult<GetBusinessProfileResponse> GetBusinessProfileByIdEtab([FromBody] string idEtab)
        {
            try
            {
                using DbLib db = new();
                DbBusinessProfile? businessProfile = db.GetBusinessByIdEtab(idEtab);
                DbBusinessScore? businessScore = db.GetBusinessScoreByIdEtab(idEtab);

                return new GetBusinessProfileResponse(businessProfile, businessScore);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessProfileResponse.Exception($"Error getting business profile by id etab : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP by idEtab.
        /// </summary>
        /// <param name="idEtabList"></param>
        [HttpPost("bp/id-etab-list")]
        [Authorize]
        public ActionResult<GetBusinessProfileSmoothListResponse> GetBusinessProfileSmoothListByIdEtab([FromBody] List<string> idEtabList)
        {
            try
            {
                using DbLib db = new();
                List<BusinessProfileSmooth?> bpList = [];
                foreach ( var idEtab in idEtabList )
                {
                    bpList.Add(db.GetBusinessSmoothByIdEtab(idEtab));
                }

                return new GetBusinessProfileSmoothListResponse(bpList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessProfileSmoothListResponse.Exception($"Error getting business profile smooth list by id etab list : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP smooth list by place id.
        /// </summary>
        /// <param name="placeId"></param>
        [HttpPost("bp/place-id")]
        [Authorize]
        public ActionResult<GetBusinessProfileResponse> GetBusinessProfileByPlaceId([FromBody] string placeId)
        {
            try
            {
                using DbLib db = new();
                DbBusinessProfile? businessProfile = db.GetBusinessByPlaceId(placeId);

                if (businessProfile == null)
                    return new GetBusinessProfileResponse(null, null);

                DbBusinessScore? businessScore = db.GetBusinessScoreByIdEtab(businessProfile.IdEtab);

                return new GetBusinessProfileResponse(businessProfile, businessScore);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessProfileResponse.Exception($"Error getting business profile by place id : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP smooth list by id etab.
        /// </summary>
        /// <param name="placeIdList"></param>
        [HttpPost("bp/place-id-list")]
        [Authorize]
        public ActionResult<GetBusinessProfileSmoothListResponse> GetBusinessProfileSmoothListByPlaceId([FromBody] List<string> placeIdList)
        {
            try
            {
                using DbLib db = new();
                List<BusinessProfileSmooth?> bpList = [];
                foreach (var placeId in placeIdList)
                {
                    bpList.Add(db.GetBusinessSmoothByPlaceId(placeId));
                }

                return new GetBusinessProfileSmoothListResponse(bpList);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessProfileSmoothListResponse.Exception($"Error getting business profile smooth list by place id list : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get BP by url.
        /// </summary>
        /// <param name="url"></param>
        [HttpPost("bp/url")]
        [Authorize]
        public ActionResult<GetBusinessProfileResponse> GetBusinessProfileByUrl([FromBody] string url)
        {
            try
            {
                using DbLib db = new();
                DbBusinessUrl? businessUrl = db.GetBusinessUrlByUrlEncoded(ToolBox.ComputeMd5Hash(url));

                if (businessUrl == null)
                    return new GetBusinessProfileResponse(null, null);

                DbBusinessProfile? businessProfile = db.GetBusinessByGuid(businessUrl.Guid);
                DbBusinessScore? businessScore = db.GetBusinessScoreByIdEtab(businessProfile.IdEtab);

                return new GetBusinessProfileResponse(businessProfile, businessScore);
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetBusinessProfileResponse.Exception($"Error getting business profile by url : [{e.Message}]");

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
                return GetIdEtabResponse.Exception($"Error getting business profile by id etab : [{e.Message}]");
            }
        }
        /// <summary>
        /// Get ID ETAB by given Place ID.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Id etab list with the id etab or empty if not found</returns>
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
                return GetIdEtabResponse.Exception($"Error getting business profile by id etab : [{e.Message}]");
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update BP.
        /// </summary>
        /// <param name="businessProfile"></param>
        [HttpPut("bp/update")]
        [Authorize]
        public ActionResult<GenericResponse> UpdateBusinessProfile([FromBody] DbBusinessProfile businessProfile)
        {
            try
            {
                using DbLib db = new();
                db.UpdateBusinessProfileFromWeb(businessProfile);

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
                    businessUrl = new(Guid.NewGuid().ToString("N"), url, "manually", ToolBox.ComputeMd5Hash(url), urlState);
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