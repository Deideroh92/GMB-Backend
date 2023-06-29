namespace GMS.Sdk.Core.DbModels {
    public class DbBusinessAgent {
        public string? IdEtab { get; set; }
        public string? Guid { get; set; }
        public string Url { get; set; }

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="guid"></param>
        /// <param name="url"></param>
        /// <param name="idEtab"></param>
        public DbBusinessAgent(string? guid, string url, string? idEtab = null) {
            IdEtab = idEtab;
            Guid = guid;
            Url = url;
        }
        #endregion
    }
}
