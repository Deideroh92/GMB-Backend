using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Database.Models
{
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
    /// <param name="visitDate"></param>
    public class DbBusinessReview(string idEtab, string idReview, string googleReviewId, GoogleUser user, int score, string? reviewText, string? reviewGoogleDate, DateTime? reviewDate, bool reviewReplied, DateTime? dateUpdate, DateTime? reviewReplyDate, string? reviewReplyGoogleDate, string? visitDate, DbBusinessReviewReply? reviewReply = null, DateTime? dateInsert = null, bool deleted = false, DateTime? lastSeenAt = null)
    {
        public long Id { get; set; }
        public string IdEtab { get; set; } = idEtab;
        public string IdReview { get; set; } = idReview;
        public string GoogleReviewId { get; set; } = googleReviewId;
        public GoogleUser User { get; set; } = user;
        public int Score { get; set; } = score;
        public string? ReviewText { get; set; } = reviewText;
        public string? ReviewGoogleDate { get; set; } = reviewGoogleDate;
        public DateTime? ReviewDate { get; set; } = reviewDate;
        public DateTime? ReviewReplyDate { get; set; } = reviewReplyDate;
        public string? ReviewReplyGoogleDate { get; set; } = reviewReplyGoogleDate;
        public bool ReviewReplied { get; set; } = reviewReplied;
        public DateTime? DateInsert { get; set; } = dateInsert;
        public DateTime? DateUpdate { get; set; } = dateUpdate;
        public DbBusinessReviewReply? ReviewReply { get; set; } = reviewReply;
        public string? VisitDate { get; set; } = visitDate;
        public bool? Deleted { get; set; } = deleted;

        public DateTime? LastSeenAt { get; set; } = lastSeenAt;

        #region Local

        public bool Equals(DbBusinessReview other)
        {
            return other.ReviewText == ReviewText && other.Score == Score && other.User.Name == User.Name && other.User.LocalGuide == User.LocalGuide && other.ReviewReplied == ReviewReplied && other.VisitDate == VisitDate;
        }
        public bool EqualsWithStatus(DbBusinessReview other)
        {
            return other.ReviewText == ReviewText && other.Score == Score && other.User.Name == User.Name && other.User.NbReviews == User.NbReviews && other.User.LocalGuide == User.LocalGuide && other.ReviewReplied == ReviewReplied & other.Deleted == Deleted;
        }
        #endregion
    }

    public class ReviewData
    {
        public string ReviewId { get; set; }
        public string ReviewGoogleDateUpdate { get; set; }
        public DateTime DateInsert { get; set; }
        public DateTime NewDate { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}