using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Models;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox {

        [TestMethod]
        public void Main() {
            using DbLib db = new();

            var test = db.CheckBusinessUrlExist(ToolBox.ComputeMd5Hash("https://www.google.com/maps/place/Sellola/@42.0937792,9.1015375,17z/data=!4m6!3m5!1s0x12da03913f99db3f:0xfe3aa5956dd1ba29!8m2!3d42.0937792!4d9.1015375!16s%2Fg%2F11hzpqbhwd?entry=ttu"));

            return;
        }

        [TestMethod]
        public void GetXPathfromPage() {
            string url = "https://www.google.com/maps/place/BRED-Banque+Populaire/@48.8280761,2.2411834,15z/data=!4m10!1m2!2m1!1sbanque!3m6!1s0x47e67af2357c45ab:0x1b7baec714122e5b!8m2!3d48.8255006!4d2.2479565!15sCgZiYW5xdWWSAQRiYW5r4AEA!16s%2Fg%2F1wf37y2x";
            using SeleniumDriver driver = new();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            driver.GetToPage(url);

            if (ToolBox.Exists(ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test))) {
                string value = ToolBox.FindElementSafe(driver.WebDriver, XPathProfile.test).GetAttribute("src").Trim();
                Console.WriteLine(value);
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