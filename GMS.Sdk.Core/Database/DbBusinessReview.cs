namespace GMS.Sdk.Core.Database
{
    public class DbBusinessReview : IEquatable<DbBusinessReview?>
    {
        public long Id { get; set; }
        public long IdEtab { get; set; }
        public long IdReview { get; set; }

        public DbGoogleUser User { get; set; }
        public int Score { get; set; }
        public string? Review { get; set; }
        public string? ReviewGoogleDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool ReviewReplied { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }

        #region Equality

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DbBusinessReview);
        }

        public bool Equals(DbBusinessReview? obj)
        {
            return obj is DbBusinessReview review &&
                   IdEtab == review.IdEtab &&
                   IdReview == review.IdReview &&
                   EqualityComparer<DbGoogleUser>.Default.Equals(User, review.User) &&
                   Score == review.Score &&
                   Review == review.Review &&
                   ReviewGoogleDate == review.ReviewGoogleDate &&
                   ReviewDate == review.ReviewDate &&
                   ReviewReplied == review.ReviewReplied &&
                   DateInsert == review.DateInsert &&
                   DateUpdate == review.DateUpdate;
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(IdEtab);
            hash.Add(IdReview);
            hash.Add(User);
            hash.Add(Score);
            hash.Add(Review);
            hash.Add(ReviewGoogleDate);
            hash.Add(ReviewDate);
            hash.Add(ReviewReplied);
            hash.Add(DateInsert);
            hash.Add(DateUpdate);
            return hash.ToHashCode();
        }
        #endregion

        #region Local

        /// <summary>
        /// Create an instance of Business Review.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="idReview"></param>
        /// <param name="user"></param>
        /// <param name="score"></param>
        /// <param name="review"></param>
        /// <param name="reviewGoogleDate"></param>
        /// <param name="reviewDate"></param>
        /// <param name="reviewReplied"></param>
        /// <param name="dateInsert"></param>
        /// <param name="dateUpdate"></param>
        public DbBusinessReview(long idEtab, long idReview, DbGoogleUser user, int score, string? review, string? reviewGoogleDate, DateTime? reviewDate, bool reviewReplied, DateTime? dateInsert, DateTime? dateUpdate)
        {
            IdEtab = idEtab;
            IdReview = idReview;
            User = user;
            Score = score;
            Review = review;
            ReviewGoogleDate = reviewGoogleDate;
            ReviewDate = reviewDate;
            ReviewReplied = reviewReplied;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
        }
        #endregion
    }
}