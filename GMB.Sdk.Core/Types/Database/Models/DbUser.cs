namespace GMB.Sdk.Core.Types.Database.Models
{
    public class DbUser(string login, string? password, long id = -500)
    {
        public long Id { get; set; } = id;
        public string Login { get; set; } = login;
        public string? Password { get; set; } = password;
    }
}