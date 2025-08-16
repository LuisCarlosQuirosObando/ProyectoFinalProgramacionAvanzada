using BancoLosPatitos.Filtros;
using BancoLosPatitos.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace BancoLosPatitos.Controllers
{
    [LoggingExceptionFilter]
    [Authorize(Roles = "Administrador,Cajero")]
    public class CajasController : Controller
    {
        private PatitosContext db = new PatitosContext();

        /* ========= Helpers de seguridad para Cajero ========= */

        private bool EsCajero()
            => User.IsInRole("Cajero");

        private Guid NetUserId()
            => Guid.Parse(User.Identity.GetUserId()); // AspNetUsers.Id es string GUID

        // Comercios a los que pertenece el cajero
        private int[] ComerciosPermitidos()
        {
            var netId = NetUserId();
            return db.Usuarios
                     .Where(u => u.Estado == 1 && u.IdNetUser == netId)
                     .Select(u => u.IdComercio)
                     .Distinct()
                     .ToArray();
        }

        private bool CajeroPuedeComercio(int idComercio)
        {
            if (!EsCajero()) return true;
            var permitidos = ComerciosPermitidos();
            return permitidos.Contains(idComercio);
        }

        /* ======================= Actions ===================== */

        // GET: Cajas
        public ActionResult Index(int? idComercio)
        {
            var cajas = db.Cajas.Include(c => c.Comercio);

            if (EsCajero())
            {
                var permitidos = ComerciosPermitidos();
                cajas = cajas.Where(c => permitidos.Contains(c.IdComercio));

                if (idComercio.HasValue && !permitidos.Contains(idComercio.Value))
                    return new HttpStatusCodeResult(403);
            }

            if (idComercio.HasValue)
            {
                cajas = cajas.Where(c => c.IdComercio == idComercio.Value);
                ViewBag.FiltroComercio = db.Comercios
                    .Where(c => c.IdComercio == idComercio.Value)
                    .Select(c => c.Nombre)
                    .FirstOrDefault();
            }

            return View(cajas.ToList());
        }

        // GET: Cajas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var caja = db.Cajas.Find(id);
            if (caja == null) return HttpNotFound();

            if (!CajeroPuedeComercio(caja.IdComercio))
                return new HttpStatusCodeResult(403);

            return View(caja);
        }

        // GET: Cajas/Create
        public ActionResult Create()
        {
            if (EsCajero())
            {
                var permitidos = ComerciosPermitidos();
                ViewBag.IdComercio = new SelectList(
                    db.Comercios.Where(c => permitidos.Contains(c.IdComercio)),
                    "IdComercio", "Identificacion");
            }
            else
            {
                ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion");
            }

            return View();
        }

        // POST: Cajas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdCaja,IdComercio,Nombre,Descripcion,TelefonoSINPE,FechaDeRegistro,FechaDeModificacion,Estado")] Caja caja)
        {
            if (EsCajero() && !CajeroPuedeComercio(caja.IdComercio))
                return new HttpStatusCodeResult(403);

            if (ModelState.IsValid)
            {
                try
                {
                    bool existeNombre = db.Cajas.Any(c => c.Nombre == caja.Nombre && c.IdComercio == caja.IdComercio);
                    bool existeTelefono = db.Cajas.Any(c => c.TelefonoSINPE == caja.TelefonoSINPE);

                    if (existeNombre)
                        ModelState.AddModelError("Nombre", "Ya existe una caja con este nombre para este comercio.");

                    if (existeTelefono)
                        ModelState.AddModelError("TelefonoSINPE", "Ya existe una caja registrada con este número de SINPE.");

                    if (!existeNombre && !existeTelefono)
                    {
                        caja.FechaDeRegistro = DateTime.Now;
                        caja.FechaDeModificacion = DateTime.Now;
                        db.Cajas.Add(caja);
                        db.SaveChanges();

                        Helpers.BitacoraHelper.RegistrarEvento(db, "Cajas", "Registrar", caja);
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Cajas", ex);
                    ModelState.AddModelError("", "Ocurrió un error al crear caja.");
                }
            }

            // Re-cargar combo según rol
            if (EsCajero())
                ViewBag.IdComercio = new SelectList(
                    db.Comercios.Where(c => ComerciosPermitidos().Contains(c.IdComercio)),
                    "IdComercio", "Identificacion", caja.IdComercio);
            else
                ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", caja.IdComercio);

            return View(caja);
        }

        // GET: Cajas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var caja = db.Cajas.Find(id);
            if (caja == null) return HttpNotFound();

            if (!CajeroPuedeComercio(caja.IdComercio))
                return new HttpStatusCodeResult(403);

            if (EsCajero())
            {
                var permitidos = ComerciosPermitidos();
                ViewBag.IdComercio = new SelectList(
                    db.Comercios.Where(c => permitidos.Contains(c.IdComercio)),
                    "IdComercio", "Identificacion", caja.IdComercio);
            }
            else
            {
                ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", caja.IdComercio);
            }

            return View(caja);
        }

        // POST: Cajas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCaja,IdComercio,Nombre,Descripcion,TelefonoSINPE,FechaDeRegistro,Estado")] Caja caja)
        {
            var cajaActual = db.Cajas.Find(caja.IdCaja);
            if (cajaActual == null) return HttpNotFound();

            // Seguridad: el cajero no puede editar cajas fuera de sus comercios
            if (EsCajero() && !CajeroPuedeComercio(cajaActual.IdComercio))
                return new HttpStatusCodeResult(403);

            if (ModelState.IsValid)
            {
                try
                {
                    var datosAnteriores = db.Cajas.AsNoTracking().FirstOrDefault(c => c.IdCaja == caja.IdCaja);

                    // Si permites cambiar de comercio, valida:
                    if (EsCajero() && caja.IdComercio != cajaActual.IdComercio)
                        return new HttpStatusCodeResult(403);

                    cajaActual.Nombre = caja.Nombre;
                    cajaActual.Descripcion = caja.Descripcion;
                    cajaActual.TelefonoSINPE = caja.TelefonoSINPE;
                    cajaActual.Estado = caja.Estado;
                    cajaActual.FechaDeModificacion = DateTime.Now;

                    db.SaveChanges();

                    Helpers.BitacoraHelper.RegistrarEvento(db, "Cajas", "Modificar", datosAnteriores, caja, "");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Cajas", ex);
                    ModelState.AddModelError("", "Ocurrió un error al editar caja.");
                }
            }

            // Re-cargar combo según rol
            if (EsCajero())
                ViewBag.IdComercio = new SelectList(
                    db.Comercios.Where(c => ComerciosPermitidos().Contains(c.IdComercio)),
                    "IdComercio", "Identificacion", cajaActual.IdComercio);
            else
                ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", cajaActual.IdComercio);

            return View(caja);
        }

        // GET: Cajas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var caja = db.Cajas.Find(id);
            if (caja == null) return HttpNotFound();

            if (!CajeroPuedeComercio(caja.IdComercio))
                return new HttpStatusCodeResult(403);

            return View(caja);
        }

        // POST: Cajas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var caja = db.Cajas.Find(id);
            if (caja == null) return HttpNotFound();

            if (!CajeroPuedeComercio(caja.IdComercio))
                return new HttpStatusCodeResult(403);

            db.Cajas.Remove(caja);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult VerSinpes(string telefono)
        {
            if (string.IsNullOrEmpty(telefono))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Seguridad: solo sinpes de cajas de comercios permitidos
            if (EsCajero())
            {
                var permitidos = ComerciosPermitidos();
                bool pertenece = db.Cajas.Any(c => c.TelefonoSINPE == telefono && permitidos.Contains(c.IdComercio));
                if (!pertenece)
                    return new HttpStatusCodeResult(403);
            }

            var sinpes = db.Sinpes
                .Where(s => s.TelefonoDestinatario == telefono)
                .OrderByDescending(s => s.FechaDeRegistro)
                .ToList();

            return View("~/Views/Sinpes/Index.cshtml", sinpes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
