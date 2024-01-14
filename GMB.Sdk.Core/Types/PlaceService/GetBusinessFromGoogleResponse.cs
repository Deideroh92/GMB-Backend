using GMB.Sdk.Core.Types.PlaceService;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessFromGoogleResponse : GenericResponse<GetBusinessFromGoogleResponse>
    {
        public GetBusinessFromGoogleResponse(GoogleResponse? bp)
        {
            Business = bp;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessFromGoogleResponse() { }

        public GoogleResponse? Business { get; set; }
    }
}