using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BancoLosPatitos.Filtros
{
    public class LoggingExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;

            var ex = filterContext.Exception;

            File.AppendAllText(HttpContext.Current.Server.MapPath("~/App_Data/ErrorLog.txt"),
                $"{DateTime.Now}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}"
                );

            filterContext.Result = new ViewResult
            {
                ViewName = "Error"
            };

            filterContext.ExceptionHandled = true;

        }
    }
}