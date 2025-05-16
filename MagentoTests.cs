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
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

                // Derulează la secțiunea newsletter
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("document.getElementById('newsletter').scrollIntoView();");

                // Așteaptă încărcarea secțiunii newsletter
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("newsletter")));

                // Localizăm câmpul de email
                IWebElement emailInput = wait.Until(drv =>
                {
                    return drv.FindElement(By.XPath("//input[@name='newsletter' and @placeholder='Enter your email address']"));
                });

                // Generează un email unic
                string testEmail = "darius@gmail.com";
                emailInput.Clear();
                emailInput.SendKeys(testEmail);

                // Localizăm butonul de subscribe (iconița săgeată dreapta)
                IWebElement subscribeButton = wait.Until(drv =>
                {
                    return drv.FindElement(By.XPath("//button[contains(@class,'subscribe')]/i[contains(@class,'fa-angle-right')]"));
                });

                // Face click pe buton
                subscribeButton.Click();

                // Verifică mesajul de succes (adaptat la implementarea actuală)
                try
                {
                    IWebElement successMessage = wait.Until(drv =>
                    {
                        return drv.FindElement(By.XPath("//div[contains(@class,'alert-success') or contains(text(),'success')]"));
                    });
                    Assert.IsTrue(successMessage.Displayed, "Mesajul de succes nu este vizibil");
                }
                catch
                {
                    // Verifică alternativ în consolă sau alte elemente
                    IWebElement response = driver.FindElement(By.XPath("//*[contains(@class,'response')]"));
                    Assert.IsTrue(response.Text.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0,
                                "Nu s-a primit confirmarea abonării");
                    
                }
            }
            catch (Exception ex)
            {
                // Facem screenshot pentru depanare
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