using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class CreateBusinessListRequest(List<DbBusinessProfile>? businessProfileList, List<DbBusinessScore>? businessScoreList)
    {
        public List<DbBusinessProfile>? BusinessProfileList { get; set; } = businessProfileList;
        public List<DbBusinessScore>? BusinessScoreList { get; set; } = businessScoreList;
    }
}