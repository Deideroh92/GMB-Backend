using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessProfileSmoothListResponse : GenericResponse<GetBusinessProfileSmoothListResponse>
    {
        public GetBusinessProfileSmoothListResponse(List<BusinessProfileSmooth?>? businessList)
        {
            BusinessProfileList = businessList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessProfileSmoothListResponse() { }

        public List<BusinessProfileSmooth?>? BusinessProfileList { get; set; }
    }
}