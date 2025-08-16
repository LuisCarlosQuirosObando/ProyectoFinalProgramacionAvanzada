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
    [Authorize(Roles = "Administrador")]
    [RequireRegisteredUser]
    public class UsuariosController : Controller
    {
        private PatitosContext db = new PatitosContext();

        // GET: Usuarios
        public ActionResult Index(int? idComercio)
        {

            var usuario = db.Usuarios.Include(c => c.Comercio);

            if (idComercio.HasValue)
            {
                usuario = usuario.Where(c => c.IdComercio == idComercio.Value);
                ViewBag.FiltroComercio = db.Comercios
                    .Where(c => c.IdComercio == idComercio.Value)
                    .Select(c => c.Nombre)
                    .FirstOrDefault();
            }

            return View(usuario.ToList());
        }

        // GET: Usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuarios = db.Usuarios.Find(id);
            if (usuarios == null)
            {
                return HttpNotFound();
            }
            return View(usuarios);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdUsuario,IdComercio,IdNetUser,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico")] Usuarios usuarios)
        {
            // Validación: Identificación única
            if (db.Usuarios.Any(u => u.Identificacion == usuarios.Identificacion))
            {
                ModelState.AddModelError("Identificacion", "Ya existe un usuario con esta identificación.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    usuarios.FechaDeRegistro = DateTime.Now;
                    usuarios.FechaDeModificacion = DateTime.Now;
                    usuarios.Estado = 1;

                    db.Usuarios.Add(usuarios);
                    db.SaveChanges();

                    Helpers.BitacoraHelper.RegistrarEvento(db, "Usuarios", "Registrar", usuarios);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Usuarios", ex);
                    ModelState.AddModelError("", "Ocurrió un error al crear usuario.");
                }
            }

            // Si algo falla, rearmamos el combo y devolvemos la vista con errores
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", usuarios.IdComercio);
            return View(usuarios);
        }


        // GET: Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuarios = db.Usuarios.Find(id);
            if (usuarios == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", usuarios.IdComercio);

            ViewBag.Estados = new SelectList(new[]
{
                 new { Valor = 1, Nombre = "Activo" },
                 new { Valor = 0, Nombre = "Inactivo" }
             }, "Valor", "Nombre");

            return View(usuarios);
        }

        // POST: Usuarios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdUsuario,IdComercio,IdNetUser,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico,Estado")] Usuarios usuarios)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var datosAnteriores = db.Usuarios.AsNoTracking().FirstOrDefault(c => c.IdUsuario == usuarios.IdUsuario);

                    var existente = db.Usuarios.Find(usuarios.IdUsuario);
                    if (existente == null)
                        return HttpNotFound();

                    existente.Nombres = usuarios.Nombres;
                    existente.PrimerApellido = usuarios.PrimerApellido;
                    existente.SegundoApellido = usuarios.SegundoApellido;
                    existente.Identificacion = usuarios.Identificacion;
                    existente.CorreoElectronico = usuarios.CorreoElectronico;
                    existente.Estado = usuarios.Estado;
                    existente.FechaDeModificacion = DateTime.Now;

                    db.SaveChanges();

                    Helpers.BitacoraHelper.RegistrarEvento(db, "Usuarios", "Modificar", datosAnteriores, usuarios, "");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Usuarios", ex);
                    ModelState.AddModelError("", "Ocurrió un error al editar usuario.");
                }
            }

            ViewBag.IdComercio = new SelectList(db.Comercios, "IdComercio", "Identificacion", usuarios.IdComercio);
            return View(usuarios);
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuarios = db.Usuarios.Find(id);
            if (usuarios == null)
            {
                return HttpNotFound();
            }
            return View(usuarios);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Usuarios usuarios = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuarios);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult VerUsuarios(string identificacion)
        {
            if (string.IsNullOrEmpty(identificacion))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuarios = db.Usuarios
                .Where(u => u.Identificacion == identificacion)
                .OrderByDescending(u => u.Nombres)
                .ToList();

            return View("~/Views/Usuarios/Index.cshtml", usuarios);
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
