using GMS.Sdk.Core.Models;

namespace GMS.Url.Agent.Model
{
    #region Requests

    public class UrlAgentRequest {
        public List<string> Locations { get; set; }
        public string TextSearch { get; set; }

        public DriverType DriverType { get; set; }

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="textSearch"></param>
        /// <param name="driverType"></param>
        /// <param name="isTest"></param>
        public UrlAgentRequest(List<string> locations, string textSearch, DriverType driverType = DriverType.CHROME) {
            Locations = locations;
            TextSearch = textSearch;
            DriverType = driverType;
        }
        #endregion
    }
    #endregion
}
