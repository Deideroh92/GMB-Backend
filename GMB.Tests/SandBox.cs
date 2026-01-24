using GMB.BusinessService.Api.Controller;
using GMB.Scanner.Agent.Core;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using OfficeOpenXml;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox
    {

        [TestMethod]
        public async void Main()
        {
            return;
        }

        [TestMethod]
        public async Task CheckXPath()
        {
            SeleniumDriver driver = new();
            string url = "https://www.google.com/maps/place/Allianz+Assurance+PARIS+ETOILE+-+SENECHAL+%26+CAILLARD/@48.8743443,2.2503592,13z/data=!4m10!1m2!2m1!1sallianz!3m6!1s0x47e66f7d3df59105:0x2fdab494f64176c8!8m2!3d48.8743443!4d2.2915579!15sCgdhbGxpYW56IgOIAQFaCSIHYWxsaWFuepIBEGluc3VyYW5jZV9hZ2VuY3ngAQA!16s%2Fg%2F11h7tdybz2?entry=ttu&g_ep=EgoyMDI0MTIwNC4wIKXMDSoASAFQAw%3D%3D";
            (DbBusinessProfile? profile, DbBusinessScore? score) = await ScannerFunctions.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url), null);
            driver.Dispose();
            return;
        }

        [TestMethod]
        public void LaunchReviewsThemes()
        {
            int processing = 1;

            // 1️⃣ Chargement des avis (mono-thread)
            DbLib db = new();
            List<DbBusinessReview> reviews = db.GetBusinessReviewsListByProcessing(processing);

            // 2️⃣ Chargement du dictionnaire (mono-thread, shared)
            Dictionary<string, List<int>> themes = db.GetKeywordThemeMap();

            // 3️⃣ Traitement parallèle
            Parallel.ForEach(
                reviews,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(4, Environment.ProcessorCount)
                },
                review =>
                {
                    // ⚠️ DbLib par thread
                    using DbLib localDb = new();

                    if (!string.IsNullOrWhiteSpace(review.ReviewText))
                    {
                        HashSet<int> themesFound =
                            ToolBox.DetectThemes(review.ReviewText, themes);

                        if (themesFound.Count > 0)
                            localDb.InsertThemeMatches(review.IdReview, themesFound);
                    }

                    localDb.UpdateBusinessReviewProcessing(review.IdReview, 0);
                }
            );
        }


        private static DateTime ParseSafeDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return DateTime.MinValue;

            if (DateTime.TryParse(text, out DateTime result))
                return result;

            Console.WriteLine($"Impossible de parser la date : '{text}'");
            return DateTime.MinValue;
        }


        [TestMethod]
        public async Task ReviewFix()
        {
            var reviews = new List<ReviewData>();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "data_to_treat.xlsx";
            string fullPath = Path.Combine(desktopPath, fileName);

            

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Max Dav");

                using (var package = new ExcelPackage(new FileInfo(fullPath)))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // première feuille
                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    // Trouver les index des colonnes par leur nom
                    int reviewIdCol = -1;
                    int googleDateCol = -1;
                    int dateInsertCol = -1;
                    int reviewDateCol = -1;

                    for (int col = 1; col <= colCount; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (header == "REVIEW_ID")
                            reviewIdCol = col;
                        if (header == "REVIEW_GOOGLE_DATE_UPDATE")
                            googleDateCol = col;
                        if (header == "DATE_INSERT")
                            dateInsertCol = col;
                        if (header == "REVIEW_DATE")
                            reviewDateCol = col;
                    }

                    if (reviewIdCol == -1 || googleDateCol == -1 || dateInsertCol == -1 || reviewDateCol == -1)
                    {
                        throw new Exception("Une ou plusieurs colonnes sont manquantes dans le fichier Excel.");
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var review = new ReviewData
                        {
                            ReviewId = worksheet.Cells[row, reviewIdCol].Text.Trim(),
                            ReviewGoogleDateUpdate = worksheet.Cells[row, googleDateCol].Text.Trim(),
                            DateInsert = ParseSafeDate(worksheet.Cells[row, dateInsertCol].Text.Trim()),
                            ReviewDate = ParseSafeDate(worksheet.Cells[row, reviewDateCol].Text.Trim())
                        };
                        reviews.Add(review);
                    }
                }

                foreach (ReviewData review in reviews)
                {
                    try
                    {
                        review.NewDate = ToolBox.ComputeDateFromGoogleDateTemp(review.DateInsert, review.ReviewGoogleDateUpdate);
                    } catch (ArgumentOutOfRangeException ex)
                    {
                        Console.WriteLine($"Erreur sur review {review.ReviewId}: {ex.Message}");
                        // tu peux logguer ici si besoin, et passer à la suite
                    }
                }
                DbLib db = new();
                /*foreach (ReviewData review in reviews)
                { 
                    db.UpdateReviewTemp(review);
                }*/
                db.DisconnectFromDB();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void Sand()
        {
            string filePath = "input.txt";
            string[] lines = File.ReadAllLines(filePath);
            string outputFilePath = "output.txt";
            using StreamWriter writer = new(outputFilePath);

            DbLib db = new();

            BusinessController controller = new();

            int i = 0;
            try
            {
                foreach (string line in lines)
                {
                    DbBusinessProfile? bp = db.GetBusinessByIdEtab(line);

                    if (bp == null || bp.Name == null || bp.GoogleAddress == null)
                    {
                        writer.WriteLine(line + "\n");
                        continue;
                    }

                    List<DbBusinessProfile> bpList = db.GetBusinessByNameAndAdress(bp.Name, bp.GoogleAddress);
                    foreach (DbBusinessProfile bpToDelete in bpList)
                    {
                        if (bpToDelete.IdEtab != line)
                        {
                            controller.DeleteBusinessProfile(bpToDelete.IdEtab);
                            i++;
                        }
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return;
        }

        [TestMethod]
        public void SendMail()
        {
            ToolBox.SendEmail("test", "test");
            return;
        }
    }
}