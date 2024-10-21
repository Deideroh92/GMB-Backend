using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using System.Globalization;

namespace GMB.Sdk.Core.StickerCertificateGenerator
{
    public class StickerCertificateGenerator
    {
        #region Consts
        // Resources files
        const string placeCertificateTemplateFilePath = "StickerCertificateGenerator/placeCertificateTemplate.pdf";
        const string networkCertificateTemplateFilePath = "StickerCertificateGenerator/networkCertificateTemplate.pdf";
        const string montserratBoldFontFilePath = "StickerCertificateGenerator/Montserrat-Bold.ttf";
        const string montserratLightFontFilePath = "StickerCertificateGenerator/Montserrat-Light.ttf";
        const string montserratMediumFontFilePath = "StickerCertificateGenerator/Montserrat-Medium.ttf";
        const string montserratRegularFontFilePath = "StickerCertificateGenerator/Montserrat-Regular.ttf";
        const string montserratSemiBoldFontFilePath = "StickerCertificateGenerator/Montserrat-SemiBold.ttf";

        // Shared Field ids
        const string scoreFieldId = "score";
        const string footerYearFieldId = "footerYear";

        // Place Field ids
        const string placeNameFieldId = "placeName";
        const string startDateFieldId = "startDate";
        const string endDateFieldId = "endDate";
        const string rating1FieldId = "nbRating1";
        const string rating2FieldId = "nbRating2";
        const string rating3FieldId = "nbRating3";
        const string rating4FieldId = "nbRating4";
        const string rating5FieldId = "nbRating5";
        const string ratingMeanFieldId = "meanRating";

        // Network Field ids
        const string titleYearFieldId = "titleYear";
        const string networkNameFieldId = "networkName";
        const string nbEtabsFieldId = "nbEtabs";
        const string nbReviewsFieldId = "nbReviews";
        const string geoZoneFieldId = "geoZone";
        const string scoreYearFieldId = "scoreYear";
        const string certificateDateFieldId = "certificateDate";
        const string descriptionYear1FieldId = "descriptionYear1";
        const string descriptionYear2FieldId = "descriptionYear2";
        // Strangely, Adobe Acrobat adds "_af_image" to field's name.
        // Note that this field is of type image, not text like others
        const string networkImageFieldId = "networkImage_af_image";
        #endregion Consts

        static readonly CultureInfo frenchCulture = new ("fr-FR");
        // TODO: add lazy on images and fonts => first check if there is need to do it when running generator in RabbitMQ Queue
        static readonly byte[] placePdfTemplateBytes;
        static readonly byte[] networkPdfTemplateBytes;
        static readonly PdfFont montserratBoldFont;
        static readonly PdfFont montserratLightFont;
        static readonly PdfFont montserratMediumFont;
        static readonly PdfFont montserratRegularFont;
        static readonly PdfFont montserratSemiBoldFont;

        static StickerCertificateGenerator()
        {
            placePdfTemplateBytes = File.ReadAllBytes(placeCertificateTemplateFilePath);
            networkPdfTemplateBytes = File.ReadAllBytes(networkCertificateTemplateFilePath);

            montserratBoldFont = PdfFontFactory.CreateFont(montserratBoldFontFilePath);
            montserratLightFont = PdfFontFactory.CreateFont(montserratLightFontFilePath);
            montserratMediumFont = PdfFontFactory.CreateFont(montserratMediumFontFilePath);
            montserratRegularFont = PdfFontFactory.CreateFont(montserratRegularFontFilePath);
            montserratSemiBoldFont = PdfFontFactory.CreateFont(montserratSemiBoldFontFilePath);

            frenchCulture.NumberFormat.NumberGroupSeparator = " ";
        }

        public byte[] GeneratePlaceCertificatePdf(string placeName, DateTime stickerDate, int nbRating1, int nbRating2, int nbRating3, int nbRating4, int nbRating5) {
            using MemoryStream memoryStream = new();
            using (PdfReader pdfReader = new(new MemoryStream(placePdfTemplateBytes)))
            {
                PdfDocument placeCertificateDoc = new(pdfReader, new PdfWriter(memoryStream));

                PdfAcroForm form = PdfAcroForm.GetAcroForm(placeCertificateDoc, true);

                string actualYear = DateTime.Now.Year.ToString();
                double mean = Math.Round(GetMean(nbRating1, nbRating2, nbRating3, nbRating4, nbRating5), 3);
                double score = Math.Round(mean, 1);

                // place name (Font Size = 44 in pdf)
                form.GetField(placeNameFieldId).SetValue(placeName).SetFont(montserratSemiBoldFont).SetFontSizeAutoScale();

                // Place score
                form.GetField(scoreFieldId).SetValue(score.ToString("0.0", frenchCulture)).SetFontAndSize(montserratSemiBoldFont, 55);

                // Certificate dates
                DateTime startStickerDate = stickerDate.AddMonths(-12);
                string startDateString = DateOnly.FromDateTime(startStickerDate).ToString(frenchCulture);
                string endDateString = DateOnly.FromDateTime(stickerDate).ToString(frenchCulture);
                form.GetField(startDateFieldId).SetValue(startDateString).SetFontAndSize(montserratSemiBoldFont, 12);
                form.GetField(endDateFieldId).SetValue(endDateString).SetFontAndSize(montserratSemiBoldFont, 12);

                // Place ratings
                form.GetField(rating1FieldId).SetValue(nbRating1.ToString()).SetFontAndSize(montserratSemiBoldFont, 12);
                form.GetField(rating2FieldId).SetValue(nbRating2.ToString()).SetFontAndSize(montserratSemiBoldFont, 12);
                form.GetField(rating3FieldId).SetValue(nbRating3.ToString()).SetFontAndSize(montserratSemiBoldFont, 12);
                form.GetField(rating4FieldId).SetValue(nbRating4.ToString()).SetFontAndSize(montserratSemiBoldFont, 12);
                form.GetField(rating5FieldId).SetValue(nbRating5.ToString()).SetFontAndSize(montserratSemiBoldFont, 12);
                form.GetField(ratingMeanFieldId).SetValue(mean.ToString("0.000", frenchCulture)).SetFontAndSize(montserratSemiBoldFont, 12);

                // Actual year
                form.GetField(footerYearFieldId).SetValue(DateTime.Now.Year.ToString()).SetFontAndSize(montserratMediumFont, 8);

                // Flatten the form to burn fields in doc
                form.FlattenFields();

                // Close the PDF (this will write the content to the MemoryStream)
                placeCertificateDoc.Close();
            }

            // Return the generated PDF as a byte array
            return memoryStream.ToArray();
        }

        public byte[] GenerateNetworkCertificatePdf(string networkName, int nbEtabs, int nbReviews, string geoZone, double score, int scoreYear)
        {
            using MemoryStream memoryStream = new();
            using (PdfReader pdfReader = new(new MemoryStream(networkPdfTemplateBytes)))
            {
                PdfDocument networkCertificateDoc = new(pdfReader, new PdfWriter(memoryStream));

                PdfAcroForm form = PdfAcroForm.GetAcroForm(networkCertificateDoc, true);

                // Network name
                form.GetField(networkNameFieldId).SetValue(networkName).SetFont(montserratSemiBoldFont).SetFontSizeAutoScale(); // 18 in pdf

                // Number of etabs
                form.GetField(nbEtabsFieldId).SetValue(nbEtabs.ToString("N0", frenchCulture)).SetFontAndSize(montserratSemiBoldFont, 16);

                // Number of reviews
                form.GetField(nbReviewsFieldId).SetValue(nbReviews.ToString("N0", frenchCulture)).SetFontAndSize(montserratSemiBoldFont, 16);

                // Geographic zone
                form.GetField(geoZoneFieldId).SetValue(geoZone).SetFont(montserratSemiBoldFont).SetFontSizeAutoScale();//.SetFontAndSize(montserratMediumFont, 18);

                // Score
                form.GetField(scoreFieldId).SetValue(Math.Round(score, 1).ToString("0.0", frenchCulture)).SetFontAndSize(montserratSemiBoldFont, 55);

                // Title year and Score year
                form.GetField(titleYearFieldId).SetValue(scoreYear.ToString()).SetFontAndSize(montserratRegularFont, 15);
                form.GetField(scoreYearFieldId).SetValue(scoreYear.ToString()).SetFontAndSize(montserratBoldFont, 13);

                // Actual date
                form.GetField(certificateDateFieldId).SetValue(DateOnly.FromDateTime(DateTime.Now).ToString(frenchCulture)).SetFontAndSize(montserratSemiBoldFont, 16);

                // Actual year
                string actualYear = DateTime.Now.Year.ToString();
                form.GetField(descriptionYear1FieldId).SetValue(actualYear).SetFontAndSize(montserratLightFont, 9);
                form.GetField(descriptionYear2FieldId).SetValue(actualYear).SetFontAndSize(montserratLightFont, 9);
                form.GetField(footerYearFieldId).SetValue(actualYear).SetFontAndSize(montserratMediumFont, 8);

                // Get field rectangle
                PdfFormField networkImageField = form.GetField(networkImageFieldId);
                PdfWidgetAnnotation widget = networkImageField.GetWidgets()[0];
                Rectangle fieldRectangle = widget.GetRectangle().ToRectangle();
                // Print image in page
                PdfPage page = widget.GetPage();
                // Remove field as it's over the image (only when border color set to transparent, dunno why)
                form.RemoveField(networkImageFieldId);

                // Flatten the form to burn fields in doc
                form.FlattenFields();

                // Close the PDF (this will write the content to the MemoryStream)
                networkCertificateDoc.Close();
            }

            // Return the generated PDF as a byte array
            return memoryStream.ToArray();
        }

        static double GetMean(int nbRating1, int nbRating2, int nbRating3, int nbRating4, int nbRating5) =>
            (nbRating1 + nbRating2 * 2 + nbRating3 * 3 + nbRating4 * 4 + nbRating5 * 5)
            /
            (double)(nbRating1 + nbRating2 + nbRating3 + nbRating4 + nbRating5);

    }
}
