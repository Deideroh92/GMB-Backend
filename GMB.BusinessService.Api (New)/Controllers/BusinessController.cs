using GMB.Sdk.Core;
using Serilog;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Business.Api.Models;
using GMB.Url.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GMB.Sdk.Core.Types.Api;

namespace GMB.BusinessService.Api.Controllers
{
    /// <summary>
    /// Business Service Controller.
    /// </summary>
    [ApiController]
    [Route("api/business-service")]
    public class BusinessController
    {

        #region Profile & Score

        #region Create
        /// <summary>
        /// Create a new business url and business if it doesn't exist already.
        /// </summary>
        /// <param name="url"></param>
        [HttpPost("bp/create/by-url")]
        [Authorize]
        public async void CreateNewBusinessProfileByUrl(string url)
        {
            try
            {
                DbBusinessUrl businessUrl = UrlController.CreateUrl(url);

                using SeleniumDriver driver = new();

                GetBusinessProfileRequest request = new(businessUrl.Url, businessUrl.Guid, null);
                (DbBusinessProfile? profile, DbBusinessScore? score) = await Core.BusinessService.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
                DbLib db = new();

                // Update or insert Business Profile if exist or not.
                if (db.CheckBusinessProfileExistByIdEtab(profile.IdEtab))
                    db.UpdateBusinessProfile(profile);
                else
                    db.CreateBusinessProfile(profile);

                // Insert Business Score if have one.
                if (score?.Score != null)
                    db.CreateBusinessScore(score);
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while inserting new BU and BP with url = [{url}] : {e.Message}");
            }
        }
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

                List<string> ids = new();

                foreach (DbBusinessProfile businessProfile in request.BusinessProfileList)
                {
                    if (businessProfile.PlaceUrl == null)
                    {
                        ids.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + " does not have place URL");
                        continue;
                    }

                    if (db.CheckBusinessProfileExistByIdEtab(businessProfile.IdEtab))
                    {
                        ids.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + $" ID_ETAB exist in our DB");
                        continue;
                    }

                    if (businessProfile.PlaceId != null && db.CheckBusinessProfileExistByPlaceId(businessProfile.PlaceId))
                    {
                        ids.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + $" PLACE_ID exist in our DB");
                        continue;
                    }

                    if (request.BusinessScoreList.Find(x => x.IdEtab == businessProfile.IdEtab) == null)
                    {
                        ids.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + " does not have business score");
                        continue;
                    }

                    if (businessProfile.PlaceUrl != null && db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(businessProfile.PlaceUrl)))
                    {
                        ids.Add($"ID_ETAB: [{businessProfile.IdEtab}] - PLACE_ID : [{businessProfile.PlaceId}]" + $" does have an existing URL [{businessProfile.PlaceUrl}] in DB");
                        continue;
                    }

                    string guid = Guid.NewGuid().ToString("N");
                    businessProfile.FirstGuid = guid;

                    db.CreateBusinessUrl(new DbBusinessUrl(guid, businessProfile.PlaceUrl ?? "manually", "platform", ToolBox.ComputeMd5Hash(businessProfile.PlaceUrl ?? "manually")));
                    db.CreateBusinessProfile(businessProfile);
                    db.CreateBusinessScore(request.BusinessScoreList.Find(x => x.IdEtab == businessProfile.IdEtab)!);
                }

                ids.Add(request.BusinessProfileList.Count - ids.Count + "/" + request.BusinessProfileList.Count + " businesses treated.");

                return new CreateBusinessResponse(ids);
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
        /// Get BP by placeId.
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

                return new GenericResponse(null, $"BP with idEtab = [{idEtab}]");

            } catch (Exception e)
            {
                Log.Error(e, $"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GenericResponse.Exception($"An exception occurred while deleting BP with idEtab = [{idEtab}] : {e.Message}");
            }
        }
        #endregion

        #endregion

        #region Reviews
        /// <summary>
        /// Get Business Reviews list from url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dateLimit"></param>
        /// <param name="idEtab"></param>
        /// <returns>List of business reviews</returns>
        [HttpGet("bp/reviews/{url}")]
        [Authorize]
        public List<DbBusinessReview>? GetBusinessReviews(string url, DateTime? dateLimit, string idEtab = "-1")
        {
            using SeleniumDriver driver = new();

            driver.GetToPage(url);
            try
            {
                return Core.BusinessService.GetReviews(idEtab, dateLimit, driver);
            } catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region KPI
        /// <summary>
        /// Get KPI.
        /// </summary>
        [HttpGet("kpi")]
        [Authorize]
        public ActionResult<GetMainKpiResponse> GetKpi()
        {
            try
            {
                using DbLib db = new();
                int? brTotal = db.GetBRTotal();
                int? bpTotal = db.GetBRTotal();
                int? bpNetworkTotal = db.GetBPNetworkTotal();
                int? brFeelingTotal = db.GetBRFeelingsTotal();
                int? brandTotal = db.GetBrandTotal();
                MainKPI kpi = new(bpTotal, bpNetworkTotal, brTotal, brFeelingTotal, brandTotal);
                return new GetMainKpiResponse();
            } catch (Exception e)
            {
                Log.Error($"Exception = [{e.Message}], Stack = [{e.StackTrace}]");
                return GetMainKpiResponse.Exception("Error getting KPIs.");
            }
        }
        #endregion
    }
}