namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetLoginRequest
    {
        public GetLoginRequest(string? login, string? password)
        {
            this.Login = login;
            this.Password = password;
        }
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}