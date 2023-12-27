namespace GMB.Sdk.Core.Types.Api
{
    public sealed class CreateBusinessResponse : GenericResponse<CreateBusinessResponse>
    {
        public CreateBusinessResponse(List<string>? idErrors)
        {
            this.IdErrors = idErrors;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public CreateBusinessResponse() { }

        public List<string>? IdErrors { get; set; }
    }
}