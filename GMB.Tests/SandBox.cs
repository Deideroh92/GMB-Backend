using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using GMB.Sdk.Core.Types.Api;
using AngleSharp.Dom;
using GMB.Business.Api.API;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox {

        [TestMethod]
        public async Task Main() {
            using SeleniumDriver driver = new();
            string url = "https://www.google.com/maps/place/L+'oeuf+de+Seyssel+-+Grossiste+aupr%C3%A8s+des+professionnels+en+Savoie+et+Haute-Savoie/data=!4m7!3m6!1s0x478b7bc48ede25bd:0x6edc947e50220b28!8m2!3d45.9489915!4d5.853033!16s%2Fg%2F11fl0xr2jv!19sChIJvSXejsR7i0cRKAsiUH6U3G4";
            DbBusinessScore? score;
            DbBusinessProfile? profile;
            (profile, score) = await BusinessServiceApi.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url, null, null), null);
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