using GMB.Sdk.Core.FileGenerators.Sticker;
using iText.Forms;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using System.Globalization;

namespace GMB.Sdk.Core.FileGenerators.Certificate
{
    public class CertificateGenerator
    {
        #region Consts
        // Resources files
        const string networkCertificateTemplateFilePath = "FileGenerators/Certificate/Resources/networkCertificateTemplate.pdf";

        const string montserratBoldFontFilePath = "FileGenerators/Certificate/Resources/Montserrat-Bold.ttf";
        const string montserratLightFontFilePath = "FileGenerators/Certificate/Resources/Montserrat-Light.ttf";
        const string montserratMediumFontFilePath = "FileGenerators/Certificate/Resources/Montserrat-Medium.ttf";
        const string montserratRegularFontFilePath = "FileGenerators/Certificate/Resources/Montserrat-Regular.ttf";
        const string montserratSemiBoldFontFilePath = "FileGenerators/Certificate/Resources/Montserrat-SemiBold.ttf";

        // Shared Field ids
        const string scoreFieldId = "score";
        const string footerYearFieldId = "footerYear";

        // Place Certificate Field ids
        const string placeNameFieldId = "placeName";
        const string startDateFieldId = "startDate";
        const string endDateFieldId = "endDate";
        const string rating1FieldId = "nbRating1";
        const string rating2FieldId = "nbRating2";
        const string rating3FieldId = "nbRating3";
        const string rating4FieldId = "nbRating4";
        const string rating5FieldId = "nbRating5";
        const string ratingMeanFieldId = "meanRating";

        // Network Certificate Field ids
        const string titleYearFieldId = "titleYear";
        const string networkNameFieldId = "networkName";
        const string nbEtabsFieldId = "nbEtabs";
        const string nbReviewsFieldId = "nbReviews";
        const string geoZoneFieldId = "geoZone";
        const string scoreYearFieldId = "scoreYear";
        const string certificateDateFieldId = "certificateDate";
        const string descriptionYear1FieldId = "descriptionYear1";
        const string descriptionYear2FieldId = "descriptionYear2";
        #endregion Consts

        #region Static Fields
        // Cultures Info
        static readonly Dictionary<StickerLanguage, CultureInfo> cultures = new()
        {
            { StickerLanguage.FR, new CultureInfo("fr-FR") },
            { StickerLanguage.EN, new CultureInfo("en-GB") },
            { StickerLanguage.IT, new CultureInfo("it-IT") },
            { StickerLanguage.DE, new CultureInfo("de-DE") },
            { StickerLanguage.ES, new CultureInfo("es-ES") },
            { StickerLanguage.PT, new CultureInfo("pt-PT") },
        };

        // Template File Paths
        static readonly Dictionary<StickerLanguage, string> placeCertificateTemplatePaths = new()
        {
            { StickerLanguage.FR, "FileGenerators/Certificate/Resources/placeCertificateTemplate_fr.pdf" },
            { StickerLanguage.EN, "FileGenerators/Certificate/Resources/placeCertificateTemplate_en.pdf" },
            { StickerLanguage.IT, "FileGenerators/Certificate/Resources/placeCertificateTemplate_it.pdf" },
            { StickerLanguage.DE, "FileGenerators/Certificate/Resources/placeCertificateTemplate_es.pdf" },
            { StickerLanguage.ES, "FileGenerators/Certificate/Resources/placeCertificateTemplate_de.pdf" },
            { StickerLanguage.PT, "FileGenerators/Certificate/Resources/placeCertificateTemplate_pt.pdf" },
        };

        // Cached Resources
        static readonly Dictionary<StickerLanguage, Lazy<byte[]>> placeTemplateBytes = [];
        static readonly Lazy<byte[]> networkTemplateBytes = new(() => File.ReadAllBytes(networkCertificateTemplateFilePath));

        // Cached Font Programs
        static readonly Lazy<FontProgram> montserratBoldFontProgram = new(() => FontProgramFactory.CreateFont(montserratBoldFontFilePath));
        static readonly Lazy<FontProgram> montserratLightFontProgram = new(() => FontProgramFactory.CreateFont(montserratLightFontFilePath));
        static readonly Lazy<FontProgram> montserratMediumFontProgram = new(() => FontProgramFactory.CreateFont(montserratMediumFontFilePath));
        static readonly Lazy<FontProgram> montserratRegularFontProgram = new(() => FontProgramFactory.CreateFont(montserratRegularFontFilePath));
        static readonly Lazy<FontProgram> montserratSemiBoldFontProgram = new(() => FontProgramFactory.CreateFont(montserratSemiBoldFontFilePath));
        #endregion Static Fields

        static CertificateGenerator()
        {
            // Initialize Place Certificate Templates
            //foreach (StickerLanguage language in placeCertificateTemplatePaths.Keys)
            //    placeTemplateBytes[language] = new Lazy<byte[]>(() => File.ReadAllBytes(placeCertificateTemplatePaths[language]));
            placeTemplateBytes[StickerLanguage.FR] = new Lazy<byte[]>(() => File.ReadAllBytes(placeCertificateTemplatePaths[StickerLanguage.FR]));
        }

        public byte[] GeneratePlaceCertificatePdf(
            StickerLanguage language,
            string placeName,
            DateTime stickerDate,
            int nbRating1,
            int nbRating2,
            int nbRating3,
            int nbRating4,
            int nbRating5)
        {
            if (language != StickerLanguage.FR)
                throw new NotSupportedException();

            using MemoryStream memoryStream = new();
            using PdfReader pdfReader = new(new MemoryStream(GetPlaceTemplateByLanguage(language)));
            using PdfDocument placeCertificateDoc = new(pdfReader, new PdfWriter(memoryStream));

            // Create PdfFont instance for the document
            PdfFont montserratSemiBoldFont = PdfFontFactory.CreateFont(montserratSemiBoldFontFilePath);

            PdfAcroForm form = PdfAcroForm.GetAcroForm(placeCertificateDoc, true);

            FillPlaceCertificateForm(
                form,
                language,
                placeName,
                stickerDate,
                nbRating1,
                nbRating2,
                nbRating3,
                nbRating4,
                nbRating5,
                montserratSemiBoldFont);

            form.FlattenFields(); // Flatten the form to burn fields in doc
            placeCertificateDoc.Close(); // Close the PDF (this will write the content to the MemoryStream)

            // Return the generated PDF as a byte array
            return memoryStream.ToArray();
        }

        public byte[] GenerateNetworkCertificatePdf(
            string networkName,
            int nbEtabs,
            int nbReviews,
            string geoZone,
            double score,
            int scoreYear)
        {
            using MemoryStream memoryStream = new();
            using PdfReader pdfReader = new(new MemoryStream(networkTemplateBytes.Value));
            using PdfDocument networkCertificateDoc = new(pdfReader, new PdfWriter(memoryStream));

            // Create PdfFont instances for the document
            PdfFont montserratBoldFont = PdfFontFactory.CreateFont(montserratBoldFontFilePath);
            PdfFont montserratLightFont = PdfFontFactory.CreateFont(montserratLightFontFilePath);
            PdfFont montserratMediumFont = PdfFontFactory.CreateFont(montserratMediumFontFilePath);
            PdfFont montserratRegularFont = PdfFontFactory.CreateFont(montserratRegularFontFilePath);
            PdfFont montserratSemiBoldFont = PdfFontFactory.CreateFont(montserratSemiBoldFontFilePath);
            
            PdfAcroForm form = PdfAcroForm.GetAcroForm(networkCertificateDoc, true);

            FillNetworkCertificateForm(
                form,
                networkName,
                nbEtabs,
                nbReviews,
                geoZone,
                score,
                scoreYear,
                montserratSemiBoldFont,
                montserratRegularFont,
                montserratBoldFont,
                montserratLightFont,
                montserratMediumFont
                );
            
            form.FlattenFields(); // Flatten the form to burn fields in doc
            networkCertificateDoc.Close(); // Close the PDF (this will write the content to the MemoryStream)

            // Return the generated PDF as a byte array
            return memoryStream.ToArray();
        }

        #region Static methods
        static void FillPlaceCertificateForm(
            PdfAcroForm form,
            StickerLanguage language,
            string placeName,
            DateTime stickerDate,
            int nbRating1,
            int nbRating2,
            int nbRating3,
            int nbRating4,
            int nbRating5,
            PdfFont montserratSemiBoldFont)
        {
            CultureInfo culture = GetCultureByLanguage(language);

            double mean = Math.Round(GetMean(nbRating1, nbRating2, nbRating3, nbRating4, nbRating5), 3);
            double score = Math.Round(mean, 1);

            // Place Name
            form.GetField(placeNameFieldId)
                .SetValue(placeName)
                .SetFont(montserratSemiBoldFont)
                .SetFontSizeAutoScale();

            // Score
            form.GetField(scoreFieldId)
                .SetValue(score.ToString("0.0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 55);

            // Dates
            DateTime startStickerDate = stickerDate.AddMonths(-12);
            string startDateString = startStickerDate.ToString("d", culture);
            string endDateString = stickerDate.ToString("d", culture);

            form.GetField(startDateFieldId)
                .SetValue(startDateString)
                .SetFontAndSize(montserratSemiBoldFont, 12);

            form.GetField(endDateFieldId)
                .SetValue(endDateString)
                .SetFontAndSize(montserratSemiBoldFont, 12);

            // Ratings
            form.GetField(rating1FieldId)
                .SetValue(nbRating1.ToString("N0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 12);

            form.GetField(rating2FieldId)
                .SetValue(nbRating2.ToString("N0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 12);

            form.GetField(rating3FieldId)
                .SetValue(nbRating3.ToString("N0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 12);

            form.GetField(rating4FieldId)
                .SetValue(nbRating4.ToString("N0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 12);

            form.GetField(rating5FieldId)
                .SetValue(nbRating5.ToString("N0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 12);

            form.GetField(ratingMeanFieldId)
                .SetValue(mean.ToString("0.000", culture))
                .SetFontAndSize(montserratSemiBoldFont, 12);

            // Footer Year
            string currentYear = DateTime.Now.Year.ToString();
            form.GetField(footerYearFieldId)
                .SetValue(currentYear)
                .SetFontAndSize(montserratSemiBoldFont, 8);
        }

        static void FillNetworkCertificateForm(
            PdfAcroForm form,
            string networkName,
            int nbEtabs,
            int nbReviews,
            string geoZone,
            double score,
            int scoreYear,
            PdfFont montserratSemiBoldFont,
            PdfFont montserratRegularFont,
            PdfFont montserratBoldFont,
            PdfFont montserratLightFont,
            PdfFont montserratMediumFont)
        {
            CultureInfo culture = GetCultureByLanguage(StickerLanguage.FR); // Network certificates are French only

            // Network Name
            form.GetField(networkNameFieldId)
                .SetValue(networkName)
                .SetFont(montserratSemiBoldFont)
                .SetFontSizeAutoScale();

            // Number of Establishments
            form.GetField(nbEtabsFieldId)
                .SetValue(nbEtabs.ToString("N0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 16);

            // Number of Reviews
            form.GetField(nbReviewsFieldId)
                .SetValue(nbReviews.ToString("N0", culture).Replace(culture.NumberFormat.NumberGroupSeparator, " "))
                .SetFontAndSize(montserratSemiBoldFont, 16);

            // Geographic Zone
            form.GetField(geoZoneFieldId)
                .SetValue(geoZone)
                .SetFont(montserratSemiBoldFont)
                .SetFontSizeAutoScale();

            // Score
            form.GetField(scoreFieldId)
                .SetValue(Math.Round(score, 1).ToString("0.0", culture))
                .SetFontAndSize(montserratSemiBoldFont, 55);

            // Title Year and Score Year
            form.GetField(titleYearFieldId)
                .SetValue(scoreYear.ToString())
                .SetFontAndSize(montserratRegularFont, 15);

            form.GetField(scoreYearFieldId)
                .SetValue(scoreYear.ToString())
                .SetFontAndSize(montserratBoldFont, 13);

            // Certificate Date
            string certificateDate = DateTime.Now.ToString("d", culture);
            form.GetField(certificateDateFieldId)
                .SetValue(certificateDate)
                .SetFontAndSize(montserratSemiBoldFont, 16);

            // Description Year and Footer Year
            string currentYear = DateTime.Now.Year.ToString();

            form.GetField(descriptionYear1FieldId)
                .SetValue(currentYear)
                .SetFontAndSize(montserratLightFont, 9);

            form.GetField(descriptionYear2FieldId)
                .SetValue(currentYear)
                .SetFontAndSize(montserratLightFont, 9);

            form.GetField(footerYearFieldId)
                .SetValue(currentYear)
                .SetFontAndSize(montserratMediumFont, 8);
        }

        static byte[] GetPlaceTemplateByLanguage(StickerLanguage language)
        {
            if (placeTemplateBytes.TryGetValue(language, out Lazy<byte[]>? lazyBytes))
            {
                return lazyBytes.Value;
            }
            throw new NotSupportedException($"Template for language '{language}' is not supported.");
        }

        static CultureInfo GetCultureByLanguage(StickerLanguage language)
        {
            if (cultures.TryGetValue(language, out CultureInfo? culture))
            {
                return culture;
            }
            return CultureInfo.InvariantCulture;
        }

        static double GetMean(int nbRating1, int nbRating2, int nbRating3, int nbRating4, int nbRating5)
        {
            int totalRatings = nbRating1 + nbRating2 + nbRating3 + nbRating4 + nbRating5;
            if (totalRatings == 0)
            {
                return 0;
            }

            double weightedSum = nbRating1 * 1 + nbRating2 * 2 + nbRating3 * 3 + nbRating4 * 4 + nbRating5 * 5;
            return weightedSum / totalRatings;
        }
        #endregion Static methods
    }
}
