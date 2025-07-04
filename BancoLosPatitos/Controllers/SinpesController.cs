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
            // Valida si hay una caja existente
            var cajaDestino = db.Cajas.FirstOrDefault(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario);

            if (cajaDestino == null)
            {
                ModelState.AddModelError("TelefonoDestinatario", "No existe una caja con este número de teléfono.");
            }
            // caja inactiva
            else if (cajaDestino.Estado != 1) 
            {
                ModelState.AddModelError("TelefonoDestinatario", "No se puede realizar pagos hacia una caja inactiva.");
            }

            if (ModelState.IsValid)
            {
                sinpe.Estado = 0;
                sinpe.FechaDeRegistro = DateTime.Now;
                db.Sinpes.Add(sinpe);
                db.SaveChanges();
                return RedirectToAction("Index");
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
    }
}
