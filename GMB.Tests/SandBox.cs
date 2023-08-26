using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Api;
using System.Security.Cryptography;
using System.Text;
using GMB.Business.Api.API;
using GMB.Business.Api.Models;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox {

        [TestMethod]
        public async Task Main()
        {
            SeleniumDriver driver = new();
            GetBusinessProfileRequest request = new("https://www.google.com/maps/place/Brice+B%C3%A9net+-+Immobilier+N%C3%A9zignan+L'Ev%C3%AAque+-+Capifrance/data=!4m7!3m6!1s0x12b1154408e2af27:0x20bb06f5fabdb25c!8m2!3d43.4246239!4d3.4122255!16s%2Fg%2F11qnkm17pp!19sChIJJ6_iCEQVsRIRXLK9-vUGuyA", null, null);
            await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);

            return;
        }

        [TestMethod]
        public async Task BR() {
            DbLib db = new();
            List<DbBusinessReview> businessReviews = db.GetBusinessReviewTemp();
            List<Task> tasks = new();

            foreach (var chunk in businessReviews.Chunk(businessReviews.Count / 1))
            {
                Task newThread = Task.Run(() =>
                {
                    foreach (DbBusinessReview br in new List<DbBusinessReview>(chunk))
                    {
                        db.UpdateBr(br.IdReview, ToolBox.ComputeMd5Hash(br.IdEtab + br.IdReview));
                        db.UpdateBr2(br.IdReview, ToolBox.ComputeMd5Hash(br.IdEtab + br.IdReview));
                    }
                });
                tasks.Add(newThread);
            }
            await Task.WhenAll(tasks);
            return;
        }

        [TestMethod]
        public void GenerateRandomKey()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            using var rng = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            StringBuilder result = new(64);
            foreach (byte b in randomBytes)
            {
                result.Append(validChars[b % validChars.Length]);
            }
            string key = result.ToString();
            return;
        }

        [TestMethod]
        public void GetXPathfromPage() {
            string url = "https://www.google.com/maps/place/Opticien+Brest+%7C+Alain+Afflelou/@48.3882998,-4.4891174,17z/data=!3m1!4b1!4m6!3m5!1s0x4816b959cf3afcff:0x8b91c477e001d962!8m2!3d48.3882998!4d-4.4891174!16s%2Fg%2F1tgj86rm?entry=ttu";
            using SeleniumDriver driver = new();
            driver.GetToPage(url);
            var test = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.placeId).GetAttribute("innerHTML");
            int index = test.IndexOf("reviews?placeid");

            if (index != -1)
            {
                string substring = test.Substring(index + 22);
                string result = substring.Substring(0, substring.IndexOf('\\'));
            }
        }

        [TestMethod]
        public void HashToMd5() {
            string messageEncoded = ToolBox.ComputeMd5Hash("Eurorepar Garage Barbeyron" + "23 Le Bourg, 33330 Saint-Christophe-des-Bardes");
            return;
        }

        [TestMethod]
        public void HashFileToMd5() {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\test.txt";
            string endFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\test_out.txt";
            string[] etabs = File.ReadAllLines(filePath);
            using StreamWriter sw = File.AppendText(endFilePath);
            foreach (string etab in etabs) {
                string[] line = etab.Split("\t");
                sw.WriteLine(etab + "\t" + ToolBox.ComputeMd5Hash(line[1] + line[4]));
            }
            return;
        }

        [TestMethod]
        public void ComputeDateFromGoogleDate() {
            string googleDate = "il y a 2 jours";
            DateTime date = ToolBox.ComputeDateFromGoogleDate(googleDate);
            return;
        }

        [TestMethod]
        public async Task TransformGoogleAdressWithApiCall() {
            using DbLib db = new();
            GetBusinessListRequest request = new(3065766, 4, null, null, null, false, false);
            List<DbBusinessProfile> businessList = db.GetBusinessList(request);
            foreach (DbBusinessProfile business in businessList) {
                try {
                    if (business.GoogleAddress != null) {
                        ToolBox.BreakingHours();
                        AddressApiResponse? addressResponse = await ToolBox.ApiCallForAddress(business.GoogleAddress);
                        if (addressResponse != null) {
                            ToolBox.InsertApiAddressInBusiness(business, addressResponse);
                            db.UpdateBusinessProfileAddress(business);
                        }
                    }
                    db.UpdateBusinessProfileProcessingState(business.IdEtab, 0);
                } catch (Exception) {
                    continue;
                }
            }
            return;
        }
    }
}