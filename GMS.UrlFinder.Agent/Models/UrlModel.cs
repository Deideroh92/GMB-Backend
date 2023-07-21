namespace GMS.Url.Api.Models
{
    #region Requests

    public class UrlRequest
    {
        public bool DeptSearch { get; set; }
        public bool CityCodeSearch { get; set; }
        public List<string>? CustomLocations { get; set; }
        public string TextSearch { get; set; }

        #region Local
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="customLocations"></param>
        /// <param name="textSearch"></param>
        /// <param name="deptSearch"></param>
        /// <param name="cityCodeSearch"></param>
        public UrlRequest(List<string>? customLocations, string textSearch, bool deptSearch = false, bool cityCodeSearch = false)
        {
            DeptSearch = deptSearch;
            CityCodeSearch = cityCodeSearch;
            CustomLocations = customLocations;
            TextSearch = textSearch;
        }
        #endregion
    }
    #endregion
}
