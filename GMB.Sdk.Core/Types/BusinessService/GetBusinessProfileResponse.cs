using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessProfileResponse : GenericResponse<GetBusinessProfileResponse>
    {
        public GetBusinessProfileResponse(DbBusinessProfile? business, DbBusinessScore? businessScore, bool isNew = false)
        {
            BusinessProfile = business;
            IsNew = isNew;
            BusinessScore = businessScore;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessProfileResponse() { }

        public DbBusinessProfile? BusinessProfile { get; set; }
        public DbBusinessScore? BusinessScore { get; set; }
        public bool IsNew { get; set; }
    }
}