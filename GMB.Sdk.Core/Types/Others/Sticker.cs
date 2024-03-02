namespace GMB.Sdk.Core.Types.Models
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="idEtab"></param>
    /// <param name="placeId"></param>
    /// <param name="rating"></param>
    /// <param name="year"></param>
    public class Sticker(string idEtab, string? placeId, float rating, int year)
    {
        public string IdEtab { get; set; } = idEtab;
        public string? PlaceId { get; set; } = placeId;
        public float Rating { get; set; } = rating;
        public int Year { get; set; } = year;
    }
}