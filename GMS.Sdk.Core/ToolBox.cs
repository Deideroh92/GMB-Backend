using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using GMS.Sdk.Core.Models;
using System.Security.Authentication;

namespace GMS.Sdk.Core
{
    public class ToolBox
    {

        #region classes
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

        /// <summary>
        /// Transform a google date into a real date.
        /// </summary>
        /// <param name="googleDate"></param>
        /// <returns>Real date from google date.</returns>
        public static DateTime ComputeDateFromGoogleDate(string? googleDate)
        {
            if (googleDate == null) return DateTime.UtcNow;

            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core\\Files", "GoogleDate.json");
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
                    return currentDate.AddDays(-jsonValue * 7);
                if (googleDate.Contains("jour"))
                    return currentDate.AddDays(-jsonValue);
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
        /// Breaking hours, when the program needs to stop.
        /// </summary>
        public static void BreakingHours() {
            DateTime actualTime = DateTime.Now;

            // Breaking hours
            TimeSpan heureDebut = new TimeSpan(22, 0, 0); // 22h00
            TimeSpan heureFin = new TimeSpan(1, 0, 0); // 3AM

            while (actualTime.TimeOfDay >= heureDebut && actualTime.TimeOfDay <= heureFin) {
                // Pausing program for 1 hour
                Thread.Sleep(3600000);
                actualTime = DateTime.Now;
            }
        }

        #endregion
    }
}