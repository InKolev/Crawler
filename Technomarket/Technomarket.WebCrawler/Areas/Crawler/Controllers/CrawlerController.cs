using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;

namespace Technomarket.WebCrawler.Areas.Crawler.Controllers
{
    public class CrawlerController : Controller
    {
        private const string TechnomarketBaseURL = "http://www.technomarket.bg";
        // GET: Crawler/Crawler
        public async Task<ActionResult> Index()
        {
            var httpClient = new HttpClient();
            //httpClient.GetAsync("http://www.technomarket.bg/computri");

            var result = await httpClient.GetAsync("http://www.technomarket.bg/product/filter?filter_form%5Bsort%5D=default&filter_form%5Bprice%5D%5Bmin%5D=419&filter_form%5Bprice%5D%5Bmax%5D=4899&filter_form%5Bspec_laptop_hdd%5D%5Bmin%5D=&filter_form%5Bspec_laptop_hdd%5D%5Bmax%5D=&filter_key=%2Fcomputri%7Cstatic%7Cstatic&from=0&size=200");
            var resultContent = await result.Content.ReadAsStringAsync();

            var document = new HtmlDocument();
            document.Load(new StringReader(resultContent));

            var productsNodes = document.DocumentNode.SelectNodes("//figure[@class='product']");
            foreach(var product in productsNodes)
            {
                var relativeProductUrl = product.SelectSingleNode("//a[@class='product-thumb']").Attributes["href"].Value;
                var fullProductUrl = $"{TechnomarketBaseURL}{relativeProductUrl}";
                var productName = product.SelectSingleNode("//span[@itemprop='name']").InnerText;
                var BLA = product.SelectSingleNode("//ul[@class='product-description']").SelectNodes("//li");

                // Build product document
                var productDetailsResult = await httpClient.GetAsync(fullProductUrl);
                var productDetailsResultAsString = await productDetailsResult.Content.ReadAsStringAsync();
                var innerDocument = new HtmlDocument();
                innerDocument.Load(new StringReader(productDetailsResultAsString));

                // Start parsing the product document
                var productDetailsNode = innerDocument.DocumentNode;
                var productImageSrc = productDetailsNode.SelectSingleNode("//img[@itemprop='image']").Attributes["src"].Value ;
                var x = productDetailsNode.SelectSingleNode("//ul[@class='product-description']");
                var productDescriptionItems = x?.SelectNodes("//li");

                var productDescriptionBuilder = new StringBuilder();
                foreach (var description in BLA)
                {
                    productDescriptionBuilder.AppendLine(description.InnerText);
                }

                var productDescription = productDescriptionBuilder.ToString();
            }

            return this.View();
        }
    }
}