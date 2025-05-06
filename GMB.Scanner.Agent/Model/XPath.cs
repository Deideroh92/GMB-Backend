using OpenQA.Selenium;

namespace GMB.Sdk.Core.Types.Models
{
    public class XPathDriver
    {
        public static readonly List<By> acceptCookies = [By.XPath("//button[@aria-label='Tout accepter']")];
    }
    public class XPathUrl
    {
        // For Url finder Agent
        public static readonly List<By> businessList = [By.XPath("//div[contains(@jsaction, 'mouseover:pane')]")];
        public static readonly List<By> body = [By.XPath("//div[contains(@aria-label, 'Résultats pour')]")];
        public static readonly List<By> endOfList = [By.XPath("//span[text() = 'Vous êtes arrivé à la fin de la liste.']")];
    }

    public class XPathProfile
    {

        // Profile info
        public static readonly List<By> name = [By.XPath("//div[@role='main' and @aria-label]")];
        public static readonly List<By> category = [By.XPath("//button[contains(@jsaction, 'pane.') and contains(@jsaction, '.category')]"), By.XPath("//button[@jsaction='pane.rating.category']"), By.XPath("//div[contains(@jsaction, 'pane.rating.moreReviews')]/following-sibling::span")];
        public static readonly List<By> adress = [By.XPath("//button[@data-item-id='address']")];
        public static readonly List<By> nbReviews = [By.XPath("//button[contains(@jsaction, 'pane.rating.moreReviews')]"), By.XPath(".//span[contains(@arial-label, 'avis')]"), By.XPath("//button[contains(@jsaction, '.reviewChart.moreReviews')]")];
        public static readonly List<By> tel = [By.XPath("//button[contains(@aria-label, 'Numéro de téléphone:')]")];
        public static readonly List<By> website = [By.XPath("//button[contains(@aria-label, 'Site Web:')]"), By.XPath("//a[contains(@aria-label, 'Site Web:')]")];
        public static readonly List<By> score = [By.XPath("//span[substring(@aria-label, string-length(@aria-label) - string-length('étoiles ') + 1) = 'étoiles ' and contains(@role, 'img')]\r\n"), By.XPath("//button[contains(@jsaction, 'pane.reviewChart.moreReviews')]")];
        public static readonly List<By> status = [By.XPath("//div[contains(@jsaction, 'pane.openhours')]")];
        public static readonly List<By> globalScore = [By.XPath("//div[contains(@jsaction, 'pane.rating.moreReviews')]")];
        public static readonly List<By> img = [By.XPath("//img[contains(@decoding, 'async')]")];
        public static readonly List<By> plusCode = [By.XPath("//button[contains(@aria-label, 'Plus\u00A0code:')]"), By.XPath("//a[contains(@aria-label, 'Plus\u00A0code:')]")];
        public static readonly List<By> locatedIn = [By.XPath("//button[@data-item-id='locatedin']")];
        public static readonly List<By> hasOpeningHours = [By.XPath("//button[contains(@jsaction, 'pane.openhours')]"), By.XPath("//span[@aria-label='Horaires']")];

        // Plus Code
        public static readonly List<By> expand = [By.XPath("//div[contains(@class, 'expand sprite-bg')]")];
        public static readonly List<By> coordinates = [By.XPath("//div[contains(@class, 'latlng')]")];
        public static readonly List<By> shortPlusCode = [By.XPath("//div[@class = 'short-code']")];
        public static readonly List<By> shortPlusCodeLocality = [By.XPath("//div[@class = 'locality']")];
        public static readonly List<By> longPlusCode = [By.XPath("//div[contains(@class, 'detail full-code')]")];
        public static readonly List<By> longPlusCodeArea = [By.XPath(".//span")];
        public static readonly List<By> longPlusCodeLocal = [By.XPath("following-sibling::span")];

        // Hotels
        public static readonly List<By> optionsOn = [By.XPath("//div[contains(@aria-label, 'est disponible')]")];
        public static readonly List<By> hotelScore = [By.XPath("//div[contains(@jsan, 'fontDisplayLarge') and @class='fontDisplayLarge']")];


        // Place ID
        public static readonly List<By> placeId = [By.XPath("//script[contains(text(), 'https://search.google.com/local/reviews?placeid')]")];
    }

    public class XPathReview
    {
        // Getting to review page
        public static readonly List<By> toReviewsPage = [By.XPath("//button[@role='tab' and contains(@aria-label, 'Avis')]")];
        public static readonly List<By> sortReviews = [By.XPath("//button[@data-value='Trier']")];
        public static readonly List<By> sortReviews2 = [By.XPath("//li[@data-index='1']"), By.XPath("//div[@data-index='1']")];
        public static readonly List<By> reviewList = [By.XPath("//div[@data-review-id and @aria-label]")];
        public static readonly List<By> scrollingPanel = [By.XPath("//button[@aria-label='Rédiger un avis']")];

        // Review info
        public static readonly List<By> googleDate = [By.XPath(".//span[contains(text(), 'il y a')]")];
        public static readonly List<By> userName = [By.XPath(".//a[contains(@aria-label, 'Photo de')]"), By.XPath(".//button[contains(@aria-label, 'Photo de')]")];
        public static readonly List<By> score = [By.XPath(".//span[contains(@role, 'img') and contains(@aria-label, 'étoile')]")];
        public static readonly List<By> userNbReviews = [By.XPath(".//div[contains(text(), 'avis')]")];
        public static readonly List<By> text = [By.XPath(".//div[contains(@class, 'MyEned')]"), By.XPath(".//div[contains(text(), 'Visité en')]/preceding-sibling::div[1]")];
        public static readonly List<By> visitDate = [By.XPath(".//div[contains(@class, 'MyEned')]/parent::*/following-sibling::div"), By.XPath(".//div[contains(text(), 'Visité en')]")];
        public static readonly List<By> plusButton = [By.XPath(".//button[contains(@jsaction, 'review.expandReview')]"), By.XPath(".//button[@aria-label='Voir plus']")];

        // Review reply info
        public static readonly List<By> replyText = [By.XPath(".//span[contains(text(), 'Réponse du propriétaire')]/following::div")];
        public static readonly List<By> replyGoogleDate = [By.XPath(".//span[contains(text(), 'Réponse du propriétaire')]/following::span")];

        // Hotels
        public static readonly List<By> hotelSortGoogleReviews = [By.XPath("//button[@aria-label='Tous les avis']")];
        public static readonly List<By> hotelSortPress = [By.XPath("//div[@data-index='1']")];
        public static readonly List<By> hotelSortReviews = [By.XPath("//button[@aria-label='Avis les plus pertinents']")];
        public static readonly List<By> hotelScore = [By.XPath(".//span[contains(text(), 'il y a')]/preceding-sibling::span")];
    }
}
