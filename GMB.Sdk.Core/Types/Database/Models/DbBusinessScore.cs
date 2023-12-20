namespace GMB.Sdk.Core.Types.Database.Models
{
    public class DbBusinessScore
    {
        public long Id { get; set; }
        public string IdEtab { get; set; }
        public double? Score { get; set; }
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
        public DbBusinessScore(string idEtab, double? score, int? nbReviews, DateTime? dateInsert = null)
        {
            Id = -500;
            IdEtab = idEtab;
            Score = score;
            NbReviews = nbReviews;
            DateInsert = dateInsert;

            CheckValidity();
        }

        /// <summary>
        /// Checking if Business Score is valid.
        /// </summary>
        public void CheckValidity()
        {
            if (Score != null && (NbReviews == null || NbReviews == 0) || NbReviews != null && (Score == null || Score == 0))
            {
                Score = null;
                NbReviews = null;
            }  
        }
        #endregion
    }
}