namespace GMS.Url.Agent.Model
{
    #region Requests

    public class UrlFinderRequest {
        public List<string> Locations { get; set; }
        public string TextSearch { get; set; }

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="textSearch"></param>
        public UrlFinderRequest(List<string> locations, string textSearch) {
            Locations = locations;
            TextSearch = textSearch;
        }
        #endregion
    }
    #endregion
}
