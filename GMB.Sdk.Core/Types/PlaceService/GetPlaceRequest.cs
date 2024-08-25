using GMB.Sdk.Core.Types.Api;

namespace GMB.Sdk.Core.Types.PlaceService
{
    public class GetPlaceRequest : GenericResponse<GetPlaceRequest>
    {
        public GetPlaceRequest(string query, string lang = "fr")
        {
            Query = query;
            Lang = lang;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetPlaceRequest() { }

        public string Query { get; set; }

        public string Lang { get; set; }

    }
}
