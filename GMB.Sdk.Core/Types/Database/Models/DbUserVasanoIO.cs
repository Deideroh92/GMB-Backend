namespace GMB.Sdk.Core.Types.Database.Models
{
    public class DbUserVasanoIO(string firstName, string LastName, string email, string id,string country)
    {
        public string Id { get; set; } = id;
        public string FirstName { get; set; } = firstName;
        public string LasttName { get; set; } = LastName;
        public string Email { get; set; } = email;
        public string Country { get; set; } = country;
    }
}