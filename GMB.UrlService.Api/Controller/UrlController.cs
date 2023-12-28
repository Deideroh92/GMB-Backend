using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Models;
using Serilog;
using GMB.Url.Api.Models;

namespace GMB.Url.Api
{
    /// <summary>
    /// Url Service.
    /// </summary>
    public class UrlController {

        /// <summary>
        /// Create a BU.
        /// </summary>
        /// <param name="url"></param>
        public static DbBusinessUrl CreateUrl(string url, UrlState urlState = UrlState.NEW) {
            try
            {
                using DbLib db = new();
                string? urlEncoded = ToolBox.ComputeMd5Hash(url);
                DbBusinessUrl? businessUrl = db.GetBusinessUrlByUrlEncoded(urlEncoded);
                if (businessUrl == null)
                {
                    businessUrl = new(Guid.NewGuid().ToString("N"), url, "manually", ToolBox.ComputeMd5Hash(url), urlState);
                    db.CreateBusinessUrl(businessUrl);
                }
                return businessUrl;
            } catch (Exception e)
            {
                Log.Error(e, $"An exception occurred while creating business url: [{url}].");
                throw new Exception();
            }
        }
        /// <summary>
        /// Get Urls from a request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of urls found on Google</returns>
        public static List<string>? GetUrls(UrlRequest request) {
            using SeleniumDriver driver = new();
            List<string>? urls = new();

            foreach (string location in request.Locations) {
                try {
                    string textSearch = request.TextSearch + "+" + location;
                    string url = "https://www.google.com/maps/search/" + textSearch;
                    urls = UrlService.GetUrlsFromGooglePage(driver, url);
                }
                catch (Exception e) {
                    Log.Error(e, $"An exception occurred while searching for business urls with search: [{request.TextSearch + "+" + location}] : {e.Message}");
                }
            }
            return urls;
        }
    }
}
