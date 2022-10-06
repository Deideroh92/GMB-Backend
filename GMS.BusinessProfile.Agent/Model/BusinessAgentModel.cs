using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;

namespace GMS.BusinessProfile.Agent.Model {
    #region Requests

    public class BusinessAgentRequest {
        public List<string>? UrlList { get; set; }
        public string? Category { get; set; }
        public UrlState? UrlState { get; set; }
        public bool GetReviews { get; set; }
        public int? Entries { get; set; }
        public DateTime? DateLimit { get; set; }
        public DriverType DriverType { get; set; }

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="getReviews"></param>
        /// <param name="entries"></param>
        /// <param name="dateLimit"></param>
        /// <param name="category"></param>
        /// <param name="urlState"></param>
        /// <param name="driverType"></param>
        public BusinessAgentRequest(bool getReviews, int? entries, List<string>? urlList,  DateTime? dateLimit = null, string? category = null, UrlState? urlState = null, DriverType driverType = DriverType.CHROME) {
            UrlList = urlList;
            GetReviews = getReviews;
            Entries = entries;
            Category = category;
            UrlState = urlState;
            DriverType = driverType;
            DateLimit = dateLimit;
        }
        #endregion
    }
    #endregion
}
