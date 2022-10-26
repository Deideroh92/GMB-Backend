using OpenQA.Selenium;

namespace GMS.Sdk.Core.XPath {
    public class XPathDriver {
        public static readonly By businessList = By.XPath("//button[@aria-label='Tout accepter']");
    }
    public class XPathUrl {

    public static readonly By businessList = By.XPath("//div[contains(@jsaction, 'mouseover:pane')]");
    public static readonly By body = By.XPath("//div[contains(@aria-label, 'Résultats pour')]");
    public static readonly By endOfList = By.XPath("//span[text() = 'Vous êtes arrivé à la fin de la liste.']");
    }

    public class XPathProfile {

        // Profile infos
        public static readonly By name = By.XPath("//div[@role='main' and @aria-label]");
        public static readonly By category = By.XPath("//button[@jsaction='pane.rating.category']");
        public static readonly By adress = By.XPath("//button[@data-item-id='address']");
        public static readonly By nbReviews = By.XPath("//button[contains(@jsaction, 'pane.rating.moreReviews')]");
        public static readonly By nbReviews2 = By.XPath(".//span[contains(@arial-label, 'avis')]");
        public static readonly By tel = By.XPath("//button[contains(@aria-label, 'Numéro de téléphone:')]");
        public static readonly By website = By.XPath("//button[contains(@aria-label, 'Site Web:')]");
        public static readonly By website2 = By.XPath("//a[contains(@aria-label, 'Site Web:')]");
        public static readonly By score = By.XPath("//span[contains(@aria-label, 'étoiles')]");
        public static readonly By status = By.XPath("//div[contains(@jsaction, 'pane.openhours')]");
        public static readonly By globalScore = By.XPath("//div[contains(@jsaction, 'pane.rating.moreReviews')]");
    }

    public class XPathReview {

        public static readonly By toReviewsPage = By.XPath("//div[@jsaction=\'pane.rating.moreReviews\']");
        public static readonly By sortReviews = By.XPath("//button[@data-value='Trier']");
        public static readonly By sortReviews2 = By.XPath("//li[@data-index='1']");
        public static readonly By reviewList = By.XPath("//div[@jsaction=\'mouseover:pane.review.in;mouseout:pane.review.out\']");
        public static readonly By reviewList2 = By.XPath("//div[@tabindex='-1']");
        public static readonly By scrollingPanel = By.XPath("//button[@aria-label='Rédiger un avis']");

        // Review Infos
        public static readonly By googleDate = By.XPath(".//span[contains(text(), 'il y a')]");
        public static readonly By userName = By.XPath(".//a[contains(@aria-label, 'Photo de')]");
        public static readonly By score = By.XPath(".//span[contains(@role, 'img')]");
        public static readonly By userNbReviews = By.XPath(".//span[contains(text(), 'Guide')]" + "/following::span");
        public static readonly By text = By.XPath(".//div[contains(@class, 'MyEned')]");

        // Review reply infos
        public static readonly By replyText = By.XPath(".//span[contains(text(), 'Réponse du propriétaire')]/following::div");
        public static readonly By replyGoogleDate = By.XPath(".//span[contains(text(), 'Réponse du propriétaire')]/following::span");
    }
}
