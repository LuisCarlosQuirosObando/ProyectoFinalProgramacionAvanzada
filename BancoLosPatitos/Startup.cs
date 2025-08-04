using BancoLosPatitos.App_Start;
using BancoLosPatitos.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(BancoLosPatitos.Startup))]

namespace BancoLosPatitos
{
    public partial class Startup
    {
        // Este es el método que OWIN busca
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app); // llama tu configuración ya existente
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
            });
        }
    }
}
