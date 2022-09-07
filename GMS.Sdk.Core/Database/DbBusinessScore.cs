namespace GMS.Sdk.Core.Database
{
    public class DbBusinessScore : IEquatable<DbBusinessScore?>
    {
        public long Id { get; set; }
        public long IdEtab { get; set; }
        public float? Score { get; set; }
        public int? NbReviews { get; set; }
        public DateTime? DateInsert { get; set; }

        #region Equality

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DbBusinessScore);
        }

        public bool Equals(DbBusinessScore? obj)
        {
            return obj is DbBusinessScore score &&
                   IdEtab == score.IdEtab &&
                   Score == score.Score &&
                   NbReviews == score.NbReviews &&
                   DateInsert == score.DateInsert;
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(IdEtab, Score, NbReviews, DateInsert);
        }
        #endregion

        #region Local

        /// <summary>
        /// Create a new instance of Business Score.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="score"></param>
        /// <param name="nbReviews"></param>
        /// <param name="dateInsert"></param>
        public DbBusinessScore(long idEtab, float? score, int? nbReviews, DateTime? dateInsert)
        {
            IdEtab = idEtab;
            Score = score;
            NbReviews = nbReviews;
            DateInsert = dateInsert;
        }

        /// <summary>
        /// Checking if score is valid.
        /// </summary>
        /// <returns>bool</returns>
        public bool CheckValidity()
        {
            if (Score != null && NbReviews == null)
                return false;

            if (NbReviews != null && Score == null)
                return false;

            return true;
        }

        /// <summary>
        /// Save object in DB.
        /// </summary>
        public void Save()
        {

        }
        #endregion
    }
}