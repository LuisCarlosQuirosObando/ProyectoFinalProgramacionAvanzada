using BancoLosPatitos.API.Models;
using BancoLosPatitos.Filtros;
using BancoLosPatitos.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web.Mvc;




namespace BancoLosPatitos.Controllers
{



    [LoggingExceptionFilter]
    [RequireRegisteredUser]
    public class SinpesController : Controller
    {
        private PatitosContext db = new PatitosContext();

        private readonly string apiBaseUrl = "https://localhost:44361/api/sinpe/";

        // GET: Sinpes
        // Soporta entrar por telefono o por idCaja (se convierte a teléfono)
        [Authorize(Roles = "Administrador,Cajero")]
        public ActionResult Index(string telefono = null, int? idCaja = null)
        {
            // Si viene idCaja, lo validamos a su teléfono de SINPE
            if (idCaja.HasValue)
            {
                var telCaja = db.Cajas
                                .Where(c => c.IdCaja == idCaja.Value)
                                .Select(c => c.TelefonoSINPE)
                                .FirstOrDefault();

                // Si existe, priorizamos ese teléfono como filtro
                if (!string.IsNullOrWhiteSpace(telCaja))
                {
                    telefono = telCaja;
                    ViewBag.IdCaja = idCaja;   // 
                }
                else
                {
                    ViewBag.Error = "La caja indicada no existe o no tiene teléfono SINPE.";
                }
            }

            var data = new List<Sinpe>();

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                telefono = telefono.Trim();

                // Filtrado por permisos del usuario (Administrador: todos; Cajero: sus comercios)
                var permitidos = TelefonosPermitidosParaUsuarioActual();

                // Valida si puede ver ese teléfono?
                if (!permitidos.Contains(telefono))
                {
                    ViewBag.Error = "No existen SINPE asociados al teléfono ingresado, o no tiene permisos para consultarlo.";
                    ViewBag.Telefono = telefono;
                    return View(data); // 
                }

                // Consulta normal
                data = db.Sinpes
                         .Where(s => s.TelefonoDestinatario == telefono)
                         .OrderByDescending(s => s.FechaDeRegistro)
                         .ToList();

                ViewBag.Telefono = telefono;

                // Para que el botón "Registrar Nuevo SINPE" sepa de qué caja es
                ViewBag.IdCaja = db.Cajas
                                   .Where(c => c.TelefonoSINPE == telefono)
                                   .Select(c => (int?)c.IdCaja)
                                   .FirstOrDefault();

                if (data.Count == 0)
                    ViewBag.Info = "No hay movimientos para ese teléfono.";
            }
            else
            {
                // lista vacía
                if (User.IsInRole("Administrador"))
                    data = db.Sinpes.OrderByDescending(s => s.FechaDeRegistro).Take(200).ToList();
            }

            return View(data);
        }

        // Teléfonos de caja visibles según el usuario actual
        private List<string> TelefonosPermitidosParaUsuarioActual()
        {
            if (User.IsInRole("Administrador"))
                return db.Cajas.Select(c => c.TelefonoSINPE).ToList();

            var userIdStr = User.Identity.GetUserId(); // string
            Guid userGuid;
            Guid.TryParse(userIdStr, out userGuid);

            var misComercios = db.Usuarios
                                 .Where(u => u.IdNetUser == userGuid && u.Estado == 1)
                                 .Select(u => u.IdComercio)
                                 .ToList();

            return db.Cajas
                     .Where(c => misComercios.Contains(c.IdComercio))
                     .Select(c => c.TelefonoSINPE)
                     .ToList();
        }

        // GET: Sinpes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var sinpe = db.Sinpes.Find(id);
            if (sinpe == null) return HttpNotFound();
            return View(sinpe);
        }

        // GET: Sinpes/Create
        [Authorize(Roles = "Administrador")]
        public ActionResult Create(int? idCaja)
        {
            ViewBag.IdCaja = idCaja; // la vista lo usa para armar el "Regresar"
            ViewBag.Telefono = idCaja.HasValue
            ? db.Cajas.Where(c => c.IdCaja == idCaja.Value)
                .Select(c => c.TelefonoSINPE)
                .FirstOrDefault()
      : null;   

            return View();
        }

        // POST: Sinpes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult Create([Bind(Include = "IdSinpe,TelefonoOrigen,NombreOrigen,TelefonoDestinatario,NombreDestinatario,Monto,FechaDeRegistro,Descripcion,Estado")] Sinpe sinpe,
                                   int? idCaja)
        {
            // Validación de caja destino (por teléfono)
            var cajaDestino = db.Cajas.FirstOrDefault(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario);
            if (cajaDestino == null)
                ModelState.AddModelError("TelefonoDestinatario", "No existe una caja con este número de teléfono.");
            else if (cajaDestino.Estado != 1)
                ModelState.AddModelError("TelefonoDestinatario", "No se puede realizar pagos hacia una caja inactiva.");

            if (ModelState.IsValid)
            {
                try
                {
                    sinpe.Estado = 0;
                    sinpe.FechaDeRegistro = DateTime.Now;
                    db.Sinpes.Add(sinpe);
                    db.SaveChanges();

                    Helpers.BitacoraHelper.RegistrarEvento(db, "SINPES", "Registrar", sinpe);

                    // Redireccionar manteniendo el contexto por idCaja si es posible
                    var idCajaRedirect = db.Cajas
                                           .Where(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario)
                                           .Select(c => (int?)c.IdCaja)
                                           .FirstOrDefault();

                    if (idCajaRedirect.HasValue)
                        return RedirectToAction("Index", new { idCaja = idCajaRedirect.Value });

                    // por teléfono si no hubiera caja 
                    return RedirectToAction("Index", new { telefono = sinpe.TelefonoDestinatario });
                }
                catch (Exception ex)
                {
                    Helpers.BitacoraHelper.RegistrarError(db, "Sinpes", ex);
                    ModelState.AddModelError("", "Ocurrió un error al crear SINPE.");
                }
            }

            ViewBag.IdCaja = idCaja; // 
            return View(sinpe);
        }

        // GET: Sinpes/Edit/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var sinpe = db.Sinpes.Find(id);
            if (sinpe == null) return HttpNotFound();
            return View(sinpe);
        }

        // POST: Sinpes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit([Bind(Include = "IdSinpe,TelefonoOrigen,NombreOrigen,TelefonoDestinatario,NombreDestinatario,Monto,FechaDeRegistro,Descripcion,Estado")] Sinpe sinpe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sinpe).State = EntityState.Modified;
                db.SaveChanges();

                var idCajaRedirect = db.Cajas
                                       .Where(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario)
                                       .Select(c => (int?)c.IdCaja)
                                       .FirstOrDefault();

                if (idCajaRedirect.HasValue)
                    return RedirectToAction("Index", new { idCaja = idCajaRedirect.Value });

                return RedirectToAction("Index", new { telefono = sinpe.TelefonoDestinatario });
            }
            return View(sinpe);
        }

        // GET: Sinpes/Delete/5
        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var sinpe = db.Sinpes.Find(id);
            if (sinpe == null) return HttpNotFound();
            return View(sinpe);
        }

        // POST: Sinpes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult DeleteConfirmed(int id)
        {
            var sinpe = db.Sinpes.Find(id);
            var tel = sinpe?.TelefonoDestinatario;
            db.Sinpes.Remove(sinpe);
            db.SaveChanges();

            var idCajaRedirect = db.Cajas
                                   .Where(c => c.TelefonoSINPE == tel)
                                   .Select(c => (int?)c.IdCaja)
                                   .FirstOrDefault();

            if (idCajaRedirect.HasValue)
                return RedirectToAction("Index", new { idCaja = idCajaRedirect.Value });

            return RedirectToAction("Index", new { telefono = tel });
        }

        [HttpPost]
        [Authorize(Roles = "Cajero,Administrador")]
        public ActionResult SincronizarSinpe(int id)
        {
            var sinpe = db.Sinpes.Find(id);
            if (sinpe != null && sinpe.Estado == 0)
            {
                sinpe.Estado = 1;
                db.SaveChanges();
                TempData["mensaje"] = "SINPE sincronizado correctamente.";
            }
            else
            {
                TempData["mensaje"] = "Este SINPE ya estaba sincronizado o no existe.";
            }

            var idCajaRedirect = db.Cajas
                                   .Where(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario)
                                   .Select(c => (int?)c.IdCaja)
                                   .FirstOrDefault();

            if (idCajaRedirect.HasValue)
                return RedirectToAction("Index", new { idCaja = idCajaRedirect.Value });

            return RedirectToAction("Index", new { telefono = sinpe.TelefonoDestinatario });
        }

        public ActionResult Consultar() => View();

        

        [HttpPost]
        public async Task<ActionResult> Consultar(string telefono)
        {
            List<SinpeAPI> resultado = new List<SinpeAPI>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                var response = await client.GetAsync($"obtener/{telefono}");
                if (response.IsSuccessStatusCode)
                    resultado = await response.Content.ReadFromJsonAsync<List<SinpeAPI>>();
                else
                    ViewBag.Error = "No se pudo obtener la información.";
            }
            return View(resultado);
        }

        public ActionResult Sincronizar() => View();

        [HttpPost]
        public async Task<ActionResult> Sincronizar(int idSinpe)
        {
            SinpeSyncResponse resultado = new SinpeSyncResponse();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                var response = await client.PutAsync($"sincronizar/{idSinpe}", null);
                if (response.IsSuccessStatusCode)
                    resultado = await response.Content.ReadFromJsonAsync<SinpeSyncResponse>();
                else
                {
                    resultado.EsValido = false;
                    resultado.Mensaje = "No se pudo sincronizar el SINPE.";
                }
            }
            return View(resultado);
        }

        public ActionResult Recibir() => View();

        [HttpPost]
        public async Task<ActionResult> Recibir(
            string TelefonoOrigen,
            string NombreOrigen,
            string TelefonoDestinatario,
            string NombreDestinatario,
            decimal Monto,
            string Descripcion,
            DateTime Fecha,
            bool Estado)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBaseUrl);

                var nuevoSinpe = new
                {
                    TelefonoOrigen,
                    NombreOrigen,
                    TelefonoDestinatario,
                    NombreDestinatario,
                    Monto,
                    Descripcion,
                    Fecha,
                    Estado
                };

                var response = await client.PostAsJsonAsync("recibir", nuevoSinpe);
                ViewBag.Mensaje = response.IsSuccessStatusCode
                                  ? "SINPE registrado correctamente."
                                  : "Error al registrar SINPE.";
            }
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

