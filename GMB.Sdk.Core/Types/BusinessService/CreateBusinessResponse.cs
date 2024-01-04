namespace GMB.Sdk.Core.Types.Api
{
    public sealed class CreateBusinessResponse : GenericResponse<CreateBusinessResponse>
    {
        public CreateBusinessResponse(List<string>? idErrors, List<string?>? idEtabs)
        {
            IdErrors = idErrors;
            IdEtabs = idEtabs;

        }

        // DO NOT USE THIS CONSTRUCTOR
        public CreateBusinessResponse() { }

        public List<string>? IdErrors { get; set; }
        public List<string?>? IdEtabs { get; set; }
    }
}