namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetLoginResponse : GenericResponse<GetLoginResponse>
    {
        public GetLoginResponse(string userName, string token)
        {
            UserName = userName;
            Token = token;
        }

#pragma warning disable CS8618
        // DO NOT USE THIS CONSTRUCTOR
        public GetLoginResponse() { }
#pragma warning restore CS8618

        public string UserName { get; set; }
        public string Token { get; set; }
    }
}