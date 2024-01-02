namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetLoginRequest(string? login, string? password)
    {
        public string? Login { get; set; } = login;
        public string? Password { get; set; } = password;
    }
}