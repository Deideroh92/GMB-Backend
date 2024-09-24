namespace Sdk.Core.Types.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="placeId"></param>
    /// <param name="score"></param>
    /// <param name="year"></param>
    public class DbSticker(string? placeId, float score, int year)
    {
        public string? PlaceId { get; set; } = placeId;
        public float Score { get; set; } = score;
        public int Year { get; set; } = year;
    }
}