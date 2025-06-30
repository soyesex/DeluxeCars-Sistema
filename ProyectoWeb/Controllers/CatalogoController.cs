using Aplicacion.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly IProductoService _productoService;

        public CatalogoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // Acción para renderizar la vista del catálogo
        public async Task<IActionResult> Index()
        {
            var products = await _productoService.GetAll();
            return View(products);
        }

        // Acción para devolver los productos como JSON
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productoService.GetAll();
            return Json(products);
        }
    }
}
