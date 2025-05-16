using System.Threading;
using AC2025.TestData;

namespace AC2025
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using SeleniumExtras.WaitHelpers;
    using System.Linq;

    [TestClass]
    public class MagentoTests
    {
        private IWebDriver driver;

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://www.opencart.com/");
        }
        [TestMethod]
        public void Subscribe_To_Newsletter()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                // 1. Derulează și completează formularul inițial
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("document.getElementById('newsletter').scrollIntoView({behavior: 'smooth'});");

                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("newsletter")));

                string testEmail = "darius@gmail.com";
                driver.FindElement(By.XPath("//input[@name='newsletter']"))
                      .SendKeys(testEmail); 

                driver.FindElement(By.XPath("//button[contains(@class,'subscribe')]"))
                      .Click();

                // 2. Completează formularul secundar (dacă există)
                wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.Id("mc-embedded-subscribe"))).Click();

                // 3. Verificări pe pagina de confirmare MailChimp
                wait.Until(drv => drv.Url.Contains("subscription.perfector.com"));

                // Verifică titlul paginii
                IWebElement pageTitle = wait.Until(drv =>
                    drv.FindElement(By.XPath("//h1[contains(.,'subscription') or contains(.,'confirmed')]")));
                Assert.IsTrue(pageTitle.Text.IndexOf("confirmed", StringComparison.OrdinalIgnoreCase) >= 0,
                    "Titlul paginii nu confirmă abonarea");

                // Verifică mesajul principal
                IWebElement thankYouMessage = wait.Until(drv =>
                    drv.FindElement(By.XPath("//*[contains(text(),'Thank you for subscribing')]")));
                Assert.IsTrue(thankYouMessage.Displayed, "Mesajul de mulțumire lipsește");

                // Verifică mesajul secundar
                IWebElement updatesMessage = driver.FindElement(
                    By.XPath("//*[contains(text(),'Look out for news and updates')]"));
                Assert.IsTrue(updatesMessage.Displayed, "Mesajul despre noutăți lipsește");

                // Verifică butonul de continuare
                IWebElement continueButton = driver.FindElement(
                    By.XPath("//a[contains(.,'Continue to website')]"));
                Assert.IsTrue(continueButton.Displayed && continueButton.Enabled,
                    "Butonul 'Continue' nu este disponibil");

                // Verifică link-ul de gestionare a abonării
                IWebElement manageSubscription = driver.FindElement(
                    By.XPath("//a[contains(.,'Manage subscription preferences')]"));
                Assert.IsTrue(manageSubscription.Displayed, "Link-ul de gestionare lipsește");
                Assert.IsTrue(manageSubscription.GetAttribute("href").Contains("mailchimp"),
                    "Link-ul de gestionare nu pare corect");

                // Opțional: verifică prezența elementelor MailChimp în footer
                var mailchimpElements = driver.FindElements(
                    By.XPath("//*[contains(@class,'mailchimp') or contains(text(),'Mailchimp')]"));
                Assert.IsTrue(mailchimpElements.Count > 0, "Elementele MailChimp lipsesc");

                // Log pentru depanare
                Console.WriteLine($"Abonarea pentru {testEmail} a fost confirmată cu succes");
            }
            catch (Exception ex)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string screenshotPath = $"Newsletter_Error_{timestamp}.png";
                ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(screenshotPath);
                Assert.Fail($"Abonarea la newsletter a eșuat: {ex.Message}. Screenshot: {screenshotPath}");
            }
        }
        [TestMethod]
        public void Navigate_To_Features_Page()
        {
            // Navighează la pagina Features și verifică titlul
            driver.FindElement(By.LinkText("Features")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.TitleContains("Features"));
            Assert.IsTrue(driver.Title.Contains("Features"), "Pagina Features nu s-a deschis corect.");
        }

        [TestMethod]
        public void Verify_Demo_Link()
        {
            // Verifică dacă link-ul Demo funcționează și navighează corect
            driver.FindElement(By.LinkText("Demo")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.UrlContains("demo"));
            Assert.IsTrue(driver.Url.Contains("demo"), "Link-ul Demo nu a navigat corect.");
        }

        [TestMethod]
        public void Download_OpenCart()
        {
            // Navighează la butonul FREE DOWNLOAD și apoi apasă Download Now
            var downloadButton = driver.FindElement(By.LinkText("FREE DOWNLOAD"));
            Assert.IsTrue(downloadButton.Displayed, "Butonul Free Download nu este vizibil.");
            Assert.IsTrue(downloadButton.Enabled, "Butonul Free Download nu este clicabil.");
            downloadButton.Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.UrlContains("download"));

            // Caută butonul Download Now folosind alt textul imaginii
            var downloadNowButton = wait.Until(drv => drv.FindElement(By.XPath("//img[@alt='Download OpenCart now']/ancestor::a")));
            Assert.IsTrue(downloadNowButton.Displayed, "Butonul Download Now nu este vizibil.");
            Assert.IsTrue(downloadNowButton.Enabled, "Butonul Download Now nu este clicabil.");
            downloadNowButton.Click();

            wait.Until(ExpectedConditions.UrlContains("download"));
            Assert.IsTrue(driver.Url.Contains("download"), "Descărcarea nu a fost inițiată corect.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            driver.Quit();
        }
    }
}