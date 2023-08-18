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
        public async Task<ActionResult<GetBusinessProfileResponse?>> FindBusinessByQuery(string query)
        {
            string? placeId = await PlaceApi.GetPlaceId(query);
            if (placeId == null)
                return new GetBusinessProfileResponse(null);

            using DbLib db = new();
            string idEtab = ToolBox.ComputeMd5Hash(placeId);

            PlaceDetailsResponse? placeDetails = await PlaceApi.GetGMB(placeId);
            if (placeDetails == null || placeDetails.Result.Url == null)
                return new GetBusinessProfileResponse(null);

            DbBusinessUrl? businessUrl = db.GetBusinessUrlByUrlEncoded(ToolBox.ComputeMd5Hash(placeDetails.Result.Url));

            businessUrl ??= UrlController.CreateUrl(placeDetails.Result.Url);

            string? idBan = null;
            string? addressType = null;

            if (placeDetails.Result.FormattedAdress != null)
            {
                AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(placeDetails.Result.FormattedAdress);
                if (addressResponse != null)
                {
                    addressType = addressResponse.Features[0]?.Properties?.PropertyType;
                    idBan = addressResponse.Features[0]?.Properties?.Id;
                }
            }

            DbBusinessProfile? profile = new(
                placeDetails.Result.PlaceId,
                idEtab,
                businessUrl.Guid,
                placeDetails.Result.Name,
                placeDetails.Result.Types?.FirstOrDefault(),
                placeDetails.Result.FormattedAdress,
                placeDetails.Result.FormattedAdress,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("postal_code")).LongName,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("locality")).LongName,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("route")).LongName,
                placeDetails.Result.Geometry.Location.Latitude,
                placeDetails.Result.Geometry.Location.Longitude,
                idBan,
                addressType,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("street_number")).LongName,
                null,
                placeDetails.Result.FormattedPhoneNumber,
                placeDetails.Result.Website,
                placeDetails.Result.PlusCode.GlobalCode,
                null,
                (BusinessStatus)Enum.Parse(typeof(BusinessStatus), placeDetails.Result.BusinessStatus.ToString()),
                null,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("country")).LongName,
                placeDetails.Result.Geometry.Location.Latitude.ToString() + ", " + placeDetails.Result.Geometry.Location.Longitude.ToString());

            DbBusinessProfile? business = db.GetBusinessByIdEtab(idEtab);

            if (business == null)
                db.CreateBusinessProfile(profile);
            if (business != null && !profile.Equals(business))
                db.UpdateBusinessProfile(profile);

            return new GetBusinessProfileResponse(profile);
        }
        /// <summary>
        /// Get GMB from Google Api.
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>GMB as described in API</returns>
        [HttpGet("place/{placeId}")]
        //[Authorize]
        public async Task<ActionResult<GetPlaceDetailsResponse?>> GetGMBByPlaceId(string placeId)
        {
            return new GetPlaceDetailsResponse(await PlaceApi.GetGMB(placeId));
        }
        /// <summary>
        /// Get GMB from Google Api.
        /// </summary>  
        /// <param name="query"></param>
        /// <returns>GMB as described in API</returns>
        [HttpPost("place-id")]
        //[Authorize]
        public async Task<ActionResult<GetPlaceIdResponse?>> GetPlaceIdByQuery([FromBody] string query)
        {
            return new GetPlaceIdResponse(await PlaceApi.GetPlaceId(query));
        }
    }
}