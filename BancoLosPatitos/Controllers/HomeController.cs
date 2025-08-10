using BancoLosPatitos.Filtros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BancoLosPatitos.Controllers
{
    [LoggingExceptionFilter]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Aplicación del Banco Los Patitos.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "¡Contacta a nuestro equipo de trabajo!";

            return View();
        }
    }
}