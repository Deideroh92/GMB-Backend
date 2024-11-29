namespace GMB.Sdk.Core.Types.Database.Models
{
    public class DbUserVasanoIO(string name, string email, string id)
    {
        public string Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Email { get; set; } = email;
    }
}