using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Scanner.Agent.Models
{
    #region Enum
    public enum Operation
    {
        URL_STATE,
        PROCESSING_STATE,
    }
    #endregion

    #region Requests
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="textSearch"></param>
    /// <param name="deptSearch"></param>
    /// <param name="cityCodeSearch"></param>
    public class ScannerUrlRequest(List<string>? locations, string textSearch, bool deptSearch = false, bool cityCodeSearch = false)
    {
        public bool DeptSearch { get; set; } = deptSearch;
        public bool CityCodeSearch { get; set; } = cityCodeSearch;
        public List<string>? Locations { get; set; } = locations;
        public string TextSearch { get; set; } = textSearch;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="getReviews"></param>
    /// <param name="businessList"></param>
    /// <param name="dateLimit"></param>
    /// <param name="updateProcessingState"></param>
    public class ScannerBusinessRequest(Operation operation, bool getReviews, List<BusinessAgent>? businessList, DateTime? dateLimit = null, bool updateProcessingState = true)
    {
        public List<BusinessAgent>? BusinessList { get; set; } = businessList;
        public bool GetReviews { get; set; } = getReviews;
        public bool UpdateProcessingState { get; set; } = updateProcessingState;
        public DateTime? DateLimit { get; set; } = dateLimit;
        public Operation Operation { get; set; } = operation;
    }
    #endregion
}
