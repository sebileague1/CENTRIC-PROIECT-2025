using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Pentru ExpectedConditions
using System.Linq; // Necesar pentru .Last(), .Any()
using System.IO;   // Necesar pentru Path.Combine

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
        private WebDriverWait wait;

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            driver.Navigate().GoToUrl("https://www.opencart.com/");
        }
        [TestMethod]
        public void Subscribe_To_Newsletter()
        {
            string originalWindowHandle = string.Empty;
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("newsletter")));
                js.ExecuteScript("document.getElementById('newsletter').scrollIntoView({behavior: 'smooth'});");

                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("newsletter")));

                string testEmail = "darius" + "@gmail.com";
                driver.FindElement(By.XPath("//input[@name='newsletter']"))
                      .SendKeys(testEmail);

                originalWindowHandle = driver.CurrentWindowHandle;

                driver.FindElement(By.XPath("//button[contains(@class,'subscribe')]")).Click();
                try
                {
                    WebDriverWait shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    shortWait.Until(ExpectedConditions.ElementToBeClickable(By.Id("mc-embedded-subscribe"))).Click();
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("Butonul 'mc-embedded-subscribe' nu a apărut sau nu a fost necesar. Se continuă...");
                }
                catch (Exception exInner) when (exInner is ElementNotInteractableException || exInner is StaleElementReferenceException)
                {
                    Console.WriteLine($"Eroare la interacțiunea cu 'mc-embedded-subscribe': {exInner.Message}. Este posibil ca noul tab să se fi deschis deja.");
                }

                wait.Until(drv => drv.WindowHandles.Count > 1);

                string newWindowHandle = string.Empty;
                foreach (var handle in driver.WindowHandles)
                {
                    if (handle != originalWindowHandle)
                    {
                        newWindowHandle = handle;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(newWindowHandle))
                {
                    throw new Exception("Noul tab nu a putut fi găsit.");
                }

                driver.SwitchTo().Window(newWindowHandle); 

                wait.Until(ExpectedConditions.UrlContains("list-manage.com"));
                Assert.IsTrue(driver.Url.ToLower().Contains("list-manage.com"),
                    $"URL-ul paginii de confirmare '{driver.Url}' nu conține 'list-manage.com'.");

                IWebElement confirmationMessageElement = wait.Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("//*[normalize-space()='Your subscription to our list has been confirmed.']")));

                Assert.IsTrue(confirmationMessageElement.Displayed,
                    "Mesajul 'Your subscription to our list has been confirmed.' nu este vizibil pe noul tab.");

                Console.WriteLine($"Abonarea pentru {testEmail} a fost confirmată cu succes pe noul tab: {driver.Url}");

            }
            catch (Exception ex)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Newsletter_Error_{timestamp}.png");
                try
                {
                    if (driver != null)
                    {
                        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(screenshotPath);
                        Console.WriteLine($"Screenshot salvat la: {screenshotPath}");
                    }
                }
                catch (Exception scEx)
                {
                    Console.WriteLine($"Eroare la salvarea screenshot-ului: {scEx.Message}");
                }
                Assert.Fail($"Abonarea la newsletter a eșuat: {ex.Message}. Screenshot: {screenshotPath}. StackTrace: {ex.StackTrace}");
            }
            finally
            {
                if (driver != null && !string.IsNullOrEmpty(originalWindowHandle) && driver.WindowHandles.Count > 1)
                {
                    if (driver.CurrentWindowHandle != originalWindowHandle)
                    {
                        driver.Close();
                    }
                    if (driver.WindowHandles.Contains(originalWindowHandle))
                    {
                        driver.SwitchTo().Window(originalWindowHandle);
                    }
                }
            }
        }

        [TestMethod]
        public void Navigate_To_Features_Page()
        {
            driver.FindElement(By.LinkText("Features")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.TitleContains("Features"));
            Assert.IsTrue(driver.Title.Contains("Features"), "Pagina Features nu s-a deschis corect.");
        }

        [TestMethod]
        public void Verify_Demo_Link()
        {
            driver.FindElement(By.LinkText("Demo")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.UrlContains("demo"));
            Assert.IsTrue(driver.Url.Contains("demo"), "Link-ul Demo nu a navigat corect.");
        }

        [TestMethod]
        public void Download_OpenCart()
        {
            var downloadButton = driver.FindElement(By.LinkText("FREE DOWNLOAD"));
            Assert.IsTrue(downloadButton.Displayed, "Butonul Free Download nu este vizibil.");
            Assert.IsTrue(downloadButton.Enabled, "Butonul Free Download nu este clicabil.");
            downloadButton.Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.UrlContains("download"));

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