using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public class CreateBusinessListRequest(List<Business>? businessList)
    {
        public List<Business>? BusinessList { get; set; } = businessList;
    }
}