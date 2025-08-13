using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BancoLosPatitos.API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "¡Contacta a nuestro equipo de trabajo!";

            return View();
        }


    }
}
