using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace retaildata
{
    public class SiteConifg
    {
        public string SearchUrl;
        public string Name;
    }

    public class DataManager
    {
        public const string DATA_PATH = "data";
        public List<SiteConifg> sites;
        PhantomJSDriver driver;
        Random rand = new Random();

        public DataManager()
        {
            var service = PhantomJSDriverService.CreateDefaultService();
            service.SslProtocol = "tlsv1"; //"any" also works

            PhantomJSOptions options = new PhantomJSOptions();
            options.AddAdditionalCapability("phantomjs.page.settings.userAgent", "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");

            driver = new PhantomJSDriver(service, options);

            sites = new List<SiteConifg>();
            sites.Add(new SiteConifg
            {
                Name = "Amazon",
                SearchUrl = "https://www.amazon.com/s?field-keywords="
            });
            Directory.CreateDirectory(DATA_PATH);
        }

        public bool HasData(string upc)
        {
            return false;
        }

        private void RandomWait()
        {
            Thread.Sleep(rand.Next(1000));
        }

        private PhantomJSDriver GetUrlText(string url)
        {
            RandomWait();
            driver.Url = url;
            driver.Navigate();
            return driver;
        }

        public string GetRandomUPC()
        {
            var text = GetUrlText("https://www.upcdatabase.com/random_item.asp");
            var code = text.FindElement(By.XPath("//td[contains(text(), 'Description')]/parent::tr/td[position()=3]")).Text;


            return code;
        }

        public void GetData(string upc)
        {
            foreach (var site in sites)
            {
                try
                {
                    Dictionary<string, string> results = new Dictionary<string, string>();
                    driver.Url = "https://www.amazon.com/";
                    driver.Navigate();
                    RandomWait();
                    driver.FindElement(By.Id("twotabsearchtextbox")).Clear();
                    RandomWait();
                    driver.FindElement(By.Id("twotabsearchtextbox")).SendKeys(upc);
                    RandomWait();
                    driver.FindElement(By.ClassName("nav-search-submit")).FindElement(By.ClassName("nav-input")).Click();
                    RandomWait();
                    var elem = driver.FindElement(By.CssSelector("[class='a-link-normal s-access-detail-page  s-color-twister-title-link a-text-normal']"));
                    results.Add("Name", elem.Text);
                    results.Add("URL", elem.GetAttribute("href"));
                    driver.Url = results["URL"];
                    driver.Navigate();
                    RandomWait();
                    results.Add("Price", driver.FindElement(By.Id("priceblock_ourprice")).Text);
                    results.Add("ASIN", driver.FindElement(By.XPath("//th[contains(text(), 'ASIN')]/parent::tr/td")).Text);
                    Directory.CreateDirectory(Path.Combine(DATA_PATH, results["ASIN"]));
                    //Directory.CreateDirectory(Path.Combine(DATA_PATH, results["ASIN"], site.Name));
                    File.WriteAllLines(Path.Combine(DATA_PATH, results["ASIN"], "results.txt"), results.Select(kvp => string.Join("|", kvp.Key, kvp.Value)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}