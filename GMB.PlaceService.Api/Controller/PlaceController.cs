using GMB.Sdk.Core;
using Serilog;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Url.Api;
using GMB.Sdk.Core.Types.Api;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using GMB.PlaceService.Api.API;
using Microsoft.AspNetCore.Authorization;

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
        static readonly ILogger log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Find GMB by query (should be name + address).
        /// </summary>
        /// <param name="query"></param>
        /// <returns>GMB</returns>
        [HttpGet("find-business/{query}")]
        [Authorize]
        public async Task<ActionResult<GetBusinessProfileListResponse>> FindBusinessByQuery(string query)
        {
            List<DbBusinessProfile> businessProfiles = new();
            List<DbBusinessScore> businessScores = new();
            Place[]? places = await PlaceApi.GetPlacesByQuery(query);

            foreach (Place place in places)
            {
                DbBusinessProfile? bp = ToolBox.PlaceToBP(place);
                if (bp != null)
                    businessProfiles.Add(bp);
                businessScores.Add(new(bp.IdEtab, place.Rating, place.UserRatingCount));
            }

            using DbLib db = new();

            return new GetBusinessProfileListResponse(businessProfiles, businessScores);
        }
        /// <summary>
        /// Get GMB from Google Api.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>GMB as described in API</returns>
        [HttpGet("place/{placeId}")]
        [Authorize]
        public async Task<ActionResult<GetBusinessProfileResponse>> GetPlaceByPlaceId(string placeId)
        {
            try
            {
                Place? place = await PlaceApi.GetPlaceByPlaceId(placeId);
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
        /// Get GMB from Google Api.
        /// </summary>  
        /// <param name="query"></param>
        /// <returns>GMB as described in API</returns>
        [HttpPost("place-id")]
        [Authorize]
        public async Task<ActionResult<GetPlaceIdResponse>> GetPlaceIdByQuery([FromBody] string query)
        {
            try
            {
                string? placeId = await PlaceApi.GetPlaceIdByQuery(query);
                return new GetPlaceIdResponse(placeId);
            } catch (Exception ex)
            {
                return GetPlaceIdResponse.Exception(ex.Message);
            }
            
        }
    }
}