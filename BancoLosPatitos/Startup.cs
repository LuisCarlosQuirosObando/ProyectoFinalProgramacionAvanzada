using BancoLosPatitos.App_Start;
using BancoLosPatitos.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
            CrearRolesYUsuarios(); // crea roles y usuarios al iniciar la aplicación
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


        private void CrearRolesYUsuarios()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Administrador"))
                roleManager.Create(new IdentityRole("Administrador"));
            if (!roleManager.RoleExists("Cajero"))
                roleManager.Create(new IdentityRole("Cajero"));

            string adminEmail = "admin@bancolospatitos.com";
            string adminPassword = "Admin@12345.";
            if (userManager.FindByName(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,

                };
                var result = userManager.Create(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Administrador");
                }
            }
        }


    }
}
