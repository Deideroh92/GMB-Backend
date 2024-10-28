using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Svg.Converter;
using QRCoder;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace GMB.Sdk.Core.FileGenerators.Sticker
{
    public enum StickerLanguage
    {
        EN,
        FR,
        IT,
        DE,
        ES
    }

    public interface IStickerGenerator
    {
        Task<byte[]> Generate(StickerLanguage language, double score, string qrCodeUrl, DateTime stickerDate);
    }

    public class StickerGenerator: IStickerGenerator
    {
        const string frPlaceStickerTemplateFilePath = "FileGenerators/Sticker/Resources/placeStickerTemplate_fr.pdf";
        const string enPlaceStickerTemplateFilePath = "FileGenerators/Sticker/Resources/placeStickerTemplate_en.pdf";
        const string itPlaceStickerTemplateFilePath = "FileGenerators/Sticker/Resources/placeStickerTemplate_it.pdf";
        const string esPlaceStickerTemplateFilePath = "FileGenerators/Sticker/Resources/placeStickerTemplate_es.pdf";
        const string dePlaceStickerTemplateFilePath = "FileGenerators/Sticker/Resources/placeStickerTemplate_de.pdf";
        const string ptPlaceStickerTemplateFilePath = "FileGenerators/Sticker/Resources/placeStickerTemplate_pt.pdf";
        const string poppinsBoldFontFilePath = "FileGenerators/Sticker/Resources/Poppins-Bold.ttf";
        const string poppinsMediumFontFilePath = "FileGenerators/Sticker/Resources/Poppins-Medium.ttf";

        const string scoreFieldId = "score";
        const string footerDateFieldId = "footerDate";
        const string footerCertifiedFieldId = "footerCertified";
        const string qrCodeFieldId = "qrCode_af_image";

        // TODO: add lazy on images and fonts => first check if there is need to do it when running generator in RabbitMQ Queue
        static readonly PdfFont poppinsBoldFont;
        static readonly PdfFont poppinsMediumFont;

        static readonly byte[] frPlaceStickerTemplateBytes;
        static readonly byte[] enPlaceStickerTemplateBytes;
        static readonly byte[] itPlaceStickerTemplateBytes;
        static readonly byte[] esPlaceStickerTemplateBytes;
        static readonly byte[] dePlaceStickerTemplateBytes;
        static readonly byte[] ptPlaceStickerTemplateBytes;

        static StickerGenerator()
        {
            frPlaceStickerTemplateBytes = File.ReadAllBytes(frPlaceStickerTemplateFilePath);
            enPlaceStickerTemplateBytes = File.ReadAllBytes(enPlaceStickerTemplateFilePath);
            itPlaceStickerTemplateBytes = File.ReadAllBytes(itPlaceStickerTemplateFilePath);
            esPlaceStickerTemplateBytes = File.ReadAllBytes(esPlaceStickerTemplateFilePath);
            dePlaceStickerTemplateBytes = File.ReadAllBytes(dePlaceStickerTemplateFilePath);
            ptPlaceStickerTemplateBytes = File.ReadAllBytes(ptPlaceStickerTemplateFilePath);

            poppinsBoldFont = PdfFontFactory.CreateFont(poppinsBoldFontFilePath);
            poppinsMediumFont = PdfFontFactory.CreateFont(poppinsMediumFontFilePath);
        }

        public async Task<byte[]> Generate(StickerLanguage language, double score, string qrCodeUrl, DateTime stickerDate)
        {
            using MemoryStream generatedPdfMemoryStream = new();
            using PdfReader pdfReader = new(new MemoryStream(GetTemplateByLanguage(language)));

            PdfDocument placeStickerDoc = new(pdfReader, new PdfWriter(generatedPdfMemoryStream));
            //Document document = new(placeStickerDoc);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(placeStickerDoc, true);

            // Score
            form.GetField(scoreFieldId).SetValue(GetFormattedScore(score, language)).SetFontAndSize(poppinsBoldFont, 72);

            // Footer date
            DateTime startStickerDate = stickerDate.AddMonths(-12);
            form.GetField(footerDateFieldId).SetValue(GetFooterDateText(language, startStickerDate, stickerDate)).SetFontAndSize(poppinsMediumFont, 10);

            // Footer certified
            form.GetField(footerCertifiedFieldId).SetValue(GetFooterCertifiedText(language)).SetFontAndSize(poppinsMediumFont, 10);

            // QR Code
            string qrCodeSvg = GenerateQrCodeSvg(qrCodeUrl);
            PdfFormField qrCodeField = form.GetField(qrCodeFieldId);
            iText.Kernel.Geom.Rectangle rect = qrCodeField.GetWidgets()[0].GetRectangle().ToRectangle();

            // Get the page where the field is located
            PdfPage page = qrCodeField.GetWidgets()[0].GetPage();

            // Convert SVG to PdfFormXObject
            PdfFormXObject svgObject;
            using (var svgStream = new MemoryStream(Encoding.UTF8.GetBytes(qrCodeSvg)))
            {
                svgObject = SvgConverter.ConvertToXObject(svgStream, placeStickerDoc);
            }

            // Calculate scaling factors to fit the QR code into the field rectangle
            float widthScale = rect.GetWidth() / svgObject.GetWidth();
            float heightScale = rect.GetHeight() / svgObject.GetHeight();

            // Use the smaller scale to maintain aspect ratio
            float scale = Math.Min(widthScale, heightScale);

            // Calculate new dimensions
            float scaledWidth = svgObject.GetWidth() * scale;
            float scaledHeight = svgObject.GetHeight() * scale;

            // Calculate positions to center the QR code in the field
            float xPosition = rect.GetLeft() + (rect.GetWidth() - scaledWidth) / 2;
            float yPosition = rect.GetBottom() + (rect.GetHeight() - scaledHeight) / 2;

            // Create a PdfCanvas
            PdfCanvas pdfCanvas = new (page);

            // Save the current graphics state
            pdfCanvas.SaveState();

            // Apply transformation for scaling and positioning
            pdfCanvas.ConcatMatrix(scale, 0, 0, scale, xPosition, yPosition);

            // Add the XObject to the canvas
            pdfCanvas.AddXObjectAt(svgObject, 0, 0);

            // Restore the graphics state
            pdfCanvas.RestoreState();

            form.RemoveField(qrCodeFieldId);

            // Flatten the form to burn fields in doc
            form.FlattenFields();

            // Close the PDF (this will write the content to the MemoryStream)
            //document.Close();
            placeStickerDoc.Close();

            return generatedPdfMemoryStream.ToArray();
        }

        static byte[] GetTemplateByLanguage(StickerLanguage language)
        {
            return language switch
            {
                StickerLanguage.FR => frPlaceStickerTemplateBytes,
                StickerLanguage.EN => enPlaceStickerTemplateBytes,
                StickerLanguage.IT => itPlaceStickerTemplateBytes,
                StickerLanguage.DE => dePlaceStickerTemplateBytes,
                StickerLanguage.ES => esPlaceStickerTemplateBytes,
                _ => throw new NotSupportedException()
            };
        }

        static CultureInfo GetCultureByLanguage(StickerLanguage language)
        {
            return language switch
            {
                // TODO: put thos values in a dictionary initialized at first ctor
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
    
        static string GenerateQrCodeSvg(string url)
        {
            const string darkColorHex = "#FFFFFF";
            // Generate QR code data
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            // Render QR code as svg
            SvgQRCode svgQRCode = new(qrCodeData);
            string svgContent = svgQRCode.GetGraphic(1, darkColorHex, "none", false); // Scale 1 because Svg don't need higher scale (vector)
            return MergeSvgRectanglesIntoPath(svgContent, darkColorHex);
        }

        static string MergeSvgRectanglesIntoPath(string svgContent, string colorHexToMatch)
        {
            // Regex to extract the rectangles with fill="#FFFFFF"
            Regex rectRegex = new ($"<rect x=\"(.*?)\" y=\"(.*?)\" width=\"(.*?)\" height=\"(.*?)\" fill=\"{colorHexToMatch}\" />", RegexOptions.IgnoreCase);
            MatchCollection matches = rectRegex.Matches(svgContent);

            StringBuilder pathData = new ();
            foreach (Match match in matches)
            {
                string x = match.Groups[1].Value;
                string y = match.Groups[2].Value;
                string width = match.Groups[3].Value;
                string height = match.Groups[4].Value;

                // Add rectangle to path data
                pathData.Append($"M{x},{y} h{width} v{height} h-{width} Z ");
            }

            // Extract the SVG header => maybe this will change over libaries versions
            Regex svgHeaderRegex = new ("(<svg[^>]+>)", RegexOptions.IgnoreCase);
            string svgHeader = svgHeaderRegex.Match(svgContent).Value;

            // Build new SVG content
            StringBuilder newSvgContent = new ();
            newSvgContent.Append(svgHeader);
            newSvgContent.Append($"<path fill=\"#FFFFFF\" d=\"{pathData.ToString()}\" />");
            newSvgContent.Append("</svg>");

            return newSvgContent.ToString();
        }
    }
}