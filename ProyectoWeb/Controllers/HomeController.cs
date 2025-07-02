using Aplicacion.Models;
using Aplicacion.Models.Interfaces;
using Aplicacion.Models.ViewModels;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Aplicacion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // 1. Inyectamos IUnitOfWork en lugar de IProductoService
        private readonly IUnitOfWork _unitOfWork;

        // 2. Actualizamos el constructor
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // 3. Actualizamos la acción Index para usar la nueva lógica
        public async Task<IActionResult> Index()
        {
            // Obtenemos los productos con stock positivo desde el repositorio
            var productosDto = await _unitOfWork.Productos.GetProductosConStockPositivoAsync();

            // Mapeamos del DTO al ViewModel que la vista necesita, y tomamos solo los 6 primeros para el home
            var viewModels = productosDto
                .Take(6) // Mostramos solo 6 productos en la página principal
                .Select(p => new ProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Descripcion = p.Descripcion,
                    Stock = p.StockActual,
                    ImagenUrl = p.ImagenUrl,
                    // Las demás propiedades del ViewModel quedarán con su valor por defecto (null, 0, false)
                });

            return View(viewModels);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
