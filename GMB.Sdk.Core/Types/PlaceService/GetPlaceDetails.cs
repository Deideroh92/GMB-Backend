using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class GetPlaceDetails : GenericResponse<GetPlaceDetails>
    {
        public GetPlaceDetails(PlaceDetails? placeDetails)
        {
            PlaceDetails = placeDetails;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetPlaceDetails() { }

        public PlaceDetails? PlaceDetails { get; set; }

    }
}
