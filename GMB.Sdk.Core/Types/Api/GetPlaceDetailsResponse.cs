using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class GetPlaceDetailsResponse : GenericResponse<GetPlaceDetailsResponse>
    {
        public GetPlaceDetailsResponse(PlaceDetailsResponse? placeDetails)
        {
            PlaceDetails = placeDetails;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetPlaceDetailsResponse() { }

        public PlaceDetailsResponse? PlaceDetails { get; set; }

    }
}
