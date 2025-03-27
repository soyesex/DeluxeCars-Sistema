using Aplicacion.Application.Services;
using Aplicacion.Core.Interfaces;
using Aplicacion.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Aplicacion.Presentation.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;
        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }
        public async Task<IActionResult> Index(string filtro)
        {
            if (filtro.IsNullOrEmpty())
            {
                var productos = await _productoService.GetAll();
                return View(productos);
            }
            else
            {
                //Para que el input mantega el valor
                ViewBag.FiltroActual = filtro;
                var productos = _productoService.Search(filtro);
                return View(productos);
            }
        }
        
    }
}
