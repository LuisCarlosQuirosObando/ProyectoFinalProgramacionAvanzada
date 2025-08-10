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
            var comerciosConfigurados = db.ConfiguracionComercios
                .Select(c => c.IdComercio)
                .Distinct()
                .ToList();

            var comerciosDisponibles = db.Comercios
                .Where(c => !comerciosConfigurados.Contains(c.IdComercio))
                .ToList();

            ViewBag.IdComercio = new SelectList(comerciosDisponibles, "IdComercio", "Identificacion");

            ViewBag.TipoConfiguracion = new SelectList(new[]
            {
                new { Valor = 1, Nombre = "Plataforma" },
                new { Valor = 2, Nombre = "Externa" },
                new { Valor = 3, Nombre = "Ambas" }
            }, "Valor", "Nombre");

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
                configuracionComercio.FechaDeRegistro = DateTime.Now;
                configuracionComercio.FechaDeModificacion = DateTime.Now;
                configuracionComercio.Estado = 1;
                db.ConfiguracionComercios.Add(configuracionComercio);
                db.SaveChanges();

                Helpers.BitacoraHelper.RegistrarEvento(db, "ConfiguracionComercios", "Registrar", configuracionComercio);
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

            ViewBag.TipoConfiguraciones = new SelectList(new[]
            {
                 new { Valor = 1, Nombre = "Plataforma" },
                 new { Valor = 2, Nombre = "Externa" },
                 new { Valor = 3, Nombre = "Ambas" } 
             }, "Valor", "Nombre");

            ViewBag.Estados = new SelectList(new[]
            {
                 new { Valor = 1, Nombre = "Activo" },
                 new { Valor = 0, Nombre = "Inactivo" }
             }, "Valor", "Nombre");
            
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
                configuracionComercio.FechaDeModificacion = DateTime.Now;
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
