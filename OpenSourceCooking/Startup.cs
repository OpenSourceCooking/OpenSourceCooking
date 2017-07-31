using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(OpenSourceCooking.Startup))]
namespace OpenSourceCooking
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuthentication(app);
        }
    }
}
