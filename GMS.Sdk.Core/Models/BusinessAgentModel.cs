using GMS.Sdk.Core.DbModels;

namespace GMS.Sdk.Core.Models
{
    public enum Operation {
        URL_STATE,
        CATEGORY,
        FILE
    }

    #region Requests

    public class BusinessAgentRequest {
        public List<DbBusinessAgent>? BusinessList { get; set; }
        public bool GetReviews { get; set; }
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
        public BusinessAgentRequest(Operation operation, bool getReviews, List<DbBusinessAgent>? businessList, DateTime? dateLimit = null, DriverType driverType = DriverType.CHROME) {
            GetReviews = getReviews;
            DriverType = driverType;
            DateLimit = dateLimit;
            BusinessList = businessList;
            Operation = operation;
        }
        #endregion
    }
    #endregion
}
