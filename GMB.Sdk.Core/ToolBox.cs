using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Drawing;
using QRCoder;
using Serilog;
using System.Drawing.Text;
using AngleSharp.Io;

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
            byte[] input = Encoding.Default.GetBytes(message.ToUpper());
            byte[] hash = MD5.HashData(input);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static void SendEmail(string? message, string? subject)
        {
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Change this to your SMTP server's port
            string fromEmail = "maximiliend1998@gmail.com";
            string toEmail = "m.david@vasano.fr";
            string toEmail2 = "jm.chabrol@vasano.fr";
            string toEmail3 = "e.delatte@vasano.fr";
            string toEmail4 = "dsi@vasano.fr";
            string body = "Test Result as of " + DateTime.UtcNow.ToString() + " : " + message;

            // Create a new SmtpClient and set the SMTP server details
            SmtpClient smtpClient = new(smtpServer)
            {
                Port = smtpPort,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, "iigz pyyn ngsp wjqq"), // Replace with your email password
                EnableSsl = true // Enable SSL if your SMTP server requires it
            };

            // Create a new MailMessage object with the necessary details
            MailMessage mailMessage = new(fromEmail, toEmail, subject, body);
            MailMessage mailMessage2 = new(fromEmail, toEmail2, subject, body);
            MailMessage mailMessage3 = new(fromEmail, toEmail3, subject, body);
            MailMessage mailMessage4 = new(fromEmail, toEmail4, subject, body);
            mailMessage.IsBodyHtml = false; // Set to true if your email body is in HTML format

            smtpClient.Send(mailMessage);
            smtpClient.Send(mailMessage2);
            smtpClient.Send(mailMessage3);
            smtpClient.Send(mailMessage4);
        }

    public static void SendEmailVasanoIO(string username, int orderId, string subject, string email)
    {
        string smtpServer = "ssl0.ovh.net";
        int smtpPort = 587;
        string fromEmail = "no-reply@vasano.io";
        string password = "SthKHRdbyQ0c9nH";
        string[] toEmails = { email, "admin@vasano.io" };

        string body = $@"
        <!DOCTYPE html>
        <html>
            <head>
            <style>
                /* Define styles here */
            </style>
            </head>
            <body style='background-color: white; font-family: Arial, sans-serif;'>
            <div style='border: 1px solid #eaeaea; border-radius: 8px; margin: 40px auto; padding: 20px; max-width: 465px;'>
                <div style='margin-top: 32px; text-align: center;'>
                <a href='https://vasano.io'>
                    <img
                    src='https://vasano.io/images/logo/logo_Vasano.webp'
                    alt='Vasano Solutions'
                    style='display: block; margin: 0 auto; width: 50%; height: 50%;'
                    />
                </a>
                </div>
                <hr style='border: 1px solid #eaeaea; margin: 26px 0; width: 100%;' />
                <p style='color: black; font-size: 14px; line-height: 24px;'>Hello {username},</p>
                <p style='color: black; font-size: 14px; line-height: 24px;'>
                Your STICKERS are ready for selection for the order #
                <strong>{orderId}</strong>.
                </p>
                <p style='color: black; font-size: 14px; line-height: 24px;'>
                The next step for you is to select which STICKERS you are interested
                in, and continue your order!
                </p>
                <div style='text-align: center; margin: 32px 0;'>
                <a href='https://vasano.io/orders/{orderId}' style='background-color: #007ee6; color: white; font-size: 12px; font-weight: 600; text-decoration: none; padding: 10px 20px; border-radius: 4px; display: inline-block;'>
                    Select your STICKERS
                </a>
                </div>
                <p style='color: black; font-size: 14px; line-height: 24px;'>
                or copy and paste this URL into your browser:
                <a href='https://vasano.io/orders/{orderId}' style='color: #007ee6; text-decoration: none;'>https://vasano.io/orders/{orderId}</a>
                </p>
                <hr style='border: 1px solid #eaeaea; margin: 26px 0; width: 100%;' />
                <p style='color: #666666; font-size: 12px; line-height: 24px;'>
                This email was sent because you successfully purchased our STICKERS
                on Vasano.io. If you were not expecting this invitation, contact
                immediately <a href='mailto:contact@vasano.io' style='color: #007ee6;'>contact@vasano.io</a>.
                </p>
            </div>
            <div style='max-width: 580px; margin: 0 auto; text-align: center; color: #706a7b; font-size: 12px;'>
                <a href='https://www.linkedin.com/company/vasano-solutions/' target='_blank' style='color: #007ee6; text-decoration: none;'>LinkedIn</a>
                <p>© 2024 Vasano Solutions, All Rights Reserved</p>
                <p>10 Rue de la Paix, 75002 Paris - FRANCE</p>
            </div>
            </body>
        </html>";

        // Configure SMTP client
        SmtpClient smtpClient = new(smtpServer)
        {
            Port = smtpPort,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        // Create and send the email to each recipient
        foreach (string toEmail in toEmails)
        {
            MailMessage mailMessage = new(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            smtpClient.Send(mailMessage);
        }
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
        /// Compute Date with provided visit date.
        /// </summary>
        /// <param name="visitDate"></param>
        /// <returns>Date</returns>
        public static DateTime ComputeDateFromVisitDate(string visitDate)
        {
            string date = visitDate.Replace("Visité en", "").Trim();
            string[] parts = date.Trim().Split(' ');

            string month = parts[0].Trim();
            string yearString = parts.Length > 1 ? parts[1].Trim() : "";

            if (!int.TryParse(yearString, out int year))
            {
                // If year is not specified, use current year
                year = DateTime.Now.Year;
            }
            DateTime firstDayOfMonth = new(year, DateTime.ParseExact(month, "MMMM", new CultureInfo("fr-FR")).Month, 1);
            return firstDayOfMonth;
        }

        /// <summary>
        /// Transform a google date into a real date.
        /// </summary>
        /// <param name="googleDate"></param>
        /// <returns>Real date from google date.</returns>
        public static DateTime ComputeDateFromGoogleDate(string? googleDate)
        {
            if (googleDate == null)
                return DateTime.UtcNow;

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "GoogleDate.json");

            string json = File.ReadAllText(filePath);
            Dictionary<string, string>? mapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (mapper.TryGetValue(googleDate, out string? value) && int.TryParse(value, out int jsonValue))
            {
                DateTime currentDate = DateTime.UtcNow;


                if (googleDate.Contains("moi"))
                {
                    return currentDate.AddMonths(-jsonValue);
                }
                if (googleDate.Contains("an"))
                {
                    return currentDate.AddYears(-jsonValue);
                }
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
        /// Transform Place class to Business Profile Class
        /// </summary>
        /// <param name="place"></param>
        /// <returns>Business Profile</returns>
        public static DbBusinessProfile? PlaceToBP(Place place)
        {
            try
            {
                DbBusinessProfile? profile = new(
                place.PlaceId,
                ComputeMd5Hash(place.DisplayName?.Text + place.FormattedAddress),
                Guid.NewGuid().ToString("N"),
                place.DisplayName?.Text,
                null,
                place.FormattedAddress,
                place.ShortFormattedAddress,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("postal_code") == true)?.LongText,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("locality") == true)?.LongText,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("postal_code") == true)?.LongText,
                place.Location.Latitude,
                place.Location.Longitude,
                null,
                null,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("street_number") == true)?.LongText,
                null,
                place.NationalPhoneNumber,
                place.WebsiteUri,
                place.PlusCode?.GlobalCode,
                null,
                (BusinessStatus)Enum.Parse(typeof(BusinessStatus), place.BusinessStatus!),
                null,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("country") == true)?.LongText,
                place.GoogleMapsUri,
                place.Location?.Latitude + " , " + place.Location?.Longitude,
                0,
                null,
                place.InternationalPhoneNumber
                );
                return profile;
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Transform Place class to Business Class
        /// </summary>
        /// <param name="place"></param>
        /// <returns>Business</returns>
        public static Business? PlaceToB(Place place)
        {
            try
            {
                Business? business = new(
                place.PlaceId,
                ComputeMd5Hash(place.DisplayName?.Text + place.FormattedAddress),
                Guid.NewGuid().ToString("N"),
                place.DisplayName?.Text,
                null,
                place.FormattedAddress,
                place.ShortFormattedAddress,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("postal_code") == true)?.LongText,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("locality") == true)?.LongText,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("postal_code") == true)?.LongText,
                place.Location.Latitude,
                place.Location.Longitude,
                null,
                null,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("street_number") == true)?.LongText,
                null,
                place.NationalPhoneNumber,
                place.WebsiteUri,
                place.PlusCode?.GlobalCode,
                null,
                (BusinessStatus)Enum.Parse(typeof(BusinessStatus), place.BusinessStatus!),
                null,
                place.AddressComponents?.FirstOrDefault(x => x?.Types?.Contains("country") == true)?.LongText,
                place.GoogleMapsUri,
                0,
                place.Location?.Latitude + " , " + place.Location?.Longitude,
                null,
                place.InternationalPhoneNumber,
                place.Rating,
                place.UserRatingCount,
                null
                );
                return business;
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
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
                } catch (NoSuchElementException)
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
                } catch (NoSuchElementException)
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
                } catch (NoSuchElementException)
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
                } catch (NoSuchElementException)
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
        public static bool Exists<T>(T elements)
        {
            return elements != null;
        }

        /// <summary>
        /// Getting adress splitted thanks to https://adresse.data.gouv.fr/api-doc/adresse.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>addressResponse object if adress found or null.</returns>
        public static async Task<AddressApiResponse?> ApiCallForAddress(string address)
        {
            using HttpClientHandler handler = new();
            handler.SslProtocols = SslProtocols.Tls12;
            using HttpClient client = new(handler);

            string apiUrl = $"https://api-adresse.data.gouv.fr/search/?q={Uri.EscapeDataString(address)}";
            string[] types = ["housenumber", "street", "locality", "municipality"];
            foreach (string type in types)
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl + $"&type={type}");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    AddressApiResponse? addressResponse = AddressApiResponse.FromJson(responseBody);

                    if (addressResponse?.Features?.Length > 0)
                    {
                        return addressResponse;
                    }
                } catch (Exception)
                {
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Breaking hours, when the program needs to pause.
        /// </summary>
        public static void BreakingHours()
        {
            DateTime actualTime = DateTime.UtcNow;

            // Breaking hours
            TimeSpan heureDebut = new(1, 0, 0); // 1AM
            TimeSpan heureFin = new(3, 0, 0); // 3AM

            while (actualTime.TimeOfDay >= heureDebut && actualTime.TimeOfDay < heureFin)
            {
                // Pausing program for 1 hour
                Thread.Sleep(3600000);
                actualTime = DateTime.UtcNow;
            }
        }

        // Function to generate the QR code
        private static Bitmap GenerateQrCode(string url, int pixelsPerModule, int width, int height)
        {
            // Initialize the QR code generator
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);

            // Create the QR code with specified pixels per module and colors
            PngByteQRCode qrCode = new(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(pixelsPerModule, Color.White, Color.FromArgb(15, 157, 87), false);

            // Convert byte array to Bitmap
            using MemoryStream ms = new(qrCodeBytes);
            Bitmap qrBitmap = new Bitmap(ms);

            // Set the desired resolution (e.g., 300 DPI for print quality)
            qrBitmap.SetResolution(300, 300);  // You can adjust the DPI here if needed

            // Resize to 4K resolution (3840 x 2160)
            Bitmap highResQrBitmap = new Bitmap(qrBitmap, new Size(width, height));

            return highResQrBitmap;
        }

        // Function to create the final sticker image
        public static Bitmap CreateQrCode(string qrUrl)
        {
            int pixelsPerModule = 20; // Adjust this for a finer or coarser grid
            int width = 5167; // 4K width
            int height = 5167;

            // Generate QR code
            Bitmap qrCodeImg = GenerateQrCode(qrUrl, pixelsPerModule, width, height);

            return qrCodeImg;
        }
        #endregion
    }
}