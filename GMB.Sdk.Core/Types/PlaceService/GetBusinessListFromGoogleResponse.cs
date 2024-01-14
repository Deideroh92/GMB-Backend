using GMB.Sdk.Core.Types.PlaceService;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessListFromGoogleResponse : GenericResponse<GetBusinessListFromGoogleResponse>
    {
        public GetBusinessListFromGoogleResponse(List<GoogleResponse>? businessList)
        {
            BusinessList = businessList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessListFromGoogleResponse() { }

        public List<GoogleResponse>? BusinessList { get; set; }
    }
}