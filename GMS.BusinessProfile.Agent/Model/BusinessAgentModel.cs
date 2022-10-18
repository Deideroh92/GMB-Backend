using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;

namespace GMS.BusinessProfile.Agent.Model {
    public enum Operation {
        URL_STATE,
        CATEGORY,
        FILE
    }

    #region Requests

    public class BusinessAgentRequest {
        public List<DbBusinessAgent>? BusinessList { get; set; }
        public string? Category { get; set; }
        public UrlState? UrlState { get; set; }
        public bool GetReviews { get; set; }
        public int? Entries { get; set; }
        public DateTime? DateLimit { get; set; }
        public DriverType DriverType { get; set; }
        public Operation Operation { get; set; }

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
        public BusinessAgentRequest(Operation operation, bool getReviews, int? entries, List<DbBusinessAgent>? businessList, DateTime? dateLimit = null, string? category = null, UrlState? urlState = null, DriverType driverType = DriverType.CHROME) {
            GetReviews = getReviews;
            Entries = entries;
            Category = category;
            UrlState = urlState;
            DriverType = driverType;
            DateLimit = dateLimit;
            BusinessList = businessList;
            Operation = operation;
        }
        #endregion
    }
    #endregion
}
