using Aplicacion.Application.Services;
using Aplicacion.Core.Interfaces;
using Aplicacion.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;
        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }
        public IActionResult Index(string filtro)
        {
            var productos = string.IsNullOrEmpty(filtro)?_productoService.GetAll():_productoService.Search(filtro);

            //Para que el input mantega el valor
            ViewBag.FiltroActual = filtro;

            return View(productos);
        }
        
    }
}
