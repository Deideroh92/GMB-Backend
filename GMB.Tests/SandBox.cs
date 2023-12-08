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
using GMB.BusinessService.Api.Controllers;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox {

        [TestMethod]
        public void Main()
        {
            var test = ToolBox.ComputeMd5Hash("https://www.google.com/maps/place/4PADEL+Annemasse/@46.2065902,6.2778623,17z/data=!3m1!4b1!4m6!3m5!1s0x478c6d4d94c69b45:0x1a1464a319f512bc!8m2!3d46.2065902!4d6.2804372!16s%2Fg%2F11ff2v_mxv?entry=ttu");

            return;
        }

        [TestMethod]
        public void CheckIfPlaceIdExist()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\placeId.txt";
            using DbLib db = new();

            List<string> values = new();

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        values.Add(line);
                }
            }

            List<string> list = new();

            foreach (string placeId in values)
            {
                DbBusinessProfile? business = db.GetBusinessByPlaceId(placeId);
                using StreamWriter operationFileWritter = File.AppendText(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Files\test.txt");
                operationFileWritter.WriteLine(business != null ? business.IdEtab : "");
            }

            return;

        }

        [TestMethod]
        public async Task GetBusinessInfos()
        {
            SeleniumDriver driver = new();
            GetBusinessProfileRequest request = new("https://www.google.com/maps/place/BRED-Banque+Populaire/@48.8280758,2.2411834,15z/data=!4m12!1m2!2m1!1sbred+boulogne+billancourt!3m8!1s0x47e67af2357c45ab:0x1b7baec714122e5b!8m2!3d48.8254931!4d2.247925!9m1!1b1!15sChlicmVkIGJvdWxvZ25lIGJpbGxhbmNvdXJ0kgEEYmFua-ABAA!16s%2Fg%2F1wf37y2x?entry=ttu", null, null);
            (DbBusinessProfile? business, DbBusinessScore? score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);

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