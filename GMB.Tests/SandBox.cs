using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GMB.Tests
{
    [TestClass]
    public class SandBox
    {

        [TestMethod]
        public void Main()
        {
            string filePath = "2024-02-07-URL MAX POUR PLACE ID.txt";
            string[] lines = File.ReadAllLines(filePath);
            string outputFilePath = "file.csv";
            using StreamWriter writer = new(outputFilePath);

            DbLib db = new();

            List<string> stringList = [.. lines];

            List<BusinessAgent> bpList = db.GetBusinessAgentNetworkListByUrlList(stringList, false, false);

            foreach (BusinessAgent bp in bpList)
            {
                writer.WriteLine(bp.Url + ";" + bp.IdEtab + ";" + bp.PlaceId);
            }
            return;
        }
    }
}