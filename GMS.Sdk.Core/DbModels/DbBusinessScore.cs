namespace GMS.Sdk.Core.Database {
    public class DbBusinessScore : IEquatable<DbBusinessScore?> {
        public long Id { get; set; }
        public string IdEtab { get; set; }
        public float? Score { get; set; }
        public int? NbReviews { get; set; }
        public DateTime? DateInsert { get; set; }

        #region Equality
        public override bool Equals(object? obj) {
            return Equals(obj as DbBusinessScore);
        }

        public bool Equals(DbBusinessScore? other) {
            return other is not null &&
                   IdEtab == other.IdEtab &&
                   Score == other.Score &&
                   NbReviews == other.NbReviews &&
                   DateInsert == other.DateInsert;
        }

        public override int GetHashCode() {
            return HashCode.Combine(IdEtab, Score, NbReviews, DateInsert);
        }
        #endregion

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="score"></param>
        /// <param name="nbReviews"></param>
        /// <param name="dateInsert"></param>
        public DbBusinessScore(string idEtab, float? score, int? nbReviews, DateTime? dateInsert) {
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
            if (Score != null && NbReviews == null)
                Score = null;

            if (NbReviews != null && Score == null)
                Score = null;
        }
        #endregion
    }
}