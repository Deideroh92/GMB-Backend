using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.PlaceService
{
    public class GoogleResponse(string query, Business? business)
    {
        public string Query { get; set; } = query;
        public Business? Business { get; set; } = business;
    }
}
