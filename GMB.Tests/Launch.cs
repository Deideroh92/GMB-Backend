using GMB.BusinessService.Api.Controller;
using GMB.BusinessService.Api.Models;
using GMB.Scanner.Agent.Core;
using GMB.Scanner.Agent.Models;
using GMB.ScannerService.Api.Controller;
using GMB.ScannerService.Api.Services;
using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using GMB.Sdk.Core.Types.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GMB.Tests
{
    [TestClass]
    public class Launch
    {
        #region Url
        /// <summary>
        /// Launch URL Scanner
        /// </summary>
        [TestMethod]
        public void ThreadsUrlScraper()
        {
            string[] categories = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "Categories.txt"));
            string[] dept = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "DeptList.txt"));
            string[] idf = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "IleDeFrance.txt"));
            string[] cp = File.ReadAllLines(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "GMB.Scanner.Agent\\ReferentialFiles", "CpList.txt"));

            List<string> locations = new(cp);
            List<Task> tasks = [];

            int maxConcurrentThreads = 1;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            foreach (string search in categories)
            {
                ScannerUrlRequest request = new(locations.Select(s => s.Replace(';', ' ').Replace(' ', '+')).ToList(), search.Trim().Replace(' ', '+'));
                Task newThread = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Wait until there's an available slot to run
                    try
                    {
                        Scanner.Agent.Scanner.ScannerUrl(request);
                    } finally
                    {
                        semaphore.Release(); // Release the slot when the task is done
                    }
                });
                tasks.Add(newThread);
            }

            Task.WaitAll([.. tasks]);
            return;
        }

        /// <summary>
        /// Create a BU in DB.
        /// </summary>
        [TestMethod]
        public void CreateUrl()
        {
            string url = "";
            BusinessController controller = new();
            controller.CreateUrl(url);
        }
        #endregion

        #region Business
        /// <summary>
        /// Weekly testing for XPATH.
        /// </summary>
        [TestMethod]
        public async Task WeeklyTestAsync()
        {
            ScannerFunctions scanner = new();
            SeleniumDriver driver = new();
            bool mairieCheck = true;
            bool louvre = true;
            bool gareMontparnasse = true;
            bool gareBordeaux = true;
            bool museeOrangerie = true;
            bool tourEiffel = true;
            bool necker = true;
            bool parcDesPrinces = true;
            bool banqueDeFrance = true;
            bool rolandGarros = true;
            bool maxim = true;

            #region Mairie de Paris
            GetBusinessProfileRequest request = new("https://www.google.fr/maps/place/Mairie+de+Paris/@48.8660828,2.3108754,13z/data=!4m10!1m2!2m1!1smairie+de+paris!3m6!1s0x47e66e23b4333db3:0xbc314dec89c4971!8m2!3d48.8641075!4d2.3421539!15sCg9tYWlyaWUgZGUgcGFyaXNaESIPbWFpcmllIGRlIHBhcmlzkgEJY2l0eV9oYWxsmgEjQ2haRFNVaE5NRzluUzBWSlEwRm5TVVEyYzA5MWNrbFJFQUXgAQA!16s%2Fg%2F11c6pn36ph?hl=fr&entry=ttu");
            (DbBusinessProfile? profile, DbBusinessScore? score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Mairie de Paris" ||
                (profile.GoogleAddress != "40 Rue du Louvre, 75001 Paris" && profile.GoogleAddress != "40 Rue du Louvre, 75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.StreetNumber != "40" ||
                profile.Category != "Hôtel de ville" ||
                profile.Website != "https://www.paris.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNbLMIrgq3do3iVaTPocWsR6mluAc1wNNKW-62h=w426-h240-k-no" ||
                profile.PlusCode != "8FW4V87R+JV" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                mairieCheck = false;
            #endregion

            #region Louvre
            request = new("https://www.google.fr/maps/place/Mus%C3%A9e+du+Louvre/@48.8606111,2.337644,17z/data=!3m1!4b1!4m6!3m5!1s0x47e671d877937b0f:0xb975fcfa192f84d4!8m2!3d48.8606111!4d2.337644!16zL20vMDRnZHI?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Musée du Louvre" ||
                (profile.GoogleAddress != "75001 Paris" && profile.GoogleAddress != "75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.Category != "Musée d'art" ||
                profile.Website != "https://www.louvre.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipM_ApMgFfAP8CP2ZHJUOb13K7P_SqSkW9sh9MFY=w408-h272-k-no" ||
                profile.PlusCode != "8FW4V86Q+63" ||
                profile.Tel != "01 40 20 53 17" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                louvre = false;
            #endregion

            #region Gare Montparnasse
            request = new("https://www.google.fr/maps/place/Gare+Montparnasse/@48.8411382,2.3205261,17z/data=!3m1!4b1!4m6!3m5!1s0x47e67034ac4d4559:0xe467cc61460dc234!8m2!3d48.8411382!4d2.3205261!16zL20vMDE2anY4?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Gare Montparnasse" ||
                (profile.GoogleAddress != "17 Bd de Vaugirard, 75015 Paris" && profile.GoogleAddress != "17 Bd de Vaugirard, 75015 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75115" ||
                profile.StreetNumber != "17" ||
                profile.Country != "France" ||
                profile.Category != "Station de transit" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipOvFa1JfgCp9tJGtq0DWviVwgyl5TKMcR_6CEvF=w426-h240-k-no" ||
                profile.PlusCode != "8FW4R8RC+F6" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                gareMontparnasse = false;
            #endregion

            #region Gare de Bordeaux
            request = new("https://www.google.fr/maps/place/Bordeaux+Saint-Jean/@44.8332874,-0.6118573,14z/data=!4m10!1m2!2m1!1sgare+de+bordeaux!3m6!1s0xd552648d40fc247:0x2bc94166a0a4eed6!8m2!3d44.8264574!4d-0.5560982!15sChBnYXJlIGRlIGJvcmRlYXV4kgENdHJhaW5fc3RhdGlvbuABAA!16s%2Fg%2F1pxyc0w63?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Bordeaux Saint-Jean" ||
                (profile.GoogleAddress != "Rue Charles Domercq, 33800 Bordeaux" && profile.GoogleAddress != "Rue Charles Domercq, 33800 Bordeaux, France") ||
                profile.City != "Bordeaux" ||
                profile.Country != "France" ||
                profile.Category != "Gare" ||
                profile.Website != "http://www.gares-sncf.com/fr/gare/frboj/bordeaux-saint-jean" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNnUmG5q3sauygk9_d62NoPVFHDP2q77pdFDylN=w426-h240-k-no" ||
                profile.PlusCode != "8CPXRCGV+HH" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                gareBordeaux = false;
            #endregion

            #region Musée de l'Orangerie
            request = new("https://www.google.fr/maps/place/Mus%C3%A9e+de+l'Orangerie/@48.8637884,2.3226724,17z/data=!3m1!4b1!4m6!3m5!1s0x47e66e2eeaaaaaa3:0xdc3fd08aa701960a!8m2!3d48.8637884!4d2.3226724!16zL20vMGR0M21s?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Musée de l'Orangerie" ||
                (profile.GoogleAddress != "Jardin des Tuileries, 75001 Paris" && profile.GoogleAddress != "Jardin des Tuileries, 75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.Category != "Musée d'art" ||
                profile.Website != "https://www.musee-orangerie.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipONXsHpu5d_TUKzMzGYv4nQECCvQvNYQA7iwMJz=w408-h270-k-no" ||
                profile.PlusCode != "8FW4V87F+G3" ||
                profile.Tel != "01 44 50 43 00" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                museeOrangerie = false;
            #endregion

            #region Tour Eiffel
            request = new("https://www.google.fr/maps/place/Tour+Eiffel/@48.8583701,2.2944813,17z/data=!3m1!4b1!4m6!3m5!1s0x47e66e2964e34e2d:0x8ddca9ee380ef7e0!8m2!3d48.8583701!4d2.2944813!16zL20vMDJqODE?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Tour Eiffel" ||
                (profile.GoogleAddress != "Champ de Mars, 5 Av. Anatole France, 75007 Paris" && profile.GoogleAddress != "Champ de Mars, 5 Av. Anatole France, 75007 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75107" ||
                profile.Country != "France" ||
                profile.Category != "Site historique" ||
                profile.Website != "https://www.toureiffel.paris/fr" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipOSojyYuemoPo1TwH7J6mFC35Y89oKXMHgLIK7-=w408-h468-k-no" ||
                profile.PlusCode != "8FW4V75V+8Q" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                tourEiffel = false;
            #endregion

            #region Hôpital Necker
            request = new("https://www.google.fr/maps/place/H%C3%B4pital+Necker+AP-HP/@48.8452199,2.3157461,17z/data=!3m1!4b1!4m6!3m5!1s0x47e6703221308f89:0x57a7e5b303e7d9!8m2!3d48.8452199!4d2.3157461!16s%2Fm%2F03gzggj?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Hôpital Necker AP-HP" ||
                (profile.GoogleAddress != "149 Rue de Sèvres, 75015 Paris" && profile.GoogleAddress != "149 Rue de Sèvres, 75015 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75115" ||
                profile.Country != "France" ||
                profile.Category != "Hôpital pour enfants" ||
                profile.StreetNumber != "149" ||
                profile.Website != "http://www.hopital-necker.aphp.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipMnIqT5LZxbYjW8uj7PooQPlFrhQa_8HloZMmvz=w426-h240-k-no" ||
                profile.PlusCode != "8FW4R8W8+37" ||
                profile.Tel != "01 44 49 40 00" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                necker = false;
            #endregion

            #region Le Parc des Princes
            request = new("https://www.google.fr/maps/place/Le+Parc+des+Princes/@48.8414356,2.2530484,17z/data=!3m1!4b1!4m6!3m5!1s0x47e67ac09948a18d:0xdd2450406cef2c5c!8m2!3d48.8414356!4d2.2530484!16zL20vMDM5NXNs?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Le Parc des Princes" ||
                (profile.GoogleAddress != "24 Rue du Commandant Guilbaud, 75016 Paris" && profile.GoogleAddress != "24 Rue du Commandant Guilbaud, 75016 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75116" ||
                profile.Country != "France" ||
                profile.StreetNumber != "24" ||
                profile.Category != "Stade" ||
                profile.Website != "http://www.psg.fr/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNlF3ZAxh0AbfqGaAEHZKCuqbmDmVUbCEBoOdPa=w408-h254-k-no" ||
                profile.PlusCode != "8FW4R7R3+H6" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                parcDesPrinces = false;
            #endregion

            #region Banque de France
            request = new("https://www.google.fr/maps/place/Banque+de+France/@48.8642403,2.3395915,17z/data=!3m1!4b1!4m6!3m5!1s0x47e66e247d40c1cb:0xd9c85f8e2e769217!8m2!3d48.8642403!4d2.3395915!16s%2Fg%2F1w2yt5rt?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Banque de France" ||
                (profile.GoogleAddress != "31 Rue Croix des Petits Champs, 75001 Paris" && profile.GoogleAddress != "31 Rue Croix des Petits Champs, 75001 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75101" ||
                profile.Country != "France" ||
                profile.StreetNumber != "31" ||
                profile.Category != "Banque" ||
                profile.Website != "https://www.banque-france.fr/la-banque-de-france/missions/protection-du-consommateur/surendettement.html" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipMUP0ngLIPjME6aQ2uynmPAMv5Sd9ldDFj9QcX2=w408-h272-k-no" ||
                profile.PlusCode != "8FW4V87Q+MR" ||
                profile.Tel != "01 42 92 42 92" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                banqueDeFrance = false;
            #endregion

            #region Roland-Garros
            request = new("https://www.google.fr/maps/place/Stade+Roland-Garros/@48.8459632,2.2525486,18z/data=!4m10!1m2!2m1!1sroland+garros!3m6!1s0x47e67ac59c999975:0x9d40606b40c66989!8m2!3d48.8459632!4d2.2538361!15sCg1yb2xhbmQgZ2Fycm9zWg8iDXJvbGFuZCBnYXJyb3OSAQ5zcG9ydHNfY29tcGxleOABAA!16zL20vMDlodHQy?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Stade Roland-Garros" ||
                (profile.GoogleAddress != "2 Av. Gordon Bennett, 75016 Paris" && profile.GoogleAddress != "2 Av. Gordon Bennett, 75016 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75116" ||
                profile.Country != "France" ||
                profile.StreetNumber != "2" ||
                profile.Category != "Complexe sportif" ||
                profile.Website != "http://www.rolandgarros.com/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipMjz4HbqL38hD7PWlS7FvCP39ko48rW75Z83djk=w408-h272-k-no" ||
                profile.PlusCode != "8FW4R7W3+9G" ||
                profile.Tel != "01 47 43 48 00" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                rolandGarros = false;
            #endregion

            #region Maxim's
            request = new("https://www.google.fr/maps/place/Maxim's/@48.8674428,2.3032712,15z/data=!4m10!1m2!2m1!1smaxim's!3m6!1s0x47e66fcd4e473877:0xcf18f93e84c578c5!8m2!3d48.8674428!4d2.3223256!15sCgdtYXhpbSdzWgkiB21heGltJ3OSAQpyZXN0YXVyYW504AEA!16zL20vMDl6a3Ix?hl=fr&entry=ttu");
            (profile, score) = await scanner.GetBusinessProfileAndScoreFromGooglePageAsync(driver, request, null);
            if (profile.Name != "Maxim's" ||
                (profile.GoogleAddress != "3 Rue Royale, 75008 Paris" && profile.GoogleAddress != "3 Rue Royale, 75008 Paris, France") ||
                profile.City != "Paris" ||
                profile.CityCode != "75108" ||
                profile.Country != "France" ||
                profile.StreetNumber != "3" ||
                profile.Category != "Restaurant" ||
                profile.Website != "https://restaurant-maxims.com/" ||
                profile.Status != BusinessStatus.OPERATIONAL ||
                profile.PictureUrl != "https://lh5.googleusercontent.com/p/AF1QipNXcE3vwR3faD97mBv7BmuA_G2A2O0a07QpCT8_=w408-h272-k-no" ||
                profile.PlusCode != "8FW4V88C+XW" ||
                profile.Tel != "01 42 65 27 94" ||
                score.NbReviews == null ||
                score.Score <= 1 ||
                score.Score >= 5 ||
                score.Score == null)
                maxim = false;
            #endregion

            driver.Dispose();
        }
        /// <summary>
        /// Exporting Hotels info.
        /// </summary>
        [TestMethod]
        public void SetProcessingFromIdEtabFile()
        {
            string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\GMB.Sdk.Core\Files\Custom.txt";
            using DbLib db = new();

            List<string> values = [];

            using (StreamReader reader = new(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                        values.Add(line);
                }
            }

            foreach (string idEtab in values)
            {
                db.UpdateBusinessProfileProcessingState(idEtab, 9);
            }
        }
        /// <summary>
        /// Starting Scanner.
        /// </summary>
        [TestMethod]
        public void LaunchBusinessScanner()
        {
            AuthorizationPolicyService policy = new();
            ScannerController scannerController = new(policy);
            BusinessScannerRequest request = new(100000, 7, Operation.PROCESSING_STATE, true, DateTime.UtcNow.AddMonths(12), false);

            Task.Run(() => scannerController.StartBusinessScannerAsync(request)).Wait();
            return;
        }
        #endregion
    }
}