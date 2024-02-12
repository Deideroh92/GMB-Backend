using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessResponse : GenericResponse<GetBusinessResponse>
    {
        public GetBusinessResponse(Business? business, bool isNew = false)
        {
            Business = business;
            IsNew = isNew;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessResponse() { }

        public Business? Business { get; set; }
        public bool IsNew { get; set; }
    }
}