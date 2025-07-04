using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BancoLosPatitos.Models;

namespace BancoLosPatitos.Controllers
{
    public class CajasController : Controller
    {
        private PatitosContext db = new PatitosContext();

        // GET: Cajas
        public ActionResult Index(int? idComercio)
        {
            var cajas = db.Cajas.Include(c => c.Comercio);

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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Caja caja = db.Cajas.Find(id);
            if (caja == null)
            {
                return HttpNotFound();
            }
            return View(caja);
        }

        // GET: Cajas/Create
        public ActionResult Create()
        {
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion");
            return View();
        }

        // POST: Cajas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdCaja,IdComercio,Nombre,Descripcion,TelefonoSINPE,FechaDeRegistro,FechaDeModificacion,Estado")] Caja caja)
        {
            if (ModelState.IsValid)
            {
                // Revisa si hay nombre repetido
                bool existeNombre = db.Cajas.Any(c => c.Nombre == caja.Nombre && c.IdComercio == caja.IdComercio);

                // Revisa si hay un teléfono repetido
                bool existeTelefono = db.Cajas.Any(c => c.TelefonoSINPE == caja.TelefonoSINPE);

                if (existeNombre)
                {
                    ModelState.AddModelError("Nombre", "Ya existe una caja con este nombre para este comercio.");
                }

                if (existeTelefono)
                {
                    ModelState.AddModelError("TelefonoSINPE", "Ya existe una caja registrada con este número de SINPE.");
                }

                if (!existeNombre && !existeTelefono)
                {
                    caja.FechaDeRegistro = DateTime.Now;
                    caja.FechaDeModificacion = DateTime.Now;
                    db.Cajas.Add(caja);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", caja.IdComercio);
            return View(caja);
        }

        // GET: Cajas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Caja caja = db.Cajas.Find(id);
            if (caja == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", caja.IdComercio);
            return View(caja);
        }

        // POST: Cajas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCaja,IdComercio,Nombre,Descripcion,TelefonoSINPE,FechaDeRegistro,Estado")] Caja caja)
        {
            if (ModelState.IsValid)
            {
                // Obtener la caja existente
                var cajaActual = db.Cajas.Find(caja.IdCaja);
                if (cajaActual == null) return HttpNotFound();

                cajaActual.Nombre = caja.Nombre;
                cajaActual.Descripcion = caja.Descripcion;
                cajaActual.TelefonoSINPE = caja.TelefonoSINPE;
                cajaActual.Estado = caja.Estado;

                //Registra fecha de modificación automatico
                cajaActual.FechaDeModificacion = DateTime.Now;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", caja.IdComercio);
            return View(caja);
        }

        // GET: Cajas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Caja caja = db.Cajas.Find(id);
            if (caja == null)
            {
                return HttpNotFound();
            }
            return View(caja);
        }

        // POST: Cajas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Caja caja = db.Cajas.Find(id);
            db.Cajas.Remove(caja);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult VerSinpes(string telefono)
        {
            if (string.IsNullOrEmpty(telefono))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var sinpes = db.Sinpes
                .Where(s => s.TelefonoDestinatario == telefono)
                .OrderByDescending(s => s.FechaDeRegistro)
                .ToList();

            return View("~/Views/Sinpes/Index.cshtml", sinpes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
