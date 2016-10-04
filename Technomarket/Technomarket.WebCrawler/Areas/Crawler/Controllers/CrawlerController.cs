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
using Technomarket.WebCrawler.Areas.Crawler.Models;

namespace Technomarket.WebCrawler.Areas.Crawler.Controllers
{
    public class CrawlerController : Controller
    {
        private const string TechnomarketBaseURL = "http://www.technomarket.bg";

        // GET: Crawler/Crawler
        public async Task<ActionResult> Index()
        {
            var computersDetailsResult = await this.DoWhatYouDoBest("http://www.technomarket.bg/product/filter?filter_form%5Bsort%5D=default&filter_form%5Bprice%5D%5Bmin%5D=419&filter_form%5Bprice%5D%5Bmax%5D=4899&filter_form%5Bspec_laptop_hdd%5D%5Bmin%5D=&filter_form%5Bspec_laptop_hdd%5D%5Bmax%5D=&filter_key=%2Fcomputri%7Cstatic%7Cstatic&from=0&size=100");
            var laptopDetailsResult = await this.DoWhatYouDoBest("http://www.technomarket.bg/product/filter?filter_form%5Bsort%5D=default&filter_form%5Bprice%5D%5Bmin%5D=359&filter_form%5Bprice%5D%5Bmax%5D=5586&filter_form%5Bspec_tv_screen%5D%5Bmin%5D=&filter_form%5Bspec_tv_screen%5D%5Bmax%5D=&filter_form%5Bspec_laptop_hdd%5D%5Bmin%5D=&filter_form%5Bspec_laptop_hdd%5D%5Bmax%5D=&filter_form%5Bspec_laptop_wg%5D%5Bmin%5D=&filter_form%5Bspec_laptop_wg%5D%5Bmax%5D=&filter_key=%2Flaptopi%7Cstatic%7Cstatic&from=0&size=100");
            var monitorsDetailsResult = await this.DoWhatYouDoBest("http://www.technomarket.bg/product/filter?filter_form%5Bsort%5D=default&filter_form%5Bprice%5D%5Bmin%5D=139&filter_form%5Bprice%5D%5Bmax%5D=749&filter_form%5Bspec_tv_screen%5D%5Bmin%5D=&filter_form%5Bspec_tv_screen%5D%5Bmax%5D=&filter_key=%2Fmonitori%7Cstatic%7Cstatic&from=0&size=100");
            //var laptopDetails = this.DoWhatYouDoBest("");

            var computerDetails = computersDetailsResult;
            var laptopDetails = laptopDetailsResult;
            var monitorsDetails = computersDetailsResult;

            var results = new List<IEnumerable<ProductDetails>>();
            results.Add(computerDetails);
            results.Add(laptopDetails);
            results.Add(monitorsDetails);

            return this.View(results);
        }

        private string FilterText(string text)
        {
            if (text.Contains("&quot;"))
            {
                text = text.Replace("&quot;", "\"");
            }

            if (text.Contains("&#039;"))
            {
                text = text.Replace("&#039;", "\'");
            }

            if (text.Contains("&amp;"))
            {
                text = text.Replace("&amp;", "&");
            }

            return text;
        }

        private async Task<IEnumerable<ProductDetails>> DoWhatYouDoBest(string url)
        {
            var httpClient = new HttpClient();
            var result = await httpClient.GetAsync(url);
            var resultContent = await result.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.Load(new StringReader(resultContent));

            var productsDetails = new List<ProductDetails>();
            var productsNodes = document.DocumentNode.SelectNodes("//figure[@class='product']");
            foreach (var product in productsNodes)
            {
                var relativeProductUrl = product.SelectSingleNode(".//a[@class='product-thumb']")?.Attributes["href"]?.Value;
                var fullProductUrl = $"{TechnomarketBaseURL}{relativeProductUrl}";
                var productName = product.SelectSingleNode(".//span[@itemprop='name']").InnerText;
                productName = this.FilterText(productName);



                // get price




                // Build product document
                var productDetailsResult = await httpClient.GetAsync(fullProductUrl);
                var productDetailsResultAsString = await productDetailsResult.Content.ReadAsStringAsync();
                var innerDocument = new HtmlDocument();
                innerDocument.Load(new StringReader(productDetailsResultAsString));

                // Start parsing the product document
                var productDetailsNode = innerDocument.DocumentNode;
                var productImageSrc = productDetailsNode.SelectSingleNode("//img[@itemprop='image']").Attributes["src"].Value;
                var a = productDetailsNode.SelectSingleNode("//ul[contains(@itemprop,'description')]");
                var descriptionItems = a?.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element);

                var descriptionItemsList = new List<string>();
                foreach (var descriptionItem in descriptionItems)
                {
                    var descriptionItemText = descriptionItem.InnerText.Trim();
                    if (descriptionItemText != "виж повече")
                    {
                        descriptionItemText = this.FilterText(descriptionItemText);
                        descriptionItemsList.Add(descriptionItemText);
                    }
                }

                //section class="product-specifications"
                var specificationsItems = new List<Specification>();
                var specificationsElement = productDetailsNode.SelectSingleNode("//section[contains(@class,'product-specifications')]");
                if (specificationsElement != null)
                {
                    var specificationsItemsHeaders = specificationsElement.SelectNodes("//th");
                    var specificationsItemsValues = specificationsElement.SelectNodes("//td");

                    for (int i = 0; i < specificationsItemsHeaders.Count; i++)
                    {
                        specificationsItems.Add(new Specification
                        {
                            SpecificationName = specificationsItemsHeaders[i].InnerText,
                            SpecificationValue = specificationsItemsValues[i].InnerText
                        });
                    }
                }

                // Build final product details content
                var productDetailsToAdd =
                    new ProductDetails
                    {
                        ProductName = productName,
                        DescriptionItems = descriptionItemsList,
                        ProductImageUrl = productImageSrc
                    };

                if (specificationsElement != null)
                {
                    productDetailsToAdd.SpecificationItems = specificationsItems;
                }

                productsDetails.Add(productDetailsToAdd);
            }

            return productsDetails;
        }
    }
}