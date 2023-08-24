namespace GMB.Sdk.Core.Types.Database.Models
{
    public class DbUser
    {
        public DbUser(string login, string? password, long id = -500)
        {
            Login = login;
            Password = password;
            Id = id;
        }

        public long Id { get; set; }
        public string Login { get; set; }
        public string? Password { get; set; }
    }
}