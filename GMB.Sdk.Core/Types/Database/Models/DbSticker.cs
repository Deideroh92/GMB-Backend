namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="placeId"></param>
    /// <param name="score"></param>
    /// <param name="createdDate"></param>
    /// <param name="image"></param>
    public class DbSticker(string id, string placeId, string score, DateTime createdDate, byte[] image)
    {
        public string Id { get; set; } = id;
        public string PlaceId { get; set; } = placeId;
        public string Score { get; set; } = score;
        public byte[] Image { get; set; } = image;
        public DateTime CreatedDate { get; set; } = createdDate;
    }
}