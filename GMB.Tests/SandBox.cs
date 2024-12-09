using GMB.BusinessService.Api.Controller;
using GMB.Scanner.Agent.Core;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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