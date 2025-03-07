using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    public class CatalogoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
