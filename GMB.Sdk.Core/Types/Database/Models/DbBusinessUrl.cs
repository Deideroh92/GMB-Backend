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

    public class DbBusinessUrl
    {
        public long Id { get; set; }
        public string Guid { get; set; }
        public string Url { get; set; }
        public DateTime? DateInsert { get; set; }
        public UrlState State { get; set; }
        public string? TextSearch { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string UrlEncoded { get; set; }

        #region Local
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
        public DbBusinessUrl(string guid, string url, string? textSearch, string urlEncoded, UrlState state = UrlState.NEW, DateTime? dateInsert = null, DateTime? dateUpdate = null)
        {
            Guid = guid;
            Url = url;
            DateInsert = dateInsert;
            State = state;
            TextSearch = textSearch;
            DateUpdate = dateUpdate;
            UrlEncoded = urlEncoded;
        }
        #endregion
    }
}
