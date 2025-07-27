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
    public class ReportesController : Controller
    {
        private PatitosContext db = new PatitosContext();

        // GET: Reportes
        public ActionResult Index()
        {
            var reportes = db.Reportes.Include(r => r.Comercio);
            return View(reportes.ToList());
        }

        // GET: Reportes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporte reporte = db.Reportes.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }
            return View(reporte);
        }

        // GET: Reportes/Create
        public ActionResult Create()
        {
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion");
            return View();
        }

        // POST: Reportes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdReporte,IdComercio,CantidadDeCajas,MontoTotalRecaudado,CantidadDeSINPES,MontoTotalComision,FechaDelReporte")] Reporte reporte)
        {
            if (ModelState.IsValid)
            {
                db.Reportes.Add(reporte);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", reporte.IdComercio);
            return View(reporte);
        }

        // GET: Reportes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporte reporte = db.Reportes.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", reporte.IdComercio);
            return View(reporte);
        }

        // POST: Reportes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdReporte,IdComercio,CantidadDeCajas,MontoTotalRecaudado,CantidadDeSINPES,MontoTotalComision,FechaDelReporte")] Reporte reporte)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reporte).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", reporte.IdComercio);
            return View(reporte);
        }

        // GET: Reportes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reporte reporte = db.Reportes.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }
            return View(reporte);
        }

        // POST: Reportes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reporte reporte = db.Reportes.Find(id);
            db.Reportes.Remove(reporte);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarReportes()
        {
            var fechaActual = DateTime.Now;
            var comercios = db.Comercios.ToList();
            try
            {

                foreach (var comercio in comercios)
                {
                    var cajas = db.Cajas.Where(c => c.IdComercio == comercio.IdComercio && c.Estado == 1).ToList();

                    int cantidadDeCajas = cajas.Count();

                    var telefonosCajas = cajas.Select(c => c.TelefonoSINPE).Distinct().ToList();

                    var sinpes = db.Sinpes.Where(s => telefonosCajas.Contains(s.TelefonoDestinatario)).ToList();

                    decimal montoRecaudado = sinpes.Sum(s => s.Monto);
                    int cantidadDeSINPES = sinpes.Count;

                    var configuracionComi = db.ConfiguracionComercios.FirstOrDefault(cc => cc.IdComercio == comercio.IdComercio && cc.Estado == 1);

                    decimal porcentajeComision = configuracionComi != null ? configuracionComi.Comision / 100m : 0m;
                    decimal montoComision = montoRecaudado * porcentajeComision;

                    var reporteExistente = db.Reportes.FirstOrDefault(r => r.IdComercio == comercio.IdComercio);

                    if (reporteExistente != null)
                    {
                            reporteExistente.CantidadDeCajas = cantidadDeCajas;
                            reporteExistente.MontoTotalRecaudado = montoRecaudado;
                            reporteExistente.CantidadDeSINPES = cantidadDeSINPES;
                            reporteExistente.MontoTotalComision = montoComision;
                            reporteExistente.FechaDelReporte = fechaActual; 
                    }
                    else
                    {
                        var nuevoReporte = new Reporte
                        {
                            IdComercio = comercio.IdComercio,
                            CantidadDeCajas = cantidadDeCajas,
                            MontoTotalRecaudado = montoRecaudado,
                            CantidadDeSINPES = cantidadDeSINPES,
                            MontoTotalComision = montoComision,
                            FechaDelReporte = fechaActual
                        };

                        db.Reportes.Add(nuevoReporte);
                    }
                }
                Helpers.BitacoraHelper.RegistrarEvento(db, "Reportes", "Actualizar", "");
            }
            catch (Exception ex)
            {
                Helpers.BitacoraHelper.RegistrarError(db, "Reportes", ex);
                ModelState.AddModelError("", "Ocurrió un error al actualizar reporte.");
            }

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
