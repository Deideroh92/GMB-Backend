namespace GMS.Sdk.Core.Database {
    public class DbBusinessReview : IEquatable<DbBusinessReview?> {
        public long Id { get; set; }
        public string IdEtab { get; set; }
        public string IdReview { get; set; }
        public GoogleUser User { get; set; }
        public int Score { get; set; }
        public string? ReviewText { get; set; }
        public string? ReviewGoogleDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool ReviewReplied { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public DbBusinessReviewReply? ReviewReply { get; set; }

        #region Equality
        
        public override bool Equals(object? obj) {
            return Equals(obj as DbBusinessReview);
        }

        public bool Equals(DbBusinessReview? other) {
            return other is not null &&
                   IdEtab == other.IdEtab &&
                   IdReview == other.IdReview &&
                   EqualityComparer<GoogleUser>.Default.Equals(User, other.User) &&
                   Score == other.Score &&
                   ReviewText == other.ReviewText &&
                   ReviewGoogleDate == other.ReviewGoogleDate &&
                   ReviewDate == other.ReviewDate &&
                   ReviewReplied == other.ReviewReplied &&
                   DateInsert == other.DateInsert &&
                   DateUpdate == other.DateUpdate &&
                   EqualityComparer<DbBusinessReviewReply?>.Default.Equals(ReviewReply, other.ReviewReply);
        }

        public override int GetHashCode() {
            HashCode hash = new();
            hash.Add(IdEtab);
            hash.Add(IdReview);
            hash.Add(User);
            hash.Add(Score);
            hash.Add(ReviewText);
            hash.Add(ReviewGoogleDate);
            hash.Add(ReviewDate);
            hash.Add(ReviewReplied);
            hash.Add(DateInsert);
            hash.Add(DateUpdate);
            hash.Add(ReviewReply);
            return hash.ToHashCode();
        }
        #endregion

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="idReview"></param>
        /// <param name="user"></param>
        /// <param name="score"></param>
        /// <param name="reviewText"></param>
        /// <param name="reviewGoogleDate"></param>
        /// <param name="reviewDate"></param>
        /// <param name="reviewReplied"></param>
        /// <param name="dateInsert"></param>
        /// <param name="dateUpdate"></param>
        /// <param name="reviewReply"></param>
        public DbBusinessReview(string idEtab, string idReview, GoogleUser user, int score, string? reviewText, string? reviewGoogleDate, DateTime? reviewDate, bool reviewReplied, DateTime? dateInsert, DateTime? dateUpdate, DbBusinessReviewReply? reviewReply = null) {
            IdEtab = idEtab;
            IdReview = idReview;
            User = user;
            Score = score;
            ReviewText = reviewText;
            ReviewGoogleDate = reviewGoogleDate;
            ReviewDate = reviewDate;
            ReviewReplied = reviewReplied;
            DateInsert = dateInsert;
            DateUpdate = dateUpdate;
            ReviewReply = reviewReply;
        }
        #endregion
    }
}