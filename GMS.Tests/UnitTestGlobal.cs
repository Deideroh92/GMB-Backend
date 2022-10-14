using GMS.BusinessProfile.Agent.Model;
using GMS.Business.Agent;
using GMS.Sdk.Core.Database;
using GMS.Sdk.Core.SeleniumDriver;
using GMS.Sdk.Core.ToolBox;
using GMS.Url.Agent;
using GMS.Url.Agent.Model;

namespace GMS.Tests {
    [TestClass]
    public class UnitTestGlobal {
        [TestMethod]
        public void TestUrlFinderService() {
            List<string> textSearch = new()
            {
                "bred", "credit agricole", "lcl"
            };

            List<string> locations = new()
            {
                "Paris", "53000"
            };
            foreach (string search in textSearch) {
                foreach (string location in locations) {
                    string searchString = search.Replace(' ', '+') + location.Replace(' ', '+');
                    UrlAgentRequest request = new(searchString, DriverType.CHROME);
                    UrlService.Start(request);
                }
            }
        }

        [TestMethod]
        public void TestProfileServiceScoreByUrlState() {
            List<string> businessList = new() {
                "https://www.google.com/maps/place/St%C3%A9phane+Plaza+Immobilier+-+Croissy+Le+V%C3%A9sinet+%7C+Agence+Immobili%C3%A8re+Le+V%C3%A9sinet+%7C+%7C+Agence+Immobili%C3%A8re+Croisy+%7C+Estimation+Immobili%C3%A8re+Le+V%C3%A9sinet/data=!4m6!3m5!1s0x47e66338606cb5fb:0x8123b0d861967fa1!8m2!3d48.8916677!4d2.1344151!16s%2Fg%2F11j2y_0kl5",
                "https://www.google.com/maps/place/NATURALIA+VEGAN/data=!4m6!3m5!1s0x47e66fb02e7671bf:0xa6638bf45a051452!8m2!3d48.8864182!4d2.3156574!16s%2Fg%2F11cm16phlg",
                "https://www.google.com/maps/place/Julia+Frangoulis+iad+France/data=!4m6!3m5!1s0x12cc279f584a6a73:0x6701898ad2326fe5!8m2!3d43.6002869!4d6.9108999!16s%2Fg%2F11h_k7znxd",
                "https://www.google.com/maps/place/AXA+Assurance+Andre+Montocchio/data=!4m6!3m5!1s0x47e671d14661f79f:0x5d9bdf19a93da55d!8m2!3d48.8325616!4d2.2921005!16s%2Fg%2F11h54h9_k1",
                "https://www.google.com/maps/search/LCL+Banque+et+assurance/data=!4m6!3m5!1s0x47eac87b2724ddfd:0x43bbf6fc8802a79f!8m2!3d49.5142806!4d5.7672938!16s%2Fg%2F1tl0wn_3",
                "https://www.google.com/maps/place/Julia+Frangoulis+iad+France/data=!4m6!3m5!1s0x12cc279f584a6a73:0x6701898ad2326fe5!8m2!3d43.6002869!4d6.9108999!16s%2Fg%2F11h_k7znxd",
                "https://www.google.com/maps/place/AXA+Assurance+Andre+Montocchio/data=!4m6!3m5!1s0x47e671d14661f79f:0x5d9bdf19a93da55d!8m2!3d48.8325616!4d2.2921005!16s%2Fg%2F11h54h9_k1",
                "https://www.google.com/maps/search/LCL+Banque+et+assurance/data=!4m6!3m5!1s0x47eac87b2724ddfd:0x43bbf6fc8802a79f!8m2!3d49.5142806!4d5.7672938!16s%2Fg%2F1tl0wn_3"
            };
            BusinessAgentRequest request = new(false, 10, businessList, null, null, UrlState.NEW);
            BusinessService.Start(request);
        }

        [TestMethod]
        public void TestProfileServiceScoreByCategory() {
            System.Diagnostics.Debug.WriteLine("test");
            BusinessAgentRequest request = new(true, 10, null, DateTime.UtcNow.AddMonths(-1), "CLUB DE SPORT");
            BusinessService.Start(request);
        }

        [TestMethod]
        public void HashToMd5() {
            string messageEncoded = ToolBox.ComputeMd5Hash("https://www.google.com/maps/search/EMILIE+ALBINET+conseilli%C3%A8re+immobili%C3%A8re+SAFTI+VARENNES,+VILLEBRUMIER,+SAINT+NAUPHARY,+NOHIC,+ORGUEIL");
            System.Diagnostics.Debug.WriteLine(messageEncoded);
        }

        [TestMethod]
        public void ComputeDateFromGoogleDate() {
            string googleDate = "il y a 2 jours";
            DateTime date = ToolBox.ComputeDateFromGoogleDate(googleDate);
            System.Diagnostics.Debug.WriteLine(date);
        }

        [TestMethod]
        public void Temp() {

            string file = @"C:\Users\maxim\Desktop\url1.txt";
            string[] urls = File.ReadAllLines(file);

            BusinessAgentRequest request = new(true, null, new List<string>(urls), DateTime.UtcNow.AddYears(-1));

            BusinessService.Start(request);
        }

        [TestMethod]
        public void Temp2() {

            string file = @"C:\Users\maxim\Desktop\url2.txt";
            string[] urls = File.ReadAllLines(file);

            BusinessAgentRequest request = new(false, null, new List<string>(urls));

            BusinessService.Start(request);
        }

        [TestMethod]
        public void Temp3() {

            string file = @"C:\Users\maxim\Desktop\url3.txt";
            string[] urls = File.ReadAllLines(file);

            BusinessAgentRequest request = new(false, null, new List<string>(urls));

            BusinessService.Start(request);
        }

        [TestMethod]
        public void Temp4() {

            string file = @"C:\Users\maxim\Desktop\url4.txt";
            string[] urls = File.ReadAllLines(file);

            BusinessAgentRequest request = new(false, null, new List<string>(urls));

            BusinessService.Start(request);
        }

        [TestMethod]
        public void TempGlobal() {
            Task newThread = new(Temp);
            Task newThread2 = new(Temp2);
            Task newThread3 = new(Temp3);
            Task newThread4 = new(Temp4);

            newThread.Start();
            newThread2.Start();
            newThread3.Start();
            newThread4.Start();

            Task.WaitAll(newThread, newThread2, newThread3, newThread4);
        }
    }
}