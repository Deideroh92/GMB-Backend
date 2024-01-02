namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetIdEtabResponse : GenericResponse<GetIdEtabResponse>
    {
        public GetIdEtabResponse(List<string>? idEtabList)
        {
            IdEtabList = idEtabList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetIdEtabResponse() { }

        public List<string>? IdEtabList { get; set; }
        public bool IsNew { get; set; }
    }
}