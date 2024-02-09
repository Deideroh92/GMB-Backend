namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="url"></param>
    /// <param name="idEtab"></param>
    /// <param name="placeId"></param>
    public class BusinessAgent(string? guid, string url, string? idEtab = null, string? placeId = null)
    {
        public string? IdEtab { get; set; } = idEtab;
        public string? Guid { get; set; } = guid;
        public string Url { get; set; } = url;
        public string? PlaceId { get; set; } = placeId;
    }
}