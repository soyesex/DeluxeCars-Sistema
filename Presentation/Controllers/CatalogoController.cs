using Aplicacion.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly IProductoService _productoService;

        public CatalogoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // Acción para renderizar la vista del catálogo
        public IActionResult Index()
        {
            var products = _productoService.GetAll();
            return View(products);
        }

        // Acción para devolver los productos como JSON
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _productoService.GetAll();
            return Json(products);
        }
    }
}
