using GMS.Sdk.Core.SeleniumDriver;

namespace GMS.Url.Agent.Model {
    #region Requests

    public class UrlAgentRequest {
        public string TextSearch { get; set; }

        public DriverType DriverType { get; set; }

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="textSearch"></param>
        /// <param name="driverType"></param>
        /// <param name="isTest"></param>
        public UrlAgentRequest(string textSearch, DriverType driverType = DriverType.CHROME) {
            TextSearch = textSearch;
            DriverType = driverType;
        }
        #endregion
    }
    #endregion
}
