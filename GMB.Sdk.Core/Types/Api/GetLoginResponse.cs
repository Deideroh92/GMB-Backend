namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetLoginResponse : GenericResponse<GetLoginResponse>
    {
        public GetLoginResponse(string userName, string token)
        {
            this.UserName = userName;
            this.Token = token;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetLoginResponse() { }

        public string UserName { get; set; }
        public string Token { get; set; }
    }
}