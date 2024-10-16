﻿using GMB.Sdk.Core.Types.Api;
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
        /// Insert Api adress inside Business profile.
        /// </summary>
        /// <param name="business"></param>
        /// <param name="address"></param>
        /// <returns>Business with address updated</returns>
        public static DbBusinessProfile InsertApiAddressInBusiness(DbBusinessProfile business, AddressApiResponse address)
        {
            business.Lon = (address.Features[0]?.Geometry?.Coordinates[0]);
            business.Lat = (address.Features[0]?.Geometry?.Coordinates[1]);
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
        private static Bitmap GenerateQrCode(string url)
        {
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
            PngByteQRCode qrCode = new(qrCodeData);

            byte[] qrCodeBytes = qrCode.GetGraphic(5, [66, 134, 245], [0, 0, 0], true);

            // Convert byte array to Bitmap
            using MemoryStream ms = new(qrCodeBytes);
            return new Bitmap(ms);
        }

        public static Font LoadCustomFont(string fontPath, float fontSize)
        {
            PrivateFontCollection privateFontCollection = new();

            // Load the custom font
            privateFontCollection.AddFontFile(fontPath);

            // Return the custom font, make sure it's a valid font family
            return new Font(privateFontCollection.Families[0], fontSize);
        }

        // Function to create the final sticker image
        public static Bitmap CreateSticker(string score, string qrUrl, DateTime orderDate)
        {

            DateTime startingDate = orderDate.AddMonths(-12);

            // Load the sticker template (logo.jpg in your case)
            string stickerPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "logo.jpg");
            using Bitmap stickerImg = new(stickerPath);
            int stickerWidth = stickerImg.Width;
            int stickerHeight = stickerImg.Height;

            // Generate QR code
            Bitmap qrCodeImg = GenerateQrCode(qrUrl);

            // Setup fonts
            string fontPathBold = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Poppins-Bold.ttf");
            string fontPathMedium = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Poppins-Medium.ttf");
            Font font = LoadCustomFont(fontPathBold, 36f);
            Font font2 = LoadCustomFont(fontPathBold, 72f);
            Font font3 = LoadCustomFont(fontPathMedium, 10f);

            // Draw the text and QR code on the sticker image
            using (Graphics g = Graphics.FromImage(stickerImg))
            {
                g.DrawString($"Notre note\nGoogle", font, Brushes.White, new PointF(30, 30));
                g.DrawString(score, font2, Brushes.White, new PointF(285, 195));
                g.DrawString("Moyenne des notes relevées entre le 1er janvier 2023 et le 31 décembre 2023", font3, Brushes.Black, new PointF(40, 445));
                g.DrawString("Certifiée par Vasano | © Tous droits réservés", font3, Brushes.Black, new PointF(132, 458));

                // Paste QR code onto sticker
                g.DrawImage(qrCodeImg, new Point(55, 210));
            }

            // Resize the sticker image if needed
            Bitmap resizedStickerImg = new(stickerImg, stickerWidth, stickerHeight);

            return resizedStickerImg;
        }

        /// <summary>
        /// Craete Certificate
        /// </summary>
        /// <param name="score1"></param>
        /// <param name="score2"></param>
        /// <param name="score3"></param>
        /// <param name="score4"></param>
        /// <param name="score5"></param>
        /// <param name="averageScore"></param>
        /// <param name="name"></param>
        /// <returns>certificate</returns>
        public static Bitmap CreateCertificate(int score1, int score2, int score3, int score4, int score5, double averageScore, string name, DateTime orderDate)
        {

            // Define image size
            int width = 400;
            int height = 200;

            // Create a new Bitmap
            Bitmap bitmap = new(width, height);

            // Create a Graphics object to draw on the bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Set the background color to white
                g.Clear(Color.White);

                // Set the font and brush for drawing text
                Font font = new Font("Arial", 24, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.Black);

                // Draw the average score in the middle of the image
                string text = $"Average Score: {averageScore.ToString()}";
                SizeF textSize = g.MeasureString(text, font);

                // Center the text
                float textX = (width - textSize.Width) / 2;
                float textY = (height - textSize.Height) / 2;

                g.DrawString(text, font, brush, new PointF(textX, textY));
            }

            return bitmap;
        }

        /// <summary>
        /// Convert bitmap to byte array
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>byte array</returns>
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using MemoryStream memoryStream = new();
            // Save the bitmap to the memory stream in a specific format (e.g., PNG)
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            // Return the byte array from the memory stream
            return memoryStream.ToArray();
        }
        #endregion
    }
}