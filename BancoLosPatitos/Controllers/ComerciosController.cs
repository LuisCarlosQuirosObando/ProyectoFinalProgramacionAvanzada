using BancoLosPatitos.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace BancoLosPatitos.Controllers
{
    public class ComerciosController : Controller
    {
        private PatitosContext db = new PatitosContext();

        // GET: Comercios
        public ActionResult Index()
        {
            return View(db.Comercios.ToList());

        }

        // GET: Comercios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercio comercio = db.Comercios.Find(id);
            if (comercio == null)
            {
                return HttpNotFound();
            }
            return View(comercio);
        }

        // GET: Comercios/Create
        public ActionResult Create()
        {
            ViewBag.TiposIdentificacion = new SelectList(new[]
            {
                new { Valor = 1, Nombre = "Física" },
                new { Valor = 2, Nombre = "Jurídica" }
            }, "Valor", "Nombre");

            ViewBag.TiposDeComercio = new SelectList(new[]
            {
                new { Valor = 1, Nombre = "Restaurantes" },
                new { Valor = 2, Nombre = "Supermercados" },
                new { Valor = 3, Nombre = "Ferreterías" },
                new { Valor = 4, Nombre = "Otros" }
            }, "Valor", "Nombre");

            return View();
        }

        // POST: Comercios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdComercio,Identificacion,TipoIdentificacion,Nombre,TipoDeComercio,Telefono,CorreoElectronico,Direccion,FechaDeRegistro,FechaDeModificacion,Estado")] Comercio comercio)
        {
            if (ModelState.IsValid)
            {
              try { 
                comercio.FechaDeRegistro = DateTime.Now;
                comercio.FechaDeModificacion = DateTime.Now;
                comercio.Estado = 1;
                db.Comercios.Add(comercio);
                db.SaveChanges();

                Helpers.BitacoraHelper.RegistrarEvento(db, "Comercios", "Registrar", comercio);
                return RedirectToAction("Index");
                }

                catch (Exception ex)
                {
                Helpers.BitacoraHelper.RegistrarError(db, "Comercios", ex);
                ModelState.AddModelError("", "Ocurrió un error al crear comercio.");
            }
        }

            return View(comercio);
        }

        // GET: Comercios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercio comercio = db.Comercios.Find(id);
            if (comercio == null)
            {
                return HttpNotFound();
            }
            ViewBag.TiposDeComercios = new SelectList(new[]
{
                 new { Valor = 1, Nombre = "Restaurantes" },
                 new { Valor = 2, Nombre = "Supermercados" },
                 new { Valor = 3, Nombre = "Ferreterías" },
                 new { Valor = 4, Nombre = "Otros" }
             }, "Valor", "Nombre");

            ViewBag.Estados = new SelectList(new[]
            {
                 new { Valor = 1, Nombre = "Activo" },
                 new { Valor = 0, Nombre = "Inactivo" }
             }, "Valor", "Nombre");

            return View(comercio);
        }

        // POST: Comercios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdComercio,Identificacion,TipoIdentificacion,Nombre,TipoDeComercio,Telefono,CorreoElectronico,Direccion,FechaDeRegistro,FechaDeModificacion,Estado")] Comercio comercio)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var datosAnteriores = db.Comercios.AsNoTracking().FirstOrDefault(c => c.IdComercio == comercio.IdComercio);

                    comercio.FechaDeModificacion = DateTime.Now;
                    db.Entry(comercio).State = EntityState.Modified;
                    db.SaveChanges();

                    Helpers.BitacoraHelper.RegistrarEvento(db, "Comercios", "Modificar", datosAnteriores, comercio, "");

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Comercios", ex);
                    ModelState.AddModelError("", "Ocurrió un error al editar comercio.");
                }
            }
            return View(comercio);
        }

        // GET: Comercios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comercio comercio = db.Comercios.Find(id);
            if (comercio == null)
            {
                return HttpNotFound();
            }
            return View(comercio);
        }

        // POST: Comercios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comercio comercio = db.Comercios.Find(id);
            db.Comercios.Remove(comercio);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Comercios/ValidarIdentificacion    
        public ActionResult ValidarIdentificacion()
        {
            return View();
        }

        // POST: Comercios/ValidarIdentificacion
        [HttpPost]
        public ActionResult ValidarIdentificacion(string identificacion)
        {
            var comercioExistente = db.Comercios.FirstOrDefault(c => c.Identificacion == identificacion);

            if (comercioExistente != null)
            {
                ViewBag.IdentificacionNueva = "La identificación ingresada ya está registrada.";
                return View("ValidarIdentificacion");
            }
            else
            { 
                return RedirectToAction("Create");
            }
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
