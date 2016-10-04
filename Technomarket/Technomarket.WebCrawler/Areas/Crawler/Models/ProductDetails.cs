using System.Collections.Generic;

namespace Technomarket.WebCrawler.Areas.Crawler.Models
{
    public class ProductDetails
    {
        public string ProductName { get; set; }

        public string ProductImageUrl { get; set; }

        public IEnumerable<string> DescriptionItems { get; set; }

        public IEnumerable<Specification> SpecificationItems { get; set; }
    }
}