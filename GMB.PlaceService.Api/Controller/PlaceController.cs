using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMB.PlaceService.Api.Controller
{
    /// <summary>
    /// Place Controller.
    /// Manage all operations about Google Place API.
    /// </summary>
    [ApiController]
    [Route("api/place-service")]
    public sealed class PlaceController : ControllerBase
    {
        /// <summary>
        /// Find GMB by query (should be name + address).
        /// </summary>
        /// <param name="query"></param>
        /// <returns>GMB</returns>
        [HttpGet("place/find-by-query/{query}")]
        [Authorize]
        public async Task<ActionResult<GetBusinessProfileListResponse>> GetPlaceByQuery(string query)
        {

            List<DbBusinessProfile> businessProfiles = [];
            List<DbBusinessScore> businessScores = [];
            Place[]? places = await Core.PlaceService.GetPlacesByQuery(query);

            foreach (Place place in places)
            {
                DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                if (bp != null)
                    businessProfiles.Add(bp);
                businessScores.Add(new(bp.IdEtab, place.Rating, place.UserRatingCount));
            }
            return new GetBusinessProfileListResponse(businessProfiles, businessScores);
        }
        /// <summary>
        /// Find GMB list by query (should be name + address).
        /// </summary>
        /// <param name="queryList"></param>
        /// <returns>GMB list or null if nothing found</returns>
        [HttpPost("place/find-by-query")]
        [Authorize]
        public async Task<ActionResult<GetBusinessProfileListResponse>> GetPlacesByQueryList(List<string> queryList)
        {
            List<DbBusinessProfile> businessProfiles = [];
            List<DbBusinessScore> businessScores = [];

            try
            {
                foreach (string query in queryList)
                {
                    Place[]? places = await Core.PlaceService.GetPlacesByQuery(query);

                    if (places != null && places.Length != 0)
                    {
                        DbBusinessProfile? bp = ToolBox.PlaceToBP(places[0]);
                        if (bp != null)
                            businessProfiles.Add(bp);
                        businessScores.Add(new(bp.IdEtab, places[0].Rating, places[0].UserRatingCount));
                    }
                }
                return new GetBusinessProfileListResponse(businessProfiles, businessScores);
            } catch (Exception ex)
            {
                return GetBusinessProfileListResponse.Exception(ex.Message);
            }
        }
        /// <summary>
        /// Find GMB by Place ID.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>GMB or null if not found</returns>
        [HttpGet("place/find-by-place-id/{placeId}")]
        [Authorize]
        public async Task<ActionResult<GetBusinessProfileResponse>> GetPlaceByPlaceId(string placeId)
        {
            try
            {
                Place? place = await Core.PlaceService.GetPlaceByPlaceId(placeId);
                if (place != null)
                {
                    DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                    DbBusinessScore? bs = new(bp.IdEtab, place.Rating, place.UserRatingCount);
                    return new GetBusinessProfileResponse(bp, bs, true);
                }
                return new GetBusinessProfileResponse(null, null, true);
            } catch (Exception ex)
            {
                return GetBusinessProfileResponse.Exception(ex.Message);
            }
        }
        /// <summary>
        /// Find GMB list by Place IDs.
        /// </summary>
        /// <param name="placeIds"></param>
        /// <returns>GMB list or null if nothing found</returns>
        [HttpPost("place/find-by-place-id")]
        [Authorize]
        public async Task<ActionResult<GetBusinessProfileListResponse>> GetPlacesByPlaceIdList(List<string> placeIds)
        {
            try
            {
                List<DbBusinessProfile> bpList = [];
                List<DbBusinessScore> bsList = [];

                foreach (string placeId in placeIds)
                {
                    Place? place = await Core.PlaceService.GetPlaceByPlaceId(placeId);
                    if (place != null)
                    {
                        DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                        DbBusinessScore? bs = new(bp.IdEtab, place.Rating, place.UserRatingCount);
                        bpList.Add(bp);
                        bsList.Add(bs);
                    }
                }

                return new GetBusinessProfileListResponse(bpList, bsList);
            } catch (Exception ex)
            {
                return GetBusinessProfileListResponse.Exception(ex.Message);
            }
        }
        /// <summary>
        /// Get Place ID from Google Api.
        /// </summary>  
        /// <param name="query"></param>
        /// <returns>GMB as described in API</returns>
        [HttpPost("place-id")]
        [Authorize]
        public async Task<ActionResult<GetPlaceIdResponse>> GetPlaceIdByQuery([FromBody] string query)
        {
            try
            {
                string? placeId = await Core.PlaceService.GetPlaceIdByQuery(query);
                return new GetPlaceIdResponse(placeId);
            } catch (Exception ex)
            {
                return GetPlaceIdResponse.Exception(ex.Message);
            }

        }
    }
}