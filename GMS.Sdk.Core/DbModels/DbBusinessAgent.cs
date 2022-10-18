namespace GMS.BusinessProfile.Agent.Model {
    public class DbBusinessAgent {
        public long Id { get; set; }
        public string? IdEtab { get; set; }
        public string Guid { get; set; }
        public string Url { get; set; }

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="guid"></param>
        /// <param name="url"></param>
        /// <param name="idEtab"></param>
        public DbBusinessAgent(long id, string guid, string url, string? idEtab = null) {
            Id = id;
            IdEtab = idEtab;
            Guid = guid;
            Url = url;
        }
        #endregion
    }
}
