using GMS.Sdk.Core.SeleniumDriver;

namespace GMS.Url.Agent.Model {
    #region Requests

    public class UrlAgentRequest : IEquatable<UrlAgentRequest?> {
        public string TextSearch { get; set; }

        public DriverType DriverType { get; set; }

        public bool IsTest { get; set; }

        #region Equality

        public override bool Equals(object? obj) {
            return Equals(obj as UrlAgentRequest);
        }

        public bool Equals(UrlAgentRequest? other) {
            return other is not null &&
                   TextSearch == other.TextSearch &&
                   DriverType == other.DriverType &&
                   IsTest == other.IsTest;
        }

        public override int GetHashCode() {
            return HashCode.Combine(TextSearch, DriverType, IsTest);
        }
        #endregion

        #region Local

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="textSearch"></param>
        /// <param name="driverType"></param>
        /// <param name="isTest"></param>
        public UrlAgentRequest(string textSearch, DriverType driverType = DriverType.CHROME, bool isTest = false) {
            TextSearch = textSearch;
            DriverType = driverType;
            IsTest = isTest;
        }
        #endregion
    }
    #endregion
}
