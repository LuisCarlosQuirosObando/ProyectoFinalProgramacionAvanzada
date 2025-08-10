using BancoLosPatitos.Filtros;
using BancoLosPatitos.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BancoLosPatitos.Controllers
{
    [LoggingExceptionFilter]
    public class SinpesController : Controller
    {
        private PatitosContext db = new PatitosContext();

        // GET: Sinpes
        public ActionResult Index()
        {
            return View(db.Sinpes.ToList());
        }

        // GET: Sinpes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sinpe sinpe = db.Sinpes.Find(id);
            if (sinpe == null)
            {
                return HttpNotFound();
            }
            return View(sinpe);
        }

        // GET: Sinpes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sinpes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdSinpe,TelefonoOrigen,NombreOrigen,TelefonoDestinatario,NombreDestinatario,Monto,FechaDeRegistro,Descripcion,Estado")] Sinpe sinpe)
        {
            var cajaDestino = db.Cajas.FirstOrDefault(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario);
            if (cajaDestino == null)
            {
                ModelState.AddModelError("TelefonoDestinatario", "No existe una caja con este número de teléfono.");
            }
            else if (cajaDestino.Estado != 1) 
            {
                ModelState.AddModelError("TelefonoDestinatario", "No se puede realizar pagos hacia una caja inactiva.");
            }

            if (ModelState.IsValid)
            {
               try
                {
                sinpe.Estado = 0;
                sinpe.FechaDeRegistro = DateTime.Now;
                db.Sinpes.Add(sinpe);
                db.SaveChanges();

                Helpers.BitacoraHelper.RegistrarEvento(db, "SINPES", "Registrar", sinpe);

                    return RedirectToAction("VerSinpes", "Cajas", new { telefono = sinpe.TelefonoDestinatario }); 
                }

               catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Sinpes", ex);
                    ModelState.AddModelError("", "Ocurrió un error al crear SINPE.");
                }
            }

            return View(sinpe);
        }

        // GET: Sinpes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sinpe sinpe = db.Sinpes.Find(id);
            if (sinpe == null)
            {
                return HttpNotFound();
            }
            return View(sinpe);
        }

        // POST: Sinpes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdSinpe,TelefonoOrigen,NombreOrigen,TelefonoDestinatario,NombreDestinatario,Monto,FechaDeRegistro,Descripcion,Estado")] Sinpe sinpe)
        {
            if (ModelState.IsValid)
            {

                db.Entry(sinpe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sinpe);
        }

        // GET: Sinpes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sinpe sinpe = db.Sinpes.Find(id);
            if (sinpe == null)
            {
                return HttpNotFound();
            }
            return View(sinpe);
        }

        // POST: Sinpes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sinpe sinpe = db.Sinpes.Find(id);
            db.Sinpes.Remove(sinpe);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        [HttpPost]
        public ActionResult SincronizarSinpe(int id)
        {
            var sinpe = db.Sinpes.Find(id);
            if (sinpe != null && sinpe.Estado == 0)
            {
                sinpe.Estado = 1; // Cambiar a sincronizado
                db.SaveChanges();
                TempData["mensaje"] = "SINPE sincronizado correctamente.";
                return RedirectToAction("VerSinpes", "Cajas", new { telefono = sinpe.TelefonoDestinatario });
            }
            else
            {
                TempData["mensaje"] = "Este SINPE ya estaba sincronizado o no existe.";
                return RedirectToAction("VerSinpes", "Cajas", new { telefono = sinpe.TelefonoDestinatario });

            }
        }




    }
}
