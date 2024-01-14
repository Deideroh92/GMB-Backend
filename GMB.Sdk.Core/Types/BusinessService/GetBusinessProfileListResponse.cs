using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessProfileListResponse : GenericResponse<GetBusinessProfileListResponse>
    {
        public GetBusinessProfileListResponse(List<DbBusinessProfile?>? businessList, List<DbBusinessScore>? businessScoreList)
        {
            BusinessProfileList = businessList;
            BusinessScoreList = businessScoreList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessProfileListResponse() { }

        public List<DbBusinessProfile?>? BusinessProfileList { get; set; }
        public List<DbBusinessScore>? BusinessScoreList { get; set; }
    }
}