using BancoLosPatitos.Filtros;
using System.Web;
using System.Web.Mvc;

namespace BancoLosPatitos
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LoggingExceptionFilterAttribute());
            filters.Add(new AuthorizeAttribute());  // Asegurar que todas las acciones requieran autenticación
        }
    }
}
