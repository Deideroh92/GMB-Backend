using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.PlaceService;
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
        public async Task<ActionResult<GetBusinessListFromGoogleResponse>> GetPlaceByQuery(string query)
        {

            List<GoogleResponse> businessProfiles = [];
            Place[]? places = await Core.PlaceService.GetPlacesByQuery(query);

            foreach (Place place in places)
            {
                DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                if (bp == null || bp.IdEtab == null)
                {
                    businessProfiles.Add(new(query, null, null));
                    continue;
                }
                DbBusinessScore bs = new(bp.IdEtab, place.Rating, place.UserRatingCount);
                businessProfiles.Add(new(query, bp, bs));
            }
            return new GetBusinessListFromGoogleResponse(businessProfiles);
        }
        /// <summary>
        /// Find GMB list by query (should be name + address).
        /// </summary>
        /// <param name="queryList"></param>
        /// <returns>GMB list or null if nothing found</returns>
        [HttpPost("place/find-by-query")]
        [Authorize]
        public async Task<ActionResult<GetBusinessListFromGoogleResponse>> GetPlacesByQueryList(List<string> queryList)
        {
            List<GoogleResponse> businessProfiles = [];

            try
            {
                foreach (string query in queryList)
                {
                    Place[]? places = await Core.PlaceService.GetPlacesByQuery(query);

                    if (places != null && places.Length != 0)
                    {
                        DbBusinessProfile? bp = ToolBox.PlaceToBP(places[0]);
                        if (bp == null || bp.IdEtab == null)
                        {
                            businessProfiles.Add(new(query, null, null));
                            continue;
                        }

                        DbBusinessScore bs = new(bp.IdEtab, places[0].Rating, places[0].UserRatingCount);
                        businessProfiles.Add(new(query, bp, bs));
                    }
                }
                return new GetBusinessListFromGoogleResponse(businessProfiles);
            } catch (Exception ex)
            {
                return GetBusinessListFromGoogleResponse.Exception(ex.Message);
            }
        }
        /// <summary>
        /// Find GMB by Place ID.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>GMB or null if not found</returns>
        [HttpGet("place/find-by-place-id/{placeId}")]
        [Authorize]
        public async Task<ActionResult<GetBusinessFromGoogleResponse>> GetPlaceByPlaceId(string placeId)
        {
            try
            {
                Place? place = await Core.PlaceService.GetPlaceByPlaceId(placeId);
                if (place == null)
                {
                    return new GetBusinessFromGoogleResponse(new(placeId, null, null));
                }

                DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                if (bp == null || bp.IdEtab == null)
                {
                    return new GetBusinessFromGoogleResponse(new(placeId, null, null));
                }

                DbBusinessScore? bs = new(bp.IdEtab, place.Rating, place.UserRatingCount);
                return new GetBusinessFromGoogleResponse(new(placeId, bp, bs));
            } catch (Exception ex)
            {
                return GetBusinessFromGoogleResponse.Exception(ex.Message);
            }
        }

        /// <summary>
        /// Find GMB list by Place IDs.
        /// </summary>
        /// <param name="placeIds"></param>
        /// <returns>GMB list or null if nothing found</returns>
        [HttpPost("place/find-by-place-id")]
        [Authorize]
        public async Task<ActionResult<GetBusinessListFromGoogleResponse>> GetPlacesByPlaceIdList(List<string> placeIds)
        {
            try
            {
                List<GoogleResponse>? bpList = [];

                foreach (string placeId in placeIds)
                {
                    Place? place = await Core.PlaceService.GetPlaceByPlaceId(placeId);
                    if (place != null)
                    {
                        DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                        if (bp == null || bp.IdEtab == null)
                        {
                            bpList.Add(new(placeId, null, null));
                            continue;
                        }
                        DbBusinessScore bs = new(bp.IdEtab, place.Rating, place.UserRatingCount);
                        bpList.Add(new(placeId, bp, bs));
                    }
                }

                return new GetBusinessListFromGoogleResponse(bpList);
            } catch (Exception ex)
            {
                return GetBusinessListFromGoogleResponse.Exception(ex.Message);
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