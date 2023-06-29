using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using GMS.Sdk.Core.Models;

namespace GMS.Sdk.Core
{
    public class ToolBox
    {

        public class GoogleDate
        {

            public string? key;
            public string? value;
        }

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
        public static DateTime ComputeDateFromGoogleDate(string googleDate)
        {
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMS.Sdk.Core.ToolBox", "GoogleDate.json");
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
        /// Requires finding element by FindElementSafe(By).
        /// Checking if web element exist or not.
        /// </summary>
        /// <param name="element">Current element</param>
        /// <returns>Returns T/F depending on if element is defined or null.</returns>
        public static bool Exists(IWebElement? element)
        {
            if (element == null) { return false; }
            return true;
        }

        /// <summary>
        /// Requires finding elements by FindElementsSafe(By).
        /// Checking if web elements exist or not.
        /// </summary>
        /// <param name="elements">Current element</param>
        /// <returns>Returns T/F depending on if element is defined or null.</returns>
        public static bool Exists(ReadOnlyCollection<IWebElement>? elements)
        {
            if (elements == null) { return false; }
            return true;
        }

        /// <summary>
        /// Getting adress splitted thanks to https://adresse.data.gouv.fr/api-doc/adresse.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>addressResponse object if adress found or null.</returns>
        public static async Task<AddressResponse?> ApiCallForAddress(string address) {
            using HttpClient client = new();
            string apiUrl = $"https://api-adresse.data.gouv.fr/search/?q={address}&limit=1&autocomplete=0";
            string[] types = { "housenumber", "street", "locality", "municipality" };

            foreach (string type in types) {
                HttpResponseMessage response = await client.GetAsync(apiUrl + $"&type={type}");
                string responseBody = await response.Content.ReadAsStringAsync();
                AddressResponse? addressResponse = AddressResponse.FromJson(responseBody);

                if (addressResponse.Features.Length > 0) {
                    return addressResponse;
                }
            }

            return null;
        }

        #endregion
    }
}