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
        public void HomePage_Title_IsCorrect()
        {
            // Verifică dacă titlul paginii principale este cel așteptat
            Assert.AreEqual("OpenCart - Open Source Shopping Cart Solution", driver.Title);
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

        [TestCleanup]
        public void Cleanup()
        {
            driver.Quit();
        }
    }

}
