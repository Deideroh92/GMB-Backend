using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using QRCoder;
using System.Globalization;
using System.Text;

namespace GMB.Sdk.Core.StickerImageGenerator
{
    // TODO: Differ en-US from en-GB ? Yup we should => changes so words in text (score => mark) and date format, for now use en-GB culture
    public enum StickerLanguage
    {
        EN,
        FR,
        IT,
        DE,
        ES
    }

    public enum FontTypes
    {
        PoppinsBold,
        PoppinsMedium
    }

    public interface IStickerImageGenerator
    {
        Task<byte[]> Generate(StickerLanguage language, double score, string qrCodeUrl, DateTime stickerDate);
    }

    public class StickerImageGenerator
    {
        const string baseImageFilePath = "StickerImageGenerator/baseImage.jpg";
        const string poppinsBoldFontFilePath = "StickerImageGenerator/Poppins-Bold.ttf";
        const string poppinsMediumFontFilePath = "StickerImageGenerator/Poppins-Medium.ttf";
        const int mainTextMaxWidth = 196;
        const int qrCodeSize = 145;

        static readonly string[] linesSeparators = ["\r\n", "\n"];
        static readonly PointF mainTextPosition = new(30, 30);
        static readonly PointF scoreTextCenterPosition = new(346, 249);
        static readonly PointF footerDateTextCenterPosition = new(246, 452);
        static readonly PointF footerCertifiedTextCenterPosition = new(246, 465);
        static readonly Point qrCodePosition = new(55, 232);

        // TODO: add lazy on images and fonts => first check if there is need to do it when running generator in RabbitMQ Queue

        static readonly Image<Rgba32> _image;

        static readonly Font mainTextFont;
        static readonly Font scoreFont;
        static readonly Font footerFont;

        static StickerImageGenerator()
        {
            _image = Image.Load<Rgba32>(baseImageFilePath);

            FontCollection fontCollection = new();
            FontFamily boldFontFamily = fontCollection.Add(poppinsBoldFontFilePath);
            FontFamily mediumFontFamily = fontCollection.Add(poppinsMediumFontFilePath);

            mainTextFont = boldFontFamily.CreateFont(36, FontStyle.Bold);
            scoreFont = boldFontFamily.CreateFont(72, FontStyle.Bold);
            footerFont = mediumFontFamily.CreateFont(10, FontStyle.Regular);
        }

        public async Task<byte[]> Generate(StickerLanguage language, double score, string qrCodeUrl, DateTime stickerDate)
        {
            using Image<Rgba32> image = _image.Clone();

            AddMainText(image, language);
            AddScoreText(image, score, language);
            AddFooterText(image, language, stickerDate);
            AddQrCode(image, qrCodeUrl);

            using MemoryStream stream = new ();
            await image.SaveAsPngAsync(stream);
            
            return stream.ToArray();
        }

        static void AddMainText(Image<Rgba32> image, StickerLanguage language)
        {
            string text = GetMainTextByLanguage(language);
            string wrappedText = WrapText(text, mainTextFont, mainTextMaxWidth);
            Font adjustedFont = AdjustFontSizeToFit(wrappedText, mainTextFont, mainTextMaxWidth);
            image.Mutate(ctx => ctx.DrawText(wrappedText, adjustedFont, Color.White, mainTextPosition));
        }

        static void AddScoreText(Image<Rgba32> image, double score, StickerLanguage language)
        {
            string text = GetFormattedScore(score, language);
            PointF position = GetTextPositionFromCenter(scoreTextCenterPosition, text, scoreFont);
            image.Mutate(ctx => ctx.DrawText(text, scoreFont, Color.White, position));
        }

        static void AddFooterText(Image<Rgba32> image, StickerLanguage language, DateTime stickerDate)
        {
            DateTime startStickerDate = stickerDate.AddMonths(-12);
            string footerDateText = GetFooterDateText(language, startStickerDate, stickerDate);
            PointF footerDateTextPosition = GetTextPositionFromCenter(footerDateTextCenterPosition, footerDateText, footerFont);
            image.Mutate(ctx => ctx.DrawText(footerDateText, footerFont, Color.Black, footerDateTextPosition));

            string footerCertifiedText = GetFooterCertifiedText(language);
            PointF footerCertifiedTextPosition = GetTextPositionFromCenter(footerCertifiedTextCenterPosition, footerCertifiedText, footerFont);
            image.Mutate(ctx => ctx.DrawText(footerCertifiedText, footerFont, Color.Black, footerCertifiedTextPosition));
        }

        static void AddQrCode(Image<Rgba32> image, string qrCodeUrl) {
            Image<Rgba32> qrCodeImage = GenerateQrCode(qrCodeUrl);
            image.Mutate(ctx => ctx.DrawImage(qrCodeImage, qrCodePosition, 1));
        }

        static string GetMainTextByLanguage(StickerLanguage language)
        {
            return language switch
            {
                StickerLanguage.EN => "Our annual Google rating",
                StickerLanguage.FR => "Notre note Google annuelle",
                StickerLanguage.IT => "La nostra valutazione annuale su Google",
                StickerLanguage.DE => "Unsere jährliche Google Bewertung", // Unsere jährliche Google-Bewertung | Unser jährliches Google Rating
                StickerLanguage.ES => "Nuestra clasificación anual de Google",
                _ => throw new NotImplementedException(),
            };
        }

        static CultureInfo GetCultureByLanguage(StickerLanguage language)
        {
            return language switch
            {
                StickerLanguage.FR => new CultureInfo("fr-FR"),
                StickerLanguage.EN => new CultureInfo("en-GB"),
                StickerLanguage.IT => new CultureInfo("it-IT"),
                StickerLanguage.DE => new CultureInfo("de-DE"),
                StickerLanguage.ES => new CultureInfo("es-ES"),
                _ => CultureInfo.InvariantCulture
            };
        }

        static string GetFooterDateText(StickerLanguage language, DateTime startDate, DateTime endDate)
        {
            CultureInfo culture = GetCultureByLanguage(language);

            string startDateString = DateOnly.FromDateTime(startDate).ToString(culture);
            string endDateString = DateOnly.FromDateTime(endDate).ToString(culture);

            return language switch
            {
                StickerLanguage.EN => $"Average score between {startDateString} and {endDateString}",
                StickerLanguage.FR => $"Moyenne des notes entre le {startDateString} et le {endDateString}",
                StickerLanguage.IT => $"Punteggio medio tra il {startDateString} e il {endDateString}",
                StickerLanguage.DE => $"Durchschnitt der Noten zwischen dem {startDateString} und dem {endDateString}",
                StickerLanguage.ES => $"Puntuación media entre el {startDateString} y el {endDateString}",
                _ => throw new NotImplementedException()
            };
        }

        static string GetFooterCertifiedText(StickerLanguage language)
        {
            return language switch
            {
                StickerLanguage.EN => "Certified by Vasano | © All rights reserved",
                StickerLanguage.FR => "Certifié par Vasano | © Tous droits réservés",
                StickerLanguage.IT => "Certificato da Vasano | © Tutti i diritti riservati",
                StickerLanguage.DE => "Zertifiziert von Vasano | © Alle Rechte vorbehalten",
                StickerLanguage.ES => "Certificado por Vasano | © Todos los derechos reservados",
                _ => throw new NotImplementedException()
            };
        }

        static string GetFormattedScore(double score, StickerLanguage language)
        {
            return score.ToString("0.0", GetCultureByLanguage(language));
        }

        static string WrapText(string text, Font font, float maxWidth)
        {
            StringBuilder wrappedText = new();
            StringBuilder currentLine = new();

            string[] words = text.Split(' ');

            foreach (string word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                FontRectangle testLineSize = TextMeasurer.MeasureSize(testLine, new TextOptions(font));

                if (testLineSize.Width > maxWidth)
                {
                    // Wrap the current line and start a new one
                    wrappedText.AppendLine(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
                else
                {
                    // Add the word to the current line
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(' ');
                    }
                    currentLine.Append(word);
                }
            }

            // Append any remaining text
            if (currentLine.Length > 0)
            {
                wrappedText.Append(currentLine);
            }

            return wrappedText.ToString();
        }

        static Font AdjustFontSizeToFit(string text, Font font, float maxWidth)
        {
            Font adjustedFont = font;
            string[] lines = text.Split(linesSeparators, StringSplitOptions.None);

            bool textExceedsMaxWidth = true; // Init as true to get in while

            while (textExceedsMaxWidth)
            {
                textExceedsMaxWidth = false; // Set to false to test all lines

                foreach (string line in lines)
                {
                    // If any line exceeds maxWidth, reduce the font size and check again
                    if (TextMeasurer.MeasureSize(line, new TextOptions(adjustedFont)).Width > maxWidth) {
                        adjustedFont = new Font(font.Family, adjustedFont.Size - 1);
                        textExceedsMaxWidth = true;
                        break; // No need to check other lines
                    }
                }
            }

            return adjustedFont;
        }

        static PointF GetTextPositionFromCenter(PointF centerPosition, string text, Font textFont)
        {
            FontRectangle textSize = TextMeasurer.MeasureSize(text, new TextOptions (textFont));
            return new PointF(centerPosition.X - (textSize.Width / 2), centerPosition.Y - (textSize.Height / 2));
        }
    
        static Image<Rgba32> GenerateQrCode(string url)
        {
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new(qrCodeData);

            byte[] qrBytes = qrCode.GetGraphic(20, System.Drawing.Color.White, System.Drawing.Color.Transparent, false); // Scale 20 for high quality
            Image<Rgba32> qrImage = Image.Load<Rgba32>(qrBytes);

            qrImage.Mutate(ctx => ctx.Resize(qrCodeSize, qrCodeSize)); // Resize to specified size
            return qrImage;
        }
    }
}
