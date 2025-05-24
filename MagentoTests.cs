using System.Threading;
// using AC2025.TestData; // Dacă nu este folosit, poate fi comentat
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
            // Inițializăm wait aici pentru a-l putea folosi pe parcursul testelor
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30)); // Poți ajusta timpul de așteptare
            driver.Navigate().GoToUrl("https://www.opencart.com/");
        }
        [TestMethod]
        public void Subscribe_To_Newsletter()
        {
            string originalWindowHandle = string.Empty;
            try
            {
                // 1. Derulează și completează formularul inițial pe pagina OpenCart
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                // Așteaptă ca elementul #newsletter să fie vizibil înainte de a derula
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("newsletter")));
                js.ExecuteScript("document.getElementById('newsletter').scrollIntoView({behavior: 'smooth'});");

                // Așteaptă din nou vizibilitatea (scroll-ul poate dura puțin)
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("newsletter")));

                // Folosește un email unic pentru a evita probleme la re-abonări
                string testEmail = "darius" + "@gmail.com";
                driver.FindElement(By.XPath("//input[@name='newsletter']"))
                      .SendKeys(testEmail);

                // Salvează handle-ul ferestrei curente ÎNAINTE de click-ul care deschide noul tab
                originalWindowHandle = driver.CurrentWindowHandle;

                // Click pe butonul principal de subscribe de pe OpenCart
                driver.FindElement(By.XPath("//button[contains(@class,'subscribe')]")).Click();

                // Acest pas este din codul tău original. Uneori, MailChimp are un al doilea buton de confirmare
                // într-un pop-up sau formular intermediar. Dacă acest click deschide noul tab, e ok.
                // Dacă primul click a deschis deja noul tab, acest pas ar putea eșua sau ar trebui ajustat.
                // Să presupunem că acest al doilea click este cel care duce la noul tab sau confirmă acțiunea.
                try
                {
                    // Încearcă să dai click pe acest buton dacă apare.
                    // Dacă nu apare în 5 secunde, mergem mai departe.
                    WebDriverWait shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); // Mărit puțin timpul
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


                // 2. Așteaptă și comută pe noul tab de confirmare MailChimp
                // Așteaptă să apară cel puțin două handle-uri de fereastră (tab-ul original + cel nou)
                wait.Until(drv => drv.WindowHandles.Count > 1);

                // Găsește handle-ul noului tab (cel care NU este originalWindowHandle)
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

                driver.SwitchTo().Window(newWindowHandle); // Comută focusul driverului pe noul tab

                // 3. Verificări MINIME și STABILE pe pagina de confirmare MailChimp (noul tab)

                // Așteaptă ca URL-ul să conțină "list-manage.com" (conform screenshot-ului tău)
                wait.Until(ExpectedConditions.UrlContains("list-manage.com"));
                Assert.IsTrue(driver.Url.ToLower().Contains("list-manage.com"),
                    $"URL-ul paginii de confirmare '{driver.Url}' nu conține 'list-manage.com'.");

                // Verifică vizibilitatea mesajului principal de confirmare (cel mai stabil element)
                // Text din screenshot: "Your subscription to our list has been confirmed."
                IWebElement confirmationMessageElement = wait.Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("//*[normalize-space()='Your subscription to our list has been confirmed.']")));

                Assert.IsTrue(confirmationMessageElement.Displayed,
                    "Mesajul 'Your subscription to our list has been confirmed.' nu este vizibil pe noul tab.");

                // Dacă ajunge aici, înseamnă că am comutat pe noul tab și cel puțin o verificare a trecut.
                Console.WriteLine($"Abonarea pentru {testEmail} a fost confirmată cu succes pe noul tab: {driver.Url}");

                // Testul va fi "verde" dacă cele două Assert.IsTrue de mai sus trec.
            }
            catch (Exception ex)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                // Folosește Path.Combine pentru o cale corectă indiferent de sistemul de operare
                string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Newsletter_Error_{timestamp}.png");
                try
                {
                    if (driver != null) // Verifică dacă driverul nu e null
                    {
                        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(screenshotPath);
                        Console.WriteLine($"Screenshot salvat la: {screenshotPath}");
                    }
                }
                catch (Exception scEx)
                {
                    Console.WriteLine($"Eroare la salvarea screenshot-ului: {scEx.Message}");
                }
                // Adaugă și StackTrace pentru detalii complete
                Assert.Fail($"Abonarea la newsletter a eșuat: {ex.Message}. Screenshot: {screenshotPath}. StackTrace: {ex.StackTrace}");
            }
            finally
            {
                // 4. Închide noul tab și comută înapoi la tab-ul original (dacă s-a deschis unul nou și driver-ul există)
                if (driver != null && !string.IsNullOrEmpty(originalWindowHandle) && driver.WindowHandles.Count > 1)
                {
                    // Verifică dacă tab-ul curent este cel nou înainte de a-l închide
                    if (driver.CurrentWindowHandle != originalWindowHandle)
                    {
                        driver.Close(); // Închide tab-ul curent (cel nou)
                    }
                    // Comută înapoi la tab-ul original (verifică dacă mai există)
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