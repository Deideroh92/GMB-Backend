namespace GMS.Business.Api.Models
{
    #region Requests

    public class GetReviewsRequest {
        public string? IdEtab { get; set; }
        DateTime? DateLimit { get; set; }

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="idEtab"></param>
        /// <param name="dateLimit"></param>
        public GetReviewsRequest(string idEtab, DateTime dateLimit)
        {
            IdEtab = idEtab;
            DateLimit = dateLimit;
        }
        #endregion
    }
    #endregion
}
