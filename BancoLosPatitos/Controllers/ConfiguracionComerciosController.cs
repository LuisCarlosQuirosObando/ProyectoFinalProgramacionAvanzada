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
    public class ConfiguracionComerciosController : Controller
    {
        private PatitosContext db = new PatitosContext();

        // GET: ConfiguracionComercios
        public ActionResult Index()
        {
            var configuracionComercios = db.ConfiguracionComercios.Include(c => c.Comercio);
            return View(configuracionComercios.ToList());
        }

        // GET: ConfiguracionComercios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracionComercio configuracionComercio = db.ConfiguracionComercios.Find(id);
            if (configuracionComercio == null)
            {
                return HttpNotFound();
            }
            return View(configuracionComercio);
        }

        // GET: ConfiguracionComercios/Create
        public ActionResult Create()
        {
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion");
            return View();
        }

        // POST: ConfiguracionComercios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdConfiguracion,IdComercio,TipoConfiguracion,Comision,FechaDeRegistro,FechaDeModificacion,Estado")] ConfiguracionComercio configuracionComercio)
        {
            if (ModelState.IsValid)
            {
                db.ConfiguracionComercios.Add(configuracionComercio);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", configuracionComercio.IdComercio);
            return View(configuracionComercio);
        }

        // GET: ConfiguracionComercios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracionComercio configuracionComercio = db.ConfiguracionComercios.Find(id);
            if (configuracionComercio == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", configuracionComercio.IdComercio);
            return View(configuracionComercio);
        }

        // POST: ConfiguracionComercios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdConfiguracion,IdComercio,TipoConfiguracion,Comision,FechaDeRegistro,FechaDeModificacion,Estado")] ConfiguracionComercio configuracionComercio)
        {
            if (ModelState.IsValid)
            {
                db.Entry(configuracionComercio).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", configuracionComercio.IdComercio);
            return View(configuracionComercio);
        }

        // GET: ConfiguracionComercios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracionComercio configuracionComercio = db.ConfiguracionComercios.Find(id);
            if (configuracionComercio == null)
            {
                return HttpNotFound();
            }
            return View(configuracionComercio);
        }

        // POST: ConfiguracionComercios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ConfiguracionComercio configuracionComercio = db.ConfiguracionComercios.Find(id);
            db.ConfiguracionComercios.Remove(configuracionComercio);
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
