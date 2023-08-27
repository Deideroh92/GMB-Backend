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
        /// Find GMB by query (should be name + address) then insert it in DB if new.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>GMB</returns>
        [HttpGet("find-business/{query}")]
        //[Authorize]
        public async Task<ActionResult<GetBusinessProfileResponse>> FindBusinessByQuery(string query)
        {
            string? placeId = await PlaceApi.GetPlaceId(query);
            if (placeId == null)
                return new GetBusinessProfileResponse(null, null);

            PlaceDetails? placeDetails = await PlaceApi.GetGMB(placeId);
            if (placeDetails == null || placeDetails.Url == null)
                return new GetBusinessProfileResponse(null, null);

            using DbLib db = new();

            string idEtab = ToolBox.ComputeMd5Hash(placeId);

            DbBusinessProfile? business = db.GetBusinessByIdEtab(idEtab);

            // New business
            if (business == null)
            {
                DbBusinessUrl? businessUrl = UrlController.CreateUrl(placeDetails.Url);
                DbBusinessProfile? profile = ToolBox.PlaceDetailsToBP(placeDetails, idEtab, businessUrl.Guid);

                if (placeDetails.Address != null)
                {
                    AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(placeDetails.Address);
                    if (addressResponse != null)
                    {
                        profile.AddressType = addressResponse.Features[0]?.Properties?.PropertyType;
                        profile.AddressScore = addressResponse.Features[0]?.Properties?.Score;
                        profile.IdBan = addressResponse.Features[0]?.Properties?.Id;
                        profile.StreetNumber = addressResponse.Features[0]?.Properties?.HouseNumber;
                        profile.CityCode = addressResponse.Features[0]?.Properties?.CityCode;
                    }
                }

                DbBusinessScore businessScore = new(profile.IdEtab, placeDetails.Rating, placeDetails.UserRatingsTotal);

                db.CreateBusinessProfile(profile);
                db.CreateBusinessScore(businessScore);

                return new GetBusinessProfileResponse(profile, businessScore);
            } else
            {
                db.UpdateBusinessProfileFromPlaceDetails(placeDetails, business.IdEtab);
                return new GetBusinessProfileResponse(business, db.GetBusinessScoreByIdEtab(business.IdEtab));
            }

            
        }
        /// <summary>
        /// Get GMB from Google Api.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>GMB as described in API</returns>
        [HttpGet("place/{placeId}")]
        //[Authorize]
        public async Task<ActionResult<GetPlaceDetails>> GetGMBByPlaceId(string placeId)
        {
            try
            {
                PlaceDetails? placeDetails = await PlaceApi.GetGMB(placeId);
                return new GetPlaceDetails(placeDetails);
            } catch (Exception ex)
            {
                return GetPlaceDetails.Exception(ex.Message);
            }
        }
        /// <summary>
        /// Get GMB from Google Api.
        /// </summary>  
        /// <param name="query"></param>
        /// <returns>GMB as described in API</returns>
        [HttpPost("place-id")]
        //[Authorize]
        public async Task<ActionResult<GetPlaceIdResponse>> GetPlaceIdByQuery([FromBody] string query)
        {
            try
            {
                string? placeId = await PlaceApi.GetPlaceId(query);
                return new GetPlaceIdResponse(placeId);
            } catch (Exception ex)
            {
                return GetPlaceIdResponse.Exception(ex.Message);
            }
            
        }
    }
}