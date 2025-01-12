namespace GMB.Sdk.Core.Types.Api
{
    public class UpdateBusinessReviewReplyRequest(string id, bool replied)
    {
        public string Id { get; set; } = id;
        public bool Replied { get; set; } = replied;
    }
}