namespace GMB.Sdk.Core.Types.Api
{
    public class GetPlaceIdResponse : GenericResponse<GetPlaceIdResponse>
    {
        public GetPlaceIdResponse(string? placeId)
        {
            PlaceId = placeId;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetPlaceIdResponse() { }

        public string? PlaceId { get; set; }

    }
}
