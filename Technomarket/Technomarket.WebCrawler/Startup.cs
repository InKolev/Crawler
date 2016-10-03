using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Technomarket.WebCrawler.Startup))]
namespace Technomarket.WebCrawler
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
