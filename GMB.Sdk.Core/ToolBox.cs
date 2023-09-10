using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Security.Authentication;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Api;
using System.Reflection;
using System.Runtime.InteropServices;
using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core
{
    public class ToolBox
    {

        #region SubClasses
        public class GoogleDate
        {

            public string? key;
            public string? value;
        }
        #endregion

        #region Local

        /// <summary>
        /// Encode a string.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>The input string encoded.</returns>
        public static string ComputeMd5Hash(string message)
        {
            using MD5 md5 = MD5.Create();
            byte[] input = Encoding.Default.GetBytes(message.ToUpper());
            byte[] hash = md5.ComputeHash(input);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static DbBusinessProfile PlaceDetailsToBP(PlaceDetails placeDetails, string idEtab, string guid)
        {
            DbBusinessProfile? profile = new(
                placeDetails.PlaceId,
                idEtab,
                guid,
                placeDetails.Name,
                placeDetails.FirstType,
                placeDetails.Address,
                placeDetails.Address,
                placeDetails.PostalCode,
                placeDetails.City,
                null,
                placeDetails.Lat,
                placeDetails.Long,
                null,
                null,
                placeDetails.StreetNumber,
                null,
                placeDetails.Phone,
                placeDetails.Website,
                placeDetails.PlusCode,
                null,
                (BusinessStatus)Enum.Parse(typeof(BusinessStatus), placeDetails.Status!),
                null,
                placeDetails.Country,
                placeDetails.Url,
                placeDetails.Lat + " , " + placeDetails.Long,
                0,
                null,
                placeDetails.PhoneInternational
                );
            return profile;
        }

        public static PlaceDetails PlaceDetailsResponseToPlaceDetails(PlaceDetailsResponse placeDetails)
        {
            return new(
                placeDetails.Result.PlaceId!,
                placeDetails.Result.Name!,
                placeDetails.Result.Types?.FirstOrDefault(),
                placeDetails.Result.FormattedAdress,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("street_number")).LongName,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("postal_code")).LongName,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("locality")).LongName,
                placeDetails.Result.Geometry.Location.Latitude,
                placeDetails.Result.Geometry.Location.Longitude,
                placeDetails.Result.AddressComponents.Find((x) => x.Types.Contains("country")).LongName,
                placeDetails.Result.FormattedPhoneNumber,
                placeDetails.Result.InternationalPhoneNumber,
                placeDetails.Result.Website,
                placeDetails.Result.PlusCode.GlobalCode,
                placeDetails.Result.Url,
                placeDetails.Result.BusinessStatus.ToString(),
                placeDetails.Result.Rating,
                placeDetails.Result.UserRatingsTotal
                );
        }

        /// <summary>
        /// Return the current exact executable root path (folder where is located exe)
        /// </summary>
        /// <returns></returns>
        public static string GetExecutableRootPath()
        {
            return $"{(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "")}{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
        }

        /// <summary>
        /// Transform a google date into a real date.
        /// </summary>
        /// <param name="googleDate"></param>
        /// <returns>Real date from google date.</returns>
        public static DateTime ComputeDateFromGoogleDate(string? googleDate)
        {
            if (googleDate == null) return DateTime.UtcNow;

            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Sdk.Core\\Files", "GoogleDate.json");
            string json = File.ReadAllText(path);
            Dictionary<string, string>? mapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (mapper.TryGetValue(googleDate, out string? value) && int.TryParse(value, out int jsonValue))
            {
                DateTime currentDate = DateTime.UtcNow;
                

                if (googleDate.Contains("moi"))
                    return currentDate.AddMonths(-jsonValue);
                if (googleDate.Contains("an"))
                    return currentDate.AddYears(-jsonValue);
                if (googleDate.Contains("semaine"))
                    return currentDate.AddDays(-jsonValue);
                if (googleDate.Contains("jour"))
                    return currentDate.AddDays(-jsonValue);
                if (googleDate.Contains("heure"))
                    return currentDate.AddHours(-jsonValue);
            }

            return DateTime.UtcNow;
        }

        /// <summary>
        /// Same as FindElement only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <returns>Returns element or null if not found</returns>
        public static IWebElement? FindElementSafe(IWebDriver driver, List<By> by)
        {
            foreach (By item in by)
            {
                try
                {
                    return driver.FindElement(item);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Same as FindElement only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <returns>Returns element or null if not found</returns>
        public static IWebElement? FindElementSafe(IWebElement webElement, List<By> by)
        {
            foreach (By item in by)
            {
                try
                {
                    return webElement.FindElement(item);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Same as FindElements only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <returns>Returns elements or null if not found</returns>
        public static ReadOnlyCollection<IWebElement>? FindElementsSafe(IWebDriver driver, List<By> by)
        {
            foreach (By item in by)
            {
                try
                {
                    return driver.FindElements(item);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Same as FindElements only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <returns>Returns elements or null if not found</returns>
        public static ReadOnlyCollection<IWebElement>? FindElementsSafe(IWebElement webElement, List<By> by)
        {
            foreach (By item in by)
            {
                try
                {
                    return webElement.FindElements(item);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Requires finding elements by FindElementsSafe(By).
        /// Checking if web elements exist or not.
        /// </summary>
        /// <param name="elements">Current element</param>
        /// <returns>Returns T/F depending on if element is defined or null.</returns>
        public static bool Exists<T>(T elements) {
            return elements != null;
        }

        /// <summary>
        /// Getting adress splitted thanks to https://adresse.data.gouv.fr/api-doc/adresse.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>addressResponse object if adress found or null.</returns>
        public static async Task<AddressApiResponse?> ApiCallForAddress(string address) {
            using HttpClientHandler handler = new();
            handler.SslProtocols = SslProtocols.Tls12;
            using HttpClient client = new(handler);

            string apiUrl = $"https://api-adresse.data.gouv.fr/search/?q={Uri.EscapeDataString(address)}";
            string[] types = { "housenumber", "street", "locality", "municipality" };
            foreach (string type in types) {
                try {
                    HttpResponseMessage response = await client.GetAsync(apiUrl + $"&type={type}");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    AddressApiResponse? addressResponse = AddressApiResponse.FromJson(responseBody);

                    if (addressResponse?.Features?.Length > 0) {
                        return addressResponse;
                    }
                } catch (Exception) {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Insert Api adress inside Business profile.
        /// </summary>
        /// <param name="business"></param>
        /// <param name="address"></param>
        /// <returns>Business with address updated</returns>
        public static DbBusinessProfile InsertApiAddressInBusiness(DbBusinessProfile business, AddressApiResponse address) {
            business.Lon = (double?)address.Features[0]?.Geometry?.Coordinates[0];
            business.Lat = (double?)address.Features[0]?.Geometry?.Coordinates[1];
            business.City = address.Features[0]?.Properties?.City;
            business.PostCode = address.Features[0]?.Properties?.Postcode;
            business.CityCode = address.Features[0]?.Properties?.CityCode;
            business.Address = address.Features[0]?.Properties?.Street;
            business.AddressType = address.Features[0]?.Properties?.PropertyType;
            business.IdBan = address.Features[0]?.Properties?.Id;
            business.StreetNumber = address.Features[0]?.Properties?.HouseNumber;

            return business;
        }


        /// <summary>
        /// Breaking hours, when the program needs to pause.
        /// </summary>
        public static void BreakingHours() {
            DateTime actualTime = DateTime.UtcNow;

            // Breaking hours
            TimeSpan heureDebut = new(1, 0, 0); // 1AM
            TimeSpan heureFin = new(3, 0, 0); // 3AM

            while (actualTime.TimeOfDay >= heureDebut && actualTime.TimeOfDay < heureFin) {
                // Pausing program for 1 hour
                Thread.Sleep(3600000);
                actualTime = DateTime.UtcNow;
            }
        }

        #endregion
    }
}