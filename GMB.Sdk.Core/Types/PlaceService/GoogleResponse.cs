using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.PlaceService
{
    public class GoogleResponse(string query, DbBusinessProfile? profile, DbBusinessScore? score)
    {
        public string Query { get; set; } = query;
        public DbBusinessProfile? Profile { get; set; } = profile;
        public DbBusinessScore? Score { get; set; } = score;
    }
}
