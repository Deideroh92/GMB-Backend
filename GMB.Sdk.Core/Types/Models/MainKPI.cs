namespace GMB.Sdk.Core.Types.Models
{
    public class MainKPI
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="businessTotal"></param>
        /// <param name="businessNetworkTotal"></param>
        /// <param name="businessReviewsTotal"></param>
        /// <param name="businessReviewsFeelingTotal"></param>
        /// <param name="brandTotal"></param>
        public MainKPI(int? businessTotal, int? businessNetworkTotal, int? businessReviewsTotal, int? businessReviewsFeelingTotal, int? brandTotal)
        {
            BusinessTotal = businessTotal;
            BusinessNetworkTotal = businessNetworkTotal;
            BusinessReviewsTotal = businessReviewsTotal;
            BusinessReviewsFeelingTotal = businessReviewsFeelingTotal;
            BrandTotal = brandTotal;
        }

        public int? BusinessTotal { get; set; }
        public int? BusinessNetworkTotal { get; set; }
        public int? BusinessReviewsTotal { get; set; }
        public int? BusinessReviewsFeelingTotal { get; set; }
        public int? BrandTotal { get; set; }
    }
}
