namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="url"></param>
    /// <param name="idEtab"></param>
    public class BusinessAgent(string? guid, string url, string? idEtab = null)
    {
        public string? IdEtab { get; set; } = idEtab;
        public string? Guid { get; set; } = guid;
        public string Url { get; set; } = url;
    }
}