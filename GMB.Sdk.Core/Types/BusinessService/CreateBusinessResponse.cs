namespace GMB.Sdk.Core.Types.Api
{
    public sealed class CreateBusinessListResponse : GenericResponse<CreateBusinessListResponse>
    {
        public CreateBusinessListResponse(List<string>? idErrors, List<string?>? idEtabs)
        {
            IdErrors = idErrors;
            IdEtabs = idEtabs;

        }

        // DO NOT USE THIS CONSTRUCTOR
        public CreateBusinessListResponse() { }

        public List<string>? IdErrors { get; set; }
        public List<string?>? IdEtabs { get; set; }
    }
}