namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetBusinessRequest : GenericResponse<GetBusinessResponse>
    {
        public GetBusinessRequest(string id, bool getReviews = false)
        {
            Id = id;
            GetReviews = getReviews;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetBusinessRequest() { }

        public string Id { get; set; }
        public bool GetReviews { get; set; }
    }
}