namespace GMB.Sdk.Core.Types.Models
{
    #region Enum
    public enum Operation {
        URL_STATE,
        OTHER,
        FILE
    }
    #endregion

    public class BusinessAgent
    {
        public string? IdEtab { get; set; }
        public string? Guid { get; set; }
        public string Url { get; set; }

        #region Local

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="guid"></param>
        /// <param name="url"></param>
        /// <param name="idEtab"></param>
        public BusinessAgent(string? guid, string url, string? idEtab = null)
        {
            IdEtab = idEtab;
            Guid = guid;
            Url = url;
        }
        #endregion
    }

    #region Requests
    public class BusinessAgentRequest {
        public List<BusinessAgent>? BusinessList { get; set; }
        public bool GetReviews { get; set; }
        public bool UpdateProcessingState { get; set; }
        public DateTime? DateLimit { get; set; }
        public Operation Operation { get; set; }

        #region Local
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="getReviews"></param>
        /// <param name="businessList"></param>
        /// <param name="dateLimit"></param>
        /// <param name="updateProcessingState"></param>
        public BusinessAgentRequest(Operation operation, bool getReviews, List<BusinessAgent>? businessList, DateTime? dateLimit = null, bool updateProcessingState = true)
        {
            GetReviews = getReviews;
            DateLimit = dateLimit;
            BusinessList = businessList;
            Operation = operation;
            UpdateProcessingState = updateProcessingState;
        }
        #endregion
    }
    #endregion
}
