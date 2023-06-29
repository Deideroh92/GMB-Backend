namespace GMS.Sdk.Core.DbModels {
    public class DbBusinessReviewReply : IEquatable<DbBusinessReviewReply?> {
        public long Id { get; set; }
        public string IdReview { get; set; }
        public string ReviewText { get; set; }
        public string? ReviewGoogleDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }

        #region Equality

        public override bool Equals(object? obj) {
            return Equals(obj as DbBusinessReviewReply);
        }

        public bool Equals(DbBusinessReviewReply? other) {
            return other is not null &&
                   IdReview == other.IdReview &&
                   ReviewText == other.ReviewText &&
                   ReviewGoogleDate == other.ReviewGoogleDate &&
                   ReviewDate == other.ReviewDate &&
                   DateInsert == other.DateInsert &&
                   DateUpdate == other.DateUpdate;
        }

        public override int GetHashCode() {
            return HashCode.Combine(IdReview, ReviewText, ReviewGoogleDate, ReviewDate, DateInsert, DateUpdate);
        }
        #endregion

        #region Local
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reviewText"></param>
        /// <param name="idReview"></param>
        /// <param name="reviewGoogleDate"></param>
        /// <param name="reviewDate"></param>
        /// <param name="dateInsert"></param>
        /// <param name="dateUpdate"></param>
        public DbBusinessReviewReply(string reviewText, string idReview, string? reviewGoogleDate, DateTime? reviewDate, DateTime? dateInsert, DateTime? dateUpdate) {
            ReviewText = reviewText;
            IdReview = idReview;
            ReviewGoogleDate = reviewGoogleDate;
            ReviewDate = reviewDate;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
        }
        #endregion
    }
}