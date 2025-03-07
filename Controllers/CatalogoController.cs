using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Controllers
{
    public class CatalogoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
