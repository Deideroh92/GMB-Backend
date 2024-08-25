using GMB.Sdk.Core.Types.Api;

namespace GMB.Sdk.Core.Types.PlaceService
{
    public class GetPlacesRequest : GenericResponse<GetPlacesRequest>
    {
        public GetPlacesRequest(List<string> queries, string lang = "en")
        {
            Queries = queries;
            Lang = lang;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetPlacesRequest() { }

        public List<string> Queries { get; set; }

        public string Lang { get; set; }

    }
}
