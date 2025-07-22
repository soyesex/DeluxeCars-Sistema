using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using Aplicacion.Core.Models;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Aplicacion.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IContentService _contentService;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IContentService contentService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _contentService = contentService;
        }

        public async Task<IActionResult> Index()
        {
            // Obtenemos los DTOs de productos con stock desde el repositorio
            var productosDto = await _unitOfWork.Productos.GetProductosConStockPositivoAsync();

            // Mapeamos solo los datos que necesitamos para las tarjetas de la página de inicio
            var productosDestacados = productosDto
                .OrderByDescending(p => p.Id)
                .Take(6) // Tomamos 6 para mostrar
                .Select(p => new ProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    ImagenUrl = string.IsNullOrEmpty(p.ImagenUrl) ? "/images/Default.jpeg" : $"/images/{p.ImagenUrl}"
                });

            var pageContent = await _contentService.GetPageContentAsync("Home");

            var model = new HomeViewModel
            {
                ProductosDestacados = productosDestacados,
                PageContent = pageContent
            };
            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Detalles(int id)
        {
            if (id == 0) return BadRequest();

            var producto = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(id);
            if (producto == null) return NotFound();

            var productoViewModel = new ProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                Stock = await _unitOfWork.Productos.GetCurrentStockAsync(id),
                ImagenUrl = string.IsNullOrEmpty(producto.ImagenUrl) ? "/images/Default.jpeg" : $"/images/{producto.ImagenUrl}",
                NombreCategoria = producto.Categoria?.Nombre
            };

            return View(productoViewModel);
        }

        public IActionResult SobreNosotros()
        {
            return View();
        }

        public IActionResult Contactanos()
        {
            // Esto buscará una vista en Views/Home/Contactanos.cshtml
            return View();
        }
    }
}
