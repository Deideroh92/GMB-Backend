namespace GMS.Sdk.Core.Types.Database.Models
{
    public class DbBusinessReviewReply
    {
        public long Id { get; set; }
        public string IdReview { get; set; }
        public string ReviewText { get; set; }
        public string? ReviewGoogleDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }

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
        public DbBusinessReviewReply(string reviewText, string idReview, string? reviewGoogleDate, DateTime? reviewDate, DateTime? dateInsert, DateTime? dateUpdate)
        {
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