using GMB.BusinessService.Api.Controller;
using GMB.ScannerService.Api.Controller;
using GMB.ScannerService.Api.Services;
using GMB.Sdk.Core;
using GMB.Sdk.Core.FileGenerators.Certificate;
using GMB.Sdk.Core.FileGenerators.Sticker;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.ScannerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace GMB.Tests
{
    [TestClass]
    public class Launch
    {
        #region Url
        /// <summary>
        /// Launch URL Scanner
        /// </summary>
        [TestMethod]
        public void ThreadsUrlScraper()
        {
            string[] categories = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "Categories.txt"));
            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "IleDeFrance.txt"));
            string[] cp = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "CpList.txt"));

            List<string> locations = new(cp);
            List<Task> tasks = [];

            int maxConcurrentThreads = 1;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            foreach (string search in categories)
            {
                ScannerUrlParameters request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Trim().Replace(' ', '+'));
                Task newThread = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Wait until there's an available slot to run
                    try
                    {
                        Scanner.Agent.Scanner.UrlScanner(request);
                    } finally
                    {
                        semaphore.Release(); // Release the slot when the task is done
                    }
                });
                tasks.Add(newThread);
            }

            Task.WaitAll([.. tasks]);
            return;
        }

        /// <summary>
        /// Create a BU in DB.
        /// </summary>
        [TestMethod]
        public void CreateUrl()
        {
            string url = "";
            BusinessController controller = new();
            controller.CreateUrl(url);
        }
        #endregion

        #region Business
        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public void SetProcessingFromIdEtabFile()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Sdk.Core\Files\Custom.txt";
            using DbLib db = new();

            List<string> values = [];

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        values.Add(line);
                }
            }

            foreach (string idEtab in values)
            {
                db.UpdateBusinessProfileProcessingState(idEtab, 11);
            }
        }        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public void DeleteFromIdEtabFile()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Sdk.Core\Files\Custom.txt";
            using DbLib db = new();

            List<string> values = [];

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        values.Add(line);
                }
            }
            try
            {
                foreach (string idEtab in values)
                {
                    DbBusinessProfile? bp = db.GetBusinessByIdEtab(idEtab);
                    List<DbBusinessReview> businessReviews = db.GetBusinessReviewsList(idEtab);
                    foreach (DbBusinessReview review in businessReviews)
                    {
                        db.DeleteBusinessReviewsFeeling(review.IdReview);
                    }
                    db.DeleteBusinessReviews(idEtab);
                    db.DeleteBusinessScore(idEtab);
                    db.DeleteBusinessProfile(idEtab);
                    db.DeleteBusinessUrlByGuid(bp.FirstGuid);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// Getting ID etab and place ID from url list.
        /// </summary>
        [TestMethod]
        public async Task GetBPFromUrlListAsync()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Sdk.Core\Files\Custom2.txt";
            using DbLib db = new();

            List<string> values = [];

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        values.Add(line);
                }
            }

            BusinessController controller = new();

            List<Task> tasks = [];

            ActionResult<GetBusinessListResponse> response = await controller.GetBusinessListByUrlAsync(values);

            string outputFilePath = "file.csv";
            using StreamWriter writer = new(outputFilePath, true);
            foreach (Business? business in response.Value.BusinessList)
            {
                if (business != null)
                    await writer.WriteAsync(business.Id + ";" + business.IdEtab + ";" + business.PlaceId + Environment.NewLine);
                else
                    await writer.WriteAsync(business.Id + ";" + business.IdEtab + ";" + business.PlaceId + "not found" + Environment.NewLine);
            }
        }

        /// <summary>
        /// Starting Scanner Test.
        /// </summary>
        [TestMethod]
        public async Task LaunchScannerTest()
        {
            ScannerController scannerController = new();
            await scannerController.StartTestAsync();
            return;
        }

        /// <summary>
        /// Starting Business Scanner.
        /// </summary>
        [TestMethod]
        public void LaunchBusinessScanner()
        {
            AuthorizationPolicyService policy = new();
            ScannerController scannerController = new();
            BusinessScannerRequest request = new(99999, 11, Operation.PROCESSING_STATE, false, DateTime.UtcNow.AddMonths(-12), false, false, null, null, null, UrlState.NEW, true, false, true);

            Task.Run(() => scannerController.StartBusinessScannerAsync(request)).Wait();
            return;
        }

        /// <summary>
        /// Starting Url Scanner.
        /// </summary>
        [TestMethod]
        public void LaunchUrlScanner()
        {
            AuthorizationPolicyService policy = new();
            ScannerController scannerController = new();

            Task.Run(() => scannerController.StartUrlScanner()).Wait();
            return;
        }
        #endregion

        #region STICKERS
        /*/// <summary>
        /// Launch Scanner Sticker
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LaunchScannerStickerAsync()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName + @"\Sdk.Core\Files\Custom.txt";
            int year = 2023;
            using DbLib db = new();
            string lang = "FR";
            int i = 0;

            // TODO : Récupérer les params de la query dans l'order : year, lang, liste place ID

            List<StickerFileRowData> rowDataList = [];

            using (StreamReader reader = new(filePath))

                while (!reader.EndOfStream)
                {
                    i++;
                    string? line = reader.ReadLine();
                    if (line != null)
                    {
                        rowDataList.Add(new StickerFileRowData(i, line));
                    }
                }
            int nbThreads = 4;

            if (rowDataList.Count < 10)
                nbThreads = 1;

            List<Task> tasks = [];

            foreach (var chunk in rowDataList.Chunk(rowDataList.Count / nbThreads))
            {
                Task newThread = Task.Run(async () =>
                {
                    ScannerController scannerController = new();

                    // TODO : mettre les paramètres de la commande ici -> lang, year
                    StickerScannerRequest request = new(Guid.NewGuid().ToString(), new List<StickerFileRowData>(chunk), year, lang);

                    ActionResult<GetStickerListResponse> response = scannerController.StartStickerScanner(request);
                });
                tasks.Add(newThread);
                Thread.Sleep(15000);
            }
            await Task.WhenAll(tasks);

            
            return;
        }*/

        [TestMethod]
        public async Task LaunchOrder()
        {
            int id = 9;
            
            DbLib db = new(true);

            // db.selectsticker();

            DbOrder? order = db.GetOrderByID(id);

            if (order == null)
                return;

            //OrderStatus status = OrderStatus.Analyzing;
            //List<DbOrder> orderList = db.GetOrderByStatus(status);

            List<DbPlace> places = db.GetPlacesFromOrderId(id);

            if (places.Count == 0)
                return;

            int nbThreads = 5;

            if (places.Count < 6)
                nbThreads = 1;

            List<Task> tasks = [];

            bool isError = false;

            foreach (var chunk in places.Chunk(places.Count / nbThreads))
            {
                Task newThread = Task.Run(() =>
                {
                    ScannerController scannerController = new();

                    StickerScannerRequest request = new(id, new List<DbPlace>(chunk), order.CreatedAt, order.Language);

                    isError = scannerController.StartStickerScanner(request);
                });
                tasks.Add(newThread);
                Thread.Sleep(15000);
            }
            await Task.WhenAll(tasks);

            if (!isError)
            {
                if (order.OwnerId == "cm1nx5an60000tem7v4rf3dkr")
                    db.UpdateOrderStatus(id, OrderStatus.Delivered);
                else
                {
                    db.UpdateOrderStatus(id, OrderStatus.Analyzed);
                    DbUserVasanoIO? user = db.GetVasanoIOUser(order.OwnerId);
                    if (user != null)
                    {
                        string translatedSubject;

                        switch (user.Country.ToLower())
                        {
                            case "france":
                                translatedSubject = "Vos STICKERS sont prêts !";
                                break;

                            case "spain":
                                translatedSubject = "¡Sus STICKERS están listos!";
                                break;

                            case "germany":
                                translatedSubject = "Ihre STICKER sind bereit!";
                                break;

                            case "italy":
                                translatedSubject = "I tuoi STICKER sono pronti!";
                                break;

                            case "portugal":
                                translatedSubject = "Os seus STICKERS estão prontos!";
                                break;

                            case "netherlands":
                                translatedSubject = "Uw STICKERS zijn klaar!";
                                break;

                            default:
                                // Default to English if no country matches
                                translatedSubject = "Your STICKERS are ready!";
                                break;
                        }


                        ToolBox.SendEmailVasanoIO(user.FirstName + " " + user.LasttName, id, translatedSubject, user.Email, user.Country);
                    }
                }
                    
            }

            return;
        }
        /// <summary>
        ///  Generating Stickers Network
        /// </summary>
        [TestMethod]
        public async Task GenerateStickersNetwork()
        {

            DbLib db = new(true);
            // Example usage of the sticker generation function
            double score = 4.9;
            string zoneGeo = "France";
            DateTime date = DateTime.Now;
            int nbReviews = 9507;
            int nbEtab = 406;
            string brand = "ERA IMMOBILIER";
            int year = 2024;

            CertificateGenerator certificateGenerator = new();
            byte[] certificate = certificateGenerator.GenerateNetworkCertificatePdf(brand, nbEtab, nbReviews, zoneGeo, score, year);

            DbStickerNetwork sticker = new(score, date, null, certificate, nbEtab, nbReviews, year, brand, zoneGeo, 4);

            int id = db.CreateStickerNetwork(sticker);

            string qrUrl = $"https://vasano.io/sticker/{id}/network/certificate";
            Bitmap stickerImage = ToolBox.CreateQrCode(qrUrl);
            stickerImage.Save($"C:\\Users\\maxim\\Desktop\\qrCode_{id}.Png", ImageFormat.Png);

            File.WriteAllBytes($"C:\\Users\\maxim\\Desktop\\certificat.pdf", certificate);
            //db.UpdateStickerNetwork(sticker);
        }
        /// <summary>
        ///  Generating Stickers Network
        /// </summary>
        [TestMethod]
        public void UpdateStickerNetworkImage()
        {

            DbLib db = new(true);
            byte[] image = File.ReadAllBytes("C:\\Users\\maxim\\Desktop\\STICKER Network - ERA.pdf");
            byte[] certificate = File.ReadAllBytes("C:\\Users\\maxim\\Desktop\\Certificat Network - ERA.pdf");
            string id = "18";
            DbStickerNetwork? sticker = db.GetStickerNetworkById(id);


            if (sticker == null)
                return;

            sticker.Certificate = certificate;
            sticker.Image = image;

            db.UpdateStickerNetwork(sticker);
        }

        /// <summary>
        ///  For testing the generation of a sticker
        /// </summary>
        [TestMethod]
        public void GenerateQrCode()
        {
            // Example usage of the sticker generation function
            string qrUrl = $"https://vasano.io/certificate/{10}";

            Bitmap stickerImage = ToolBox.CreateQrCode(qrUrl);

            // Save the final sticker image (for demonstration purposes)
            stickerImage.Save("C:\\Users\\maxim\\Desktop\\qrCode.Png", ImageFormat.Png);

            Console.WriteLine("Sticker image generated and saved as sticker_output.png");
        }

        [TestMethod]
        public void GenerateStickerTest()
        {
            StickerGenerator generator = new();
            byte[] stickerBytes = generator.Generate(StickerLanguage.FR, 4.5, "https://vasano.io/certificate/sticker_id", DateTime.Now);

            File.WriteAllBytes($"C:\\Users\\Lucas\\Documents\\Code\\Vasano\\Tests Stickers\\sticker_test.pdf", stickerBytes);
        }

        [TestMethod]
        public void GenerateStickersTest()
        {
            StickerGenerator generator = new();
            List<long> elapsedTimes = [];
            foreach (StickerLanguage language in Enum.GetValues(typeof(StickerLanguage)))
            {
                System.Diagnostics.Stopwatch sw = new();
                sw.Start();
                byte[] stickerBytes = generator.Generate(language, 4.5, "https://vasano.io/certificate/sticker_id", DateTime.Now);
                sw.Stop();
                elapsedTimes.Add(sw.ElapsedMilliseconds);

                File.WriteAllBytes($"C:\\Users\\Lucas\\Documents\\Code\\Vasano\\Tests Stickers\\sticker_{language}.pdf", stickerBytes);
            }
        }

        [TestMethod]
        public void GeneratePlaceCertificate()
        {
            CertificateGenerator generator = new();
            byte[] pdfBytes = generator.GeneratePlaceCertificatePdf(StickerLanguage.FR, "McDonald's boulogne", DateTime.Now, 35, 72, 45, 158, 24); 
            File.WriteAllBytes(@"C:\Users\Lucas\Documents\Code\Vasano\Tests Certificates\placeCertificate.pdf", pdfBytes);
        }

        [TestMethod]
        public void GenerateNetworkCertificate()
        {
            //oolBox.SendEmailVasanoIO("Test", 12, "Your STICKERS are ready!", "maximiliend1998@hotmail.fr");
        }
        #endregion
    }
}