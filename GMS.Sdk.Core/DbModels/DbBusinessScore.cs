namespace GMS.Sdk.Core.DbModels {
    public class DbBusinessScore {
        public long Id { get; set; }
        public string IdEtab { get; set; }
        public float? Score { get; set; }
        public int? NbReviews { get; set; }
        public DateTime? DateInsert { get; set; }

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="score"></param>
        /// <param name="nbReviews"></param>
        /// <param name="dateInsert"></param>
        public DbBusinessScore(string idEtab, float? score, int? nbReviews, DateTime? dateInsert = null) {
            IdEtab = idEtab;
            Score = score;
            NbReviews = nbReviews;
            DateInsert = dateInsert;

            CheckValidity();
        }

        /// <summary>
        /// Checking if Business Score is valid.
        /// </summary>
        public void CheckValidity() {
            if ((Score != null && NbReviews == null) || (NbReviews != null && Score == null))
                Score = null;
        }
        #endregion
    }
}