using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace GMS.Sdk.Core.ToolBox {
    public class ToolBox {

        #region Local

        /// <summary>
        /// Encode a string.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>The input string encoded.</returns>
        public static string ComputeMd5Hash(string message) {
            using MD5 md5 = MD5.Create();
            byte[] input = Encoding.Default.GetBytes(message.ToUpper());
            byte[] hash = md5.ComputeHash(input);

            StringBuilder sb = new();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        /// <summary>
        /// Transform a google date into a real date.
        /// </summary>
        /// <param name="googleDate"></param>
        /// <returns>Date from google date.</returns>
        public static DateTime ComputeDateFromGoogleDate(string googleDate) {
            DateTime date;
            int jsonValue = 0;

            // Getting value from key googleDate in our Json file.
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            using StreamReader r = new(path + @"\GMS.Sdk.Core\ToolBox\GoogleDate.json");
            string json = r.ReadToEnd();

            if (jsonValue == 0)
                return DateTime.UtcNow;

            // Computing date.
            if (googleDate.Contains("moi"))
                date =  DateTime.UtcNow.AddMonths(-(jsonValue));
            else if (googleDate.Contains("an"))
                date =  DateTime.UtcNow.AddYears(-(jsonValue));
                else
                    date = DateTime.UtcNow.AddDays(-(jsonValue));

            return date;
        }

        /// <summary>
        /// Same as FindElement only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <param name="by2">The second search string (if any) for finding element</param>
        /// <returns>Returns element or null if not found</returns>
        public static IWebElement? FindElementSafe(IWebDriver driver, By by, By? by2 = null) {
            try {
                return driver.FindElement(by);
            } catch (NoSuchElementException) {
                if (by2 != null) {
                    try {
                        return driver.FindElement(by2);
                    } catch (NoSuchElementException) {
                        return null;
                    }
                }
                return null;
            }
        }
        
        /// <summary>
        /// Same as FindElement only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <param name="by2">The second search string (if any) for finding element</param>
        /// <returns>Returns element or null if not found</returns>
        public static IWebElement? FindElementSafe(IWebElement webElement, By by, By? by2 = null) {
            try {
                return webElement.FindElement(by);
            } catch (NoSuchElementException) {
                if (by2 != null) {
                    try {
                        return webElement.FindElement(by2);
                    } catch (NoSuchElementException) {
                        return null;
                    }
                }
                return null;
            }
        }
        
        /// <summary>
        /// Same as FindElements only returns null when not found instead of an exception.
        /// </summary>
        /// <param name="by">The search string for finding element</param>
        /// <param name="by2">The second search string (if any) for finding element</param>
        /// <returns>Returns elements or null if not found</returns>
        public static ReadOnlyCollection<IWebElement>? FindElementsSafe(IWebDriver driver, By by, By? by2 = null) {
            try {
                return driver.FindElements(by);
            } catch (NoSuchElementException) {
                if (by2 != null) {
                    try {
                        return driver.FindElements(by2);
                    } catch (NoSuchElementException) {
                        return null;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Requires finding element by FindElementSafe(By).
        /// Checking if web element exist or not.
        /// </summary>
        /// <param name="element">Current element</param>
        /// <returns>Returns T/F depending on if element is defined or null.</returns>
        public static bool Exists(IWebElement? element) {
            if (element == null) { return false; }
            return true;
        }

        /// <summary>
        /// Requires finding elements by FindElementsSafe(By).
        /// Checking if web elements exist or not.
        /// </summary>
        /// <param name="elements">Current element</param>
        /// <returns>Returns T/F depending on if element is defined or null.</returns>
        public static bool Exists(ReadOnlyCollection<IWebElement>? elements) {
            if (elements == null) { return false; }
            return true;
        }
        #endregion
    }
}