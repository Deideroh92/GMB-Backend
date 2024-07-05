using GMB.BusinessService.Api.Controller;
using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox
    {

        [TestMethod]
        public void Main()
        {
            DbLib db = new();

            bool test = db.CheckBusinessProfileExistByNameAndAdress("Relais De Mardie", "RNE 60, 45430 Mardié, France");
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
            ToolBox.SendEmail("test");
            return;
        }
    }
}