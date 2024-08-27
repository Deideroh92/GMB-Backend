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
        public async Task<ActionResult<GetBusinessListFromGoogleResponse>> GetPlaceByQuery(GetPlaceRequest query)
        {

            List<GoogleResponse> businessList = [];
            Place[]? places = await Service.PlaceService.GetPlacesByQuery(query.Query, query.Lang);

            foreach (Place place in places)
            {
                Business? bp = ToolBox.PlaceToB(place);
                if (bp == null || bp.IdEtab == null)
                {
                    businessList.Add(new(query.Query, null));
                    continue;
                }
                businessList.Add(new(query.Query, bp));
            }
            return new GetBusinessListFromGoogleResponse(businessList);
        }
        /// <summary>
        /// Find GMB list by query (should be name + address).
        /// </summary>
        /// <param name="query"></param>
        /// <returns>GMB list or null if nothing found</returns>
        [HttpPost("place/find-by-query")]
        [Authorize]
        public async Task<ActionResult<GetBusinessListFromGoogleResponse>> GetPlacesByQueryList(GetPlacesRequest query)
        {
            List<GoogleResponse> businessList = [];

            try
            {
                foreach (string record in query.Queries)
                {
                    Place[]? places = await Service.PlaceService.GetPlacesByQuery(record, query.Lang);

                    if (places != null && places.Length != 0)
                    {
                        Business? bp = ToolBox.PlaceToB(places[0]);
                        if (bp == null || bp.IdEtab == null)
                        {
                            businessList.Add(new(record, null));
                            continue;
                        }
                        businessList.Add(new(record, bp));
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
        /// <param name="query"></param>
        /// <returns>GMB or null if not found</returns>
        [HttpGet("place/find-by-place-id/{query}")]
        [Authorize]
        public async Task<ActionResult<GetBusinessFromGoogleResponse>> GetPlaceByPlaceId(GetPlaceRequest query)
        {
            try
            {
                Place? place = await Service.PlaceService.GetPlaceByPlaceId(query.Query, query.Lang);

                byte[]? test = await Service.PlaceService.GetPhotoById("AXCi2Q6N3kOdfqi9GaQOq0-4NOaBPCxwjI39VcHvitvNFD4XMuI_8ONn9Tooudy1uCnO3BUgLU9dJaeMpYV1dc11Nfkodaq2JyRO84-9TuNmnLHtatfgeCRgBdFBfmaW4pVpUCgH0-_MAy5wjBujf0PV76PD7TnKUBsgcJij");
                
                if (place == null)
                {
                    return new GetBusinessFromGoogleResponse(new(query.Query, null));
                }

                Business? bp = ToolBox.PlaceToB(place);
                if (bp == null || bp.IdEtab == null)
                {
                    return new GetBusinessFromGoogleResponse(new(query.Query, null));
                }
                return new GetBusinessFromGoogleResponse(new(query.Query, bp));
            } catch (Exception ex)
            {
                return GetBusinessFromGoogleResponse.Exception(ex.Message);
            }
        }

        /// <summary>
        /// Find GMB list by Place IDs.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>GMB list or null if nothing found</returns>
        [HttpPost("place/find-by-place-id")]
        [Authorize]
        public async Task<ActionResult<GetBusinessListFromGoogleResponse>> GetPlacesByPlaceIdList(GetPlacesRequest query)
        {
            try
            {
                List<GoogleResponse>? bpList = [];

                foreach (string record in query.Queries)
                {
                    Place? place = await Service.PlaceService.GetPlaceByPlaceId(record, query.Lang);
                    if (place != null)
                    {
                        Business? bp = ToolBox.PlaceToB(place);
                        if (bp == null || bp.IdEtab == null)
                        {
                            bpList.Add(new(record, null));
                            continue;
                        }
                        bpList.Add(new(record, bp));
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
                string? placeId = await Service.PlaceService.GetPlaceIdByQuery(query, "fr");
                return new GetPlaceIdResponse(placeId);
            } catch (Exception ex)
            {
                return GetPlaceIdResponse.Exception(ex.Message);
            }

        }
    }
}