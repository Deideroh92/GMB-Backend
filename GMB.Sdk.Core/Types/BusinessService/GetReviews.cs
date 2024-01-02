namespace GMB.BusinessService.Api.Models
{
    #region Requests
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="guid"></param>
    /// <param name="idEtab"></param>
    public class GetBusinessProfileRequest(string url, string? guid = null, string? idEtab = null)
    {
        public string Url { get; set; } = url;
        public string? Guid { get; set; } = guid;
        public string? IdEtab { get; set; } = idEtab;
    }
    #endregion
}
