namespace GMS.Sdk.Core.DbModels {
    #region Enums
    public enum UrlState {
        NEW,
        PROCESSING,
        UPDATED,
        TEST,
        NO_CATEGORY,
        DELETED
    }
    #endregion

    public class DbBusinessUrl : IEquatable<DbBusinessUrl?> {
        public long Id { get; set; }
        public string Guid { get; set; }
        public string Url { get; set; }
        public DateTime? DateInsert { get; set; }
        public UrlState State { get; set; }
        public string? TextSearch { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string UrlEncoded { get; set; }

        #region Equality

        public override bool Equals(object? obj) {
            return Equals(obj as DbBusinessUrl);
        }

        public bool Equals(DbBusinessUrl? other) {
            return other is not null &&
                   Guid == other.Guid &&
                   Url == other.Url &&
                   DateInsert == other.DateInsert &&
                   State == other.State &&
                   TextSearch == other.TextSearch &&
                   DateUpdate == other.DateUpdate &&
                   UrlEncoded == other.UrlEncoded;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Guid, Url, DateInsert, State, TextSearch, DateUpdate, UrlEncoded);
        }
        #endregion

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
        public DbBusinessUrl(string guid, string url, DateTime? dateInsert, UrlState state, string? textSearch, DateTime? dateUpdate, string urlEncoded) {
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