using BancoLosPatitos.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BancoLosPatitos.Filtros
{
    // Exige que el usuario autenticado exista en la tabla Usuarios.
    // Permite libremente Home(Inicio, About, Contact) y Account(Login/Logout).
    public class RequireRegisteredUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext.HttpContext;

            // Rutas que SIEMPRE se permiten
            string controller = (string)filterContext.RouteData.Values["controller"];
            string action = (string)filterContext.RouteData.Values["action"];
            string[] publicos =
            {
                "Home.Index", "Home.About", "Home.Contact",
                "Account.Login", "Account.LogOff", "Account.Register"
            };
            string ruta = $"{controller}.{action}";
            if (publicos.Contains(ruta, StringComparer.OrdinalIgnoreCase))
                return;

            // Si no está autenticado, deja que Authorize se encargue
            if (!ctx.User.Identity.IsAuthenticated)
                return;

            // Admin no necesita estar en tabla Usuarios
            if (ctx.User.IsInRole("Administrador"))
                return;

            // Verifica que esté en tu tabla Usuarios (activo)
            var userId = ctx.User.Identity.GetUserId();
            Guid guid;
            Guid.TryParse(userId, out guid);

            using (var db = new PatitosContext())
            {
                bool existe = db.Usuarios.Any(u => u.IdNetUser == guid && u.Estado == 1);
                if (!existe)
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    filterContext.Controller.TempData["NoRegistrado"] =
                        "No estás registrado en ningún comercio. Contacta con el administrador.";
                    filterContext.Result = new RedirectResult(urlHelper.Action("Index", "Home"));
                }
            }
        }
    }
}
