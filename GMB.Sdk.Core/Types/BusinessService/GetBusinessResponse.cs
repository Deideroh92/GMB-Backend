using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessResponse : GenericResponse<GetBusinessResponse>
    {
        public GetBusinessResponse(Business? business, bool isNew = false, List<DbBusinessReview>? reviewList = null)
        {
            Business = business;
            IsNew = isNew;
            ReviewList = reviewList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessResponse() { }

        public Business? Business { get; set; }
        public bool IsNew { get; set; }
        public List<DbBusinessReview>? ReviewList { get; set; }
    }
}