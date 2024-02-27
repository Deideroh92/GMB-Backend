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

            List<GoogleResponse> businessList = [];
            Place[]? places = await Core.PlaceService.GetPlacesByQuery(query);

            foreach (Place place in places)
            {
                Business? bp = ToolBox.PlaceToB(place);
                if (bp == null || bp.IdEtab == null)
                {
                    businessList.Add(new(query, null));
                    continue;
                }
            }
            return new GetBusinessListFromGoogleResponse(businessList);
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
            List<GoogleResponse> businessList = [];

            try
            {
                foreach (string query in queryList)
                {
                    Place[]? places = await Core.PlaceService.GetPlacesByQuery(query);

                    if (places != null && places.Length != 0)
                    {
                        Business? bp = ToolBox.PlaceToB(places[0]);
                        if (bp == null || bp.IdEtab == null)
                        {
                            businessList.Add(new(query, null));
                            continue;
                        }
                        businessList.Add(new(query, bp));
                    }
                }
                return new GetBusinessListFromGoogleResponse(businessList);
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
                    return new GetBusinessFromGoogleResponse(new(placeId, null));
                }

                Business? bp = ToolBox.PlaceToB(place);
                if (bp == null || bp.IdEtab == null)
                {
                    return new GetBusinessFromGoogleResponse(new(placeId, null));
                }
                return new GetBusinessFromGoogleResponse(new(placeId, bp));
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
                        Business? bp = ToolBox.PlaceToB(place);
                        if (bp == null || bp.IdEtab == null)
                        {
                            bpList.Add(new(placeId, null));
                            continue;
                        }
                        bpList.Add(new(placeId, bp));
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