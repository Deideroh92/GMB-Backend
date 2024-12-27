using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Svg.Converter;
using QRCoder;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GMB.Sdk.Core.FileGenerators.Sticker
{
    public enum StickerLanguage
    {
        EN,
        FR,
        IT,
        DE,
        ES,
        PT
    }

    public interface IStickerGenerator
    {
        byte[] Generate(StickerLanguage language, double score, string qrCodeUrl, DateTime stickerDate);
    }

    public class StickerGenerator : IStickerGenerator
    {
        const string PoppinsBoldFontFilePath = "FileGenerators/Sticker/Resources/Poppins-Bold.ttf";
        const string PoppinsMediumFontFilePath = "FileGenerators/Sticker/Resources/Poppins-Medium.ttf";

        const string ScoreFieldId = "score";
        const string FooterDateFieldId = "footerDate";
        const string FooterCertifiedFieldId = "footerCertified";
        const string QrCodeFieldId = "qrCode_af_image";

        static readonly Lazy<FontProgram> PoppinsBoldFontProgram = new(() => FontProgramFactory.CreateFont(PoppinsBoldFontFilePath));
        static readonly Lazy<FontProgram> PoppinsMediumFontProgram = new(() => FontProgramFactory.CreateFont(PoppinsMediumFontFilePath));

        static readonly Dictionary<StickerLanguage, string> TemplateFilePaths = new()
        {
            { StickerLanguage.FR, "FileGenerators/Sticker/Resources/placeStickerTemplate_fr.pdf" },
            { StickerLanguage.EN, "FileGenerators/Sticker/Resources/placeStickerTemplate_en.pdf" },
            { StickerLanguage.IT, "FileGenerators/Sticker/Resources/placeStickerTemplate_it.pdf" },
            { StickerLanguage.DE, "FileGenerators/Sticker/Resources/placeStickerTemplate_de.pdf" },
            { StickerLanguage.ES, "FileGenerators/Sticker/Resources/placeStickerTemplate_es.pdf" },
            { StickerLanguage.PT, "FileGenerators/Sticker/Resources/placeStickerTemplate_pt.pdf" },
        };

        static readonly Dictionary<StickerLanguage, Lazy<byte[]>> TemplateBytes = [];
        static readonly Dictionary<StickerLanguage, CultureInfo> Cultures = new()
        {
            { StickerLanguage.FR, new CultureInfo("fr-FR") },
            { StickerLanguage.EN, new CultureInfo("en-GB") },
            { StickerLanguage.IT, new CultureInfo("it-IT") },
            { StickerLanguage.DE, new CultureInfo("de-DE") },
            { StickerLanguage.ES, new CultureInfo("es-ES") },
            { StickerLanguage.PT, new CultureInfo("pt-PT") },
        };

        static StickerGenerator()
        {
            foreach (StickerLanguage language in TemplateFilePaths.Keys)
            {
                TemplateBytes[language] = new Lazy<byte[]>(() => File.ReadAllBytes(TemplateFilePaths[language]));
            }
        }

        public byte[] Generate(StickerLanguage language, double score, string qrCodeUrl, DateTime stickerDate)
        {
            using MemoryStream generatedPdfMemoryStream = new ();
            using PdfReader pdfReader = new (new MemoryStream(GetTemplateByLanguage(language)));

            using PdfDocument pdfDocument = new (pdfReader, new PdfWriter(generatedPdfMemoryStream));

            // Create PdfFont instances for this document using the cached FontProgram
            PdfFont poppinsBoldFont = PdfFontFactory.CreateFont(PoppinsBoldFontProgram.Value, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
            PdfFont poppinsMediumFont = PdfFontFactory.CreateFont(PoppinsMediumFontProgram.Value, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, true);

            FillFormFields(form, language, score, stickerDate, poppinsBoldFont, poppinsMediumFont);

            InsertQrCode(form, pdfDocument, qrCodeUrl);

            // Flatten the form to burn fields into the document
            form.FlattenFields();

            pdfDocument.Close();

            return generatedPdfMemoryStream.ToArray();
        }

        static void FillFormFields(PdfAcroForm form, StickerLanguage language, double score, DateTime stickerDate, PdfFont poppinsBoldFont, PdfFont poppinsMediumFont)
        {
            // Score
            form.GetField(ScoreFieldId)
                .SetValue(GetFormattedScore(score, language))
                .SetFontAndSize(poppinsBoldFont, 72);

            // Footer date
            DateTime startStickerDate = stickerDate.AddMonths(-12);
            form.GetField(FooterDateFieldId)
                .SetValue(GetFooterDateText(language, startStickerDate, stickerDate))
                .SetFontAndSize(poppinsMediumFont, 10);

            // Footer certified
            form.GetField(FooterCertifiedFieldId)
                .SetValue(GetFooterCertifiedText(language))
                .SetFontAndSize(poppinsMediumFont, 10);
        }

        static void InsertQrCode(PdfAcroForm form, PdfDocument pdfDocument, string qrCodeUrl)
        {
            // Generate QR Code SVG
            string qrCodeSvg = GenerateQrCodeSvg(qrCodeUrl);

            PdfFormField qrCodeField = form.GetField(QrCodeFieldId);
            Rectangle rect = qrCodeField.GetWidgets()[0].GetRectangle().ToRectangle();

            PdfPage page = qrCodeField.GetWidgets()[0].GetPage();

            // Convert SVG to PdfFormXObject
            PdfFormXObject svgObject;
            using (MemoryStream svgStream = new (Encoding.UTF8.GetBytes(qrCodeSvg)))
            {
                svgObject = SvgConverter.ConvertToXObject(svgStream, pdfDocument);
            }

            // Calculate scaling and positioning
            float scale = CalculateScale(rect, svgObject);
            (float xPosition, float yPosition) = CalculatePosition(rect, svgObject, scale);

            // Draw the QR code on the canvas
            PdfCanvas pdfCanvas = new (page);
            pdfCanvas.SaveState();
            pdfCanvas.ConcatMatrix(scale, 0, 0, scale, xPosition, yPosition);
            pdfCanvas.AddXObjectAt(svgObject, 0, 0);
            pdfCanvas.RestoreState();

            form.RemoveField(QrCodeFieldId);
        }

        static float CalculateScale(Rectangle rect, PdfFormXObject svgObject)
        {
            float widthScale = rect.GetWidth() / svgObject.GetWidth();
            float heightScale = rect.GetHeight() / svgObject.GetHeight();
            return Math.Min(widthScale, heightScale);
        }

        static (float xPosition, float yPosition) CalculatePosition(Rectangle rect, PdfFormXObject svgObject, float scale)
        {
            float scaledWidth = svgObject.GetWidth() * scale;
            float scaledHeight = svgObject.GetHeight() * scale;

            float xPosition = rect.GetLeft() + (rect.GetWidth() - scaledWidth) / 2;
            float yPosition = rect.GetBottom() + (rect.GetHeight() - scaledHeight) / 2;

            return (xPosition, yPosition);
        }

        static byte[] GetTemplateByLanguage(StickerLanguage language)
        {
            if (TemplateBytes.TryGetValue(language, out Lazy<byte[]>? lazyBytes))
            {
                return lazyBytes.Value;
            }
            throw new NotSupportedException($"Template for language '{language}' is not supported.");
        }

        static CultureInfo GetCultureByLanguage(StickerLanguage language)
        {
            if (Cultures.TryGetValue(language, out CultureInfo? culture))
            {
                return culture;
            }
            return CultureInfo.InvariantCulture;
        }

        static string GetFooterDateText(StickerLanguage language, DateTime startDate, DateTime endDate)
        {
            CultureInfo culture = GetCultureByLanguage(language);

            string startDateString = startDate.ToString("d", culture);
            string endDateString = endDate.ToString("d", culture);

            return language switch
            {
                StickerLanguage.EN => $"Average score between {startDateString} and {endDateString}",
                StickerLanguage.FR => $"Moyenne des notes entre le {startDateString} et le {endDateString}",
                StickerLanguage.IT => $"Punteggio medio tra il {startDateString} e il {endDateString}",
                StickerLanguage.DE => $"Durchschnitt der Noten zwischen dem {startDateString} und dem {endDateString}",
                StickerLanguage.ES => $"Puntuación media entre el {startDateString} y el {endDateString}",
                StickerLanguage.PT => $"Pontuação média entre {startDateString} e {endDateString}",
                _ => throw new NotImplementedException()
            };
        }

        static string GetFooterCertifiedText(StickerLanguage language)
        {
            return language switch
            {
                StickerLanguage.EN => "Certified by Vasano | © All rights reserved",
                StickerLanguage.FR => "Certifiée par Vasano | © Tous droits réservés",
                StickerLanguage.IT => "Certificato da Vasano | © Tutti i diritti riservati",
                StickerLanguage.DE => "Zertifiziert von Vasano | © Alle Rechte vorbehalten",
                StickerLanguage.ES => "Certificado por Vasano | © Todos los derechos reservados",
                StickerLanguage.PT => "Certificado por Vasano | © Todos os direitos reservados",
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
            using QRCodeGenerator qrGenerator = new ();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            // Render QR code as SVG
            SvgQRCode svgQRCode = new (qrCodeData);
            string svgContent = svgQRCode.GetGraphic(1, darkColorHex, "none", false); // Scale 1 because SVG doesn't need higher scale (vector)
            return MergeSvgRectanglesIntoPath(svgContent, darkColorHex);
        }

        static string MergeSvgRectanglesIntoPath(string svgContent, string colorHexToMatch)
        {
            // Regex to extract the rectangles with the specified fill color
            Regex rectRegex = new($@"<rect x=""(.*?)"" y=""(.*?)"" width=""(.*?)"" height=""(.*?)"" fill=""{colorHexToMatch}"" />", RegexOptions.IgnoreCase);
            MatchCollection matches = rectRegex.Matches(svgContent);

            StringBuilder pathData = new();
            foreach (Match match in matches)
            {
                string x = match.Groups[1].Value;
                string y = match.Groups[2].Value;
                string width = match.Groups[3].Value;
                string height = match.Groups[4].Value;

                // Add rectangle to path data
                pathData.Append($"M{x},{y} h{width} v{height} h-{width} Z ");
            }

            // Extract the SVG header
            Regex svgHeaderRegex = new("(<svg[^>]+>)", RegexOptions.IgnoreCase);
            string svgHeader = svgHeaderRegex.Match(svgContent).Value;

            // Build new SVG content
            StringBuilder newSvgContent = new();
            newSvgContent.Append(svgHeader);
            newSvgContent.Append($"<path fill=\"{colorHexToMatch}\" d=\"{pathData}\" />");
            newSvgContent.Append("</svg>");

            return newSvgContent.ToString();
        }
    }
}
