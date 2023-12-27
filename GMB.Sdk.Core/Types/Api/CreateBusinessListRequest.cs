using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class CreateBusinessListRequest
    {
        public CreateBusinessListRequest(List<DbBusinessProfile>? businessProfileList, List<DbBusinessScore>? businessScoreList)
        {
            this.BusinessProfileList = businessProfileList;
            this.BusinessScoreList = businessScoreList;
        }

        public List<DbBusinessProfile>? BusinessProfileList { get; set; }
        public List<DbBusinessScore>? BusinessScoreList { get; set; }
    }
}