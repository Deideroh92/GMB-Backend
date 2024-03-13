namespace GMB.Sdk.Core.Types.Api
{
    public class UpdateBusinessProcessingListRequest(List<string> idEtabList, int processing)
    {
        public List<string> IdEtabList { get; set; } = idEtabList;
        public int Processing { get; set; } = processing;
    }
}