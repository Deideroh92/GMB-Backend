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

    public class DbBusinessUrl : IDisposable {
        public long Id { get; set; }
        public string Guid { get; set; }
        public string Url { get; set; }
        public DateTime? DateInsert { get; set; }
        public UrlState State { get; set; }
        public string? TextSearch { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string UrlEncoded { get; set; }

        private bool disposed = false;

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
        public DbBusinessUrl(string guid, string url, string? textSearch, DateTime? dateUpdate, string urlEncoded, UrlState state = UrlState.NEW, DateTime? dateInsert = null) {
            Guid = guid;
            Url = url;
            DateInsert = dateInsert;
            State = state;
            TextSearch = textSearch;
            DateUpdate = dateUpdate;
            UrlEncoded = urlEncoded;
        }

        /// <summary>
        /// Dispose method implementation.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method implementation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    // Dispose managed resources here
                }

                // Dispose unmanaged resources here

                disposed = true;
            }
        }

        #endregion

        // Finalizer
        ~DbBusinessUrl() {
            Dispose(false);
        }
    }
}
