using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Models;
using Serilog;
using System.Globalization;
using GMB.Url.Api.Models;

namespace GMB.Url.Api
{
    /// <summary>
    /// Url Service.
    /// </summary>
    public class UrlController {

        /// <summary>
        /// Start the URL Scraper.
        /// </summary>
        /// <param name="request"></param>
        public static void Scraper(UrlRequest request) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-{Date}.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 7, fileSizeLimitBytes: 5242880)
            .CreateLogger();

            using DbLib db = new();
            using SeleniumDriver driver = new();
            List<string>? urls = new();

            string[]? locations = request.CustomLocations.ToArray() ?? null;

            if (request.DeptSearch)
                locations = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "DeptList.txt"));
            if (request.CityCodeSearch)
                locations = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "CpList.txt"));

            foreach (string location in locations) {
                ToolBox.BreakingHours();

                try {
                    string textSearch = request.TextSearch + "+" + location;
                    string url = "https://www.google.com/maps/search/" + textSearch;
                    urls = UrlService.GetUrlsFromGooglePage(driver, url);

                    if (urls == null)
                        continue;

                    foreach(string urlToValidate in urls) {
                        if (!db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash(url))) {
                            DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, textSearch, null, ToolBox.ComputeMd5Hash(url));
                            db.CreateBusinessUrl(businessUrl);
                        }
                    }
                } catch (Exception e) {
                    Log.Error(e, $"An exception occurred while searching for business urls with search: [{request.TextSearch + "+" + location}] : {e.Message}");
                }
            }
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Create a BU.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="date"></param>
        public static void CreateUrl(string url, DateTime? date = null) {
            using DbLib db = new();

            DbBusinessUrl businessUrl = new(Guid.NewGuid().ToString("N"), url, "manually", date, ToolBox.ComputeMd5Hash(url), UrlState.NEW, date);
            db.CreateBusinessUrl(businessUrl);
        }
        /// <summary>
        /// Get Urls from a request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of urls found on Google</returns>
        public static List<string>? GetUrls(UrlRequest request) {
            using SeleniumDriver driver = new();
            List<string>? urls = new();
            string[]? locations = request.CustomLocations.ToArray() ?? null;

            if (request.DeptSearch)
                locations = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "DeptList.txt"));
            if (request.CityCodeSearch)
                locations = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "CpList.txt"));

            foreach (string location in locations) {
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
