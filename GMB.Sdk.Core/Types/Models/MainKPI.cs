namespace GMB.Sdk.Core.Types.Models
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="businessTotal"></param>
    /// <param name="businessNetworkTotal"></param>
    /// <param name="businessReviewsTotal"></param>
    /// <param name="businessReviewsFeelingTotal"></param>
    /// <param name="brandTotal"></param>
    public class MainKPI(int? businessTotal, int? businessNetworkTotal, int? businessReviewsTotal, int? businessReviewsFeelingTotal, int? brandTotal)
    {
        public int? BusinessTotal { get; set; } = businessTotal;
        public int? BusinessNetworkTotal { get; set; } = businessNetworkTotal;
        public int? BusinessReviewsTotal { get; set; } = businessReviewsTotal;
        public int? BusinessReviewsFeelingTotal { get; set; } = businessReviewsFeelingTotal;
        public int? BrandTotal { get; set; } = brandTotal;
    }
}
