﻿using OpenQA.Selenium;

namespace GMS.Sdk.Core.Models
{
    public class XPathDriver
    {
        public static readonly List<By> acceptCookies = new() { By.XPath("//button[@aria-label='Tout accepter']") };
    }
    public class XPathUrl
    {

        public static readonly List<By> businessList = new() { By.XPath("//div[contains(@jsaction, 'mouseover:pane')]") };
        public static readonly List<By> body = new() { By.XPath("//div[contains(@aria-label, 'Résultats pour')]") };
        public static readonly List<By> endOfList = new() { By.XPath("//span[text() = 'Vous êtes arrivé à la fin de la liste.']") };
    }

    public class XPathProfile
    {

        // Profile infos
        public static readonly List<By> name = new() { By.XPath("//div[@role='main' and @aria-label]") };
        public static readonly List<By> category = new() { By.XPath("//button[@jsaction='pane.rating.category']") };
        public static readonly List<By> adress = new() { By.XPath("//button[@data-item-id='address']") };
        public static readonly List<By> nbReviews = new() { By.XPath("//button[contains(@jsaction, 'pane.rating.moreReviews')]"), By.XPath("//span[contains(@arial-label, 'avis')]"), By.XPath("//button[contains(@jsaction, 'pane.reviewChart.moreReviews')]") };
        public static readonly List<By> tel = new() { By.XPath("//button[contains(@aria-label, 'Numéro de téléphone:')]") };
        public static readonly List<By> website = new() { By.XPath("//button[contains(@aria-label, 'Site Web:')]"), By.XPath("//a[contains(@aria-label, 'Site Web:')]") };
        public static readonly List<By> score = new() { By.XPath("//span[contains(@aria-label, 'étoiles')]"), By.XPath("//button[contains(@jsaction, 'pane.reviewChart.moreReviews')]") };
        public static readonly List<By> status = new() { By.XPath("//div[contains(@jsaction, 'pane.openhours')]") };
        public static readonly List<By> globalScore = new() { By.XPath("//div[contains(@jsaction, 'pane.rating.moreReviews')]") };
        public static readonly List<By> test = new() { By.XPath("//img[contains(@decoding, 'async')]") };

        // Hotels
        public static readonly List<By> hotelCategory = new() { By.XPath("//div[contains(@jsaction, 'pane.rating.moreReviews')]/following-sibling::span") };
        public static readonly List<By> hotelScore = new() { By.XPath("//div[contains(@jsan, 'fontDisplayLarge') and @class='fontDisplayLarge']") };
        public static readonly List<By> optionsOn = new() { By.XPath("//div[contains(@aria-label, 'est disponible')]") };
    }

    public class XPathReview
    {

        public static readonly List<By> toReviewsPage = new() { By.XPath("//button[@role='tab' and contains(@aria-label, 'Avis')]") };
        public static readonly List<By> sortReviews = new() { By.XPath("//button[@data-value='Trier']") };
        public static readonly List<By> sortReviews2 = new() { By.XPath("//li[@data-index='1']"), By.XPath("//div[@data-index='1']") };
        public static readonly List<By> reviewList = new() { By.XPath("//div[@jsaction='mouseover:pane.review.in; mouseout:pane.review.out']") };
        public static readonly List<By> scrollingPanel = new() { By.XPath("//button[@aria-label='Rédiger un avis']") };

        // Review Infos
        public static readonly List<By> googleDate = new() { By.XPath(".//span[contains(text(), 'il y a')]") };
        public static readonly List<By> userName = new() { By.XPath(".//a[contains(@aria-label, 'Photo de')]"), By.XPath(".//button[contains(@aria-label, 'Photo de')]") };
        public static readonly List<By> score = new() { By.XPath(".//span[contains(@role, 'img')]//img[contains(@src, 'ic_star_rate_14')]") };
        public static readonly List<By> userNbReviews = new() { By.XPath(".//div[contains(text(), 'avis')]") };
        public static readonly List<By> text = new() { By.XPath(".//div[contains(@class, 'MyEned')]") };


        // Review reply infos
        public static readonly List<By> replyText = new() { By.XPath(".//span[contains(text(), 'Réponse du propriétaire')]/following::div") };
        //public static readonly List<By> replyGoogleDate = new() { By.XPath(".//span[contains(text(), 'Réponse du propriétaire')]/following::span") };
    }
}