using BancoLosPatitos.App_Start;
using BancoLosPatitos.Filtros;
using BancoLosPatitos.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BancoLosPatitos.Controllers
{
    [LoggingExceptionFilter]
    public class AccountController : Controller
    {
        // GET: Account
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        private ApplicationUserManager _userManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await UserManager.FindAsync(model.Email, model.Password);
            if (user != null)
            {
                await SignInAsync(user, model.RememberMe);
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(model);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string rol = null)
        {
            if (string.Equals(rol, "Cajero", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.DBCorreoCajero = DBCorreoCajero();
            }

            return View(new RegisterViewModel { Rol = rol });
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (model.Rol == "Cajero") ViewBag.DBCorreoCajero = DBCorreoCajero();
                return View(model);
            }

            if (model.Rol != "Administrador" && model.Rol != "Cajero")
            {
                ModelState.AddModelError("", "Rol inválido. Elija Administrador o Cajero.");
                if (model.Rol == "Cajero") ViewBag.DBCorreoCajero = DBCorreoCajero();
                return View(model);
            }

            if (model.Rol == "Cajero")
            {
                // 1) Debe existir en Usuarios como pendiente (sin IdNetUser)
                using (var dbNegocio = new PatitosContext())
                {
                    var existePendiente = dbNegocio.Usuarios.Any(u =>
                        u.CorreoElectronico == model.Email &&
                        u.Estado == 1 &&
                        u.IdNetUser == null);

                    if (!existePendiente)
                    {
                        ModelState.AddModelError("", "Para registrarte como Cajero debes existir previamente en la tabla Usuarios (pendiente de sincronizar).");
                        ViewBag.DBCorreoCajero = DBCorreoCajero();
                        return View(model);
                    }
                }

                // 2) ¿Ya existe en Identity?
                var yaExisteIdentity = await UserManager.FindByNameAsync(model.Email);
                if (yaExisteIdentity != null)
                {
                    if (!UserManager.IsInRole(yaExisteIdentity.Id, "Cajero"))
                    {
                        var addRoleRes = await UserManager.AddToRoleAsync(yaExisteIdentity.Id, "Cajero");
                        if (!addRoleRes.Succeeded)
                        {
                            AddErrors(addRoleRes);
                            ViewBag.DBCorreoCajero = DBCorreoCajero();
                            return View(model);
                        }
                    }

                    using (var dbNegocio = new PatitosContext())
                    {
                        var cajero = dbNegocio.Usuarios.First(u => u.CorreoElectronico == model.Email);
                        cajero.IdNetUser = Guid.Parse(yaExisteIdentity.Id); // ajusta si tu IdNetUser es string
                        cajero.FechaDeModificacion = DateTime.Now;
                        dbNegocio.SaveChanges();
                    }

                    await SignInAsync(yaExisteIdentity, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }

            // Crear usuario Identity
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                AddErrors(result);
                if (model.Rol == "Cajero") ViewBag.DBCorreoCajero = DBCorreoCajero();
                return View(model);
            }

            // Asignar rol
            var roleRes = await UserManager.AddToRoleAsync(user.Id, model.Rol);
            if (!roleRes.Succeeded)
            {
                await UserManager.DeleteAsync(user);
                AddErrors(roleRes);
                if (model.Rol == "Cajero") ViewBag.DBCorreoCajero = DBCorreoCajero();
                return View(model);
            }

            // Vincular IdNetUser si es CAJERO
            if (model.Rol == "Cajero")
            {
                using (var db = new PatitosContext())
                {
                    var cajero = db.Usuarios.First(u => u.CorreoElectronico == model.Email);
                    cajero.IdNetUser = Guid.Parse(user.Id); // ajusta si tu IdNetUser es string
                    cajero.FechaDeModificacion = DateTime.Now;
                    db.SaveChanges();
                }
            }

            await SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        /* ===================== HELPERS ===================== */

        // Devuelve la lista de correos de cajeros pendientes por sincronizar
        private static System.Collections.Generic.List<string> DBCorreoCajero()
        {
            using (var db = new PatitosContext())
            {
                return db.Usuarios
                    .Where(u => u.Estado == 1 && u.IdNetUser == null)
                    .OrderBy(u => u.CorreoElectronico)
                    .Select(u => u.CorreoElectronico)
                    .ToList();
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(
                user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }
    }
}
