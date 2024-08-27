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
            string url = "https://www.google.com/maps/place/Starbucks/@48.8370969,2.2386167,18.97z/data=!3m1!5s0x47e67ae79f1bc11b:0x7edf0a3a4d967b6c!4m14!1m7!3m6!1s0x47e67ae7a1990ecb:0x4b52704d99f5fa86!2sLes+Passages+Shopping+Center!8m2!3d48.8372222!4d2.2397222!16s%2Fg%2F1tph11dz!3m5!1s0x47e67ae79ff165e9:0x327bbed739092bc4!8m2!3d48.8367!4d2.23933!16s%2Fg%2F1tcvhdfk?entry=ttu&g_ep=EgoyMDI0MDgyMS4wIKXMDSoASAFQAw%3D%3D";
            (DbBusinessProfile? profile, DbBusinessScore? score) = await ScannerFunctions.GetBusinessProfileAndScoreFromGooglePageAsync(driver, new(url), null);

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