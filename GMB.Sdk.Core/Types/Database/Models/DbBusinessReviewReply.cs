namespace GMB.Sdk.Core.Types.Database.Models
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reviewText"></param>
    /// <param name="idReview"></param>
    /// <param name="reviewGoogleDate"></param>
    /// <param name="reviewDate"></param>
    /// <param name="dateInsert"></param>
    /// <param name="dateUpdate"></param>
    public class DbBusinessReviewReply(string reviewText, string idReview, string? reviewGoogleDate, DateTime? reviewDate, DateTime? dateInsert, DateTime? dateUpdate)
    {
        public long Id { get; set; }
        public string IdReview { get; set; } = idReview;
        public string ReviewText { get; set; } = reviewText;
        public string? ReviewGoogleDate { get; set; } = reviewGoogleDate;
        public DateTime? ReviewDate { get; set; } = reviewDate;
        public DateTime? DateInsert { get; set; } = dateInsert;
        public DateTime? DateUpdate { get; set; } = dateUpdate;
    }
}