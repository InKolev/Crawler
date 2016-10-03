using System.Web.Mvc;

namespace Technomarket.WebCrawler.Areas.Crawler
{
    public class CrawlerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Crawler";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Crawler_default",
                "Crawler/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}