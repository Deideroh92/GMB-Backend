namespace GMB.Sdk.Core.Types.Database.Models
{
    #region Enums
    public enum UrlState
    {
        NEW,
        PROCESSING,
        UPDATED,
        NO_CATEGORY,
        DELETED
    }
    #endregion

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="url"></param>
    /// <param name="dateInsert"></param>
    /// <param name="state"></param>
    /// <param name="textSearch"></param>
    /// <param name="dateUpdate"></param>
    /// <param name="urlEncoded"></param>
    public class DbBusinessUrl(string guid, string url, string? textSearch, UrlState state = UrlState.NEW, DateTime? dateInsert = null, DateTime? dateUpdate = null)
    {
        public long Id { get; set; }
        public string Guid { get; set; } = guid;
        public string Url { get; set; } = url;
        public DateTime? DateInsert { get; set; } = dateInsert;
        public UrlState State { get; set; } = state;
        public string? TextSearch { get; set; } = textSearch;
        public DateTime? DateUpdate { get; set; } = dateUpdate;
        public string UrlEncoded { get; set; } = url == "manually" ? "manually" : ToolBox.ComputeMd5Hash(url);
    }
}
