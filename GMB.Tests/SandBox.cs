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
            return;
        }

        [TestMethod]
        public async void Sand()
        {
            string filePath = "input.txt";
            string[] lines = File.ReadAllLines(filePath);
            string outputFilePath = "file.csv";
            using StreamWriter writer = new(outputFilePath);

            DbLib db = new();

            List<string> stringList = [.. lines];

            BusinessController controller = new();

            ActionResult<GetBusinessListResponse> response = await controller.GetBusinessListByUrlAsync(stringList);

            foreach (Business? bp in response.Value.BusinessList)
            {
                writer.WriteLine(bp.PlaceUrl + ";" + bp.IdEtab + ";" + bp.PlaceId);
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