using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessListResponse : GenericResponse<GetBusinessListResponse>
    {
        public GetBusinessListResponse(List<Business>? businessList)
        {
            BusinessList = businessList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessListResponse() { }

        public List<Business>? BusinessList { get; set; }
    }
}