namespace GMB.Sdk.Core.Types.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="placeId"></param>
    /// <param name="rating"></param>
    /// <param name="year"></param>
    public class Sticker(string id, string? placeId, float? rating, int? year)
    {
        public string Id { get; set; } = id;
        public string? PlaceId { get; set; } = placeId;
        public float? Rating { get; set; } = rating;
        public int? Year { get; set; } = year;
    }
}