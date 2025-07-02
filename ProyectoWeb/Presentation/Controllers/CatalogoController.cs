using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    public class CatalogoController : Controller
    {
        // 1. Inyectamos IUnitOfWork
        private readonly IUnitOfWork _unitOfWork;

        public CatalogoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string orden = "nombre-asc", string categoria = "")
        {
            // 2. Llamamos al nuevo método del repositorio que hace todo el trabajo pesado
            var productosDto = await _unitOfWork.Productos.SearchPublicCatalogAsync(categoria, orden);

            // 3. Mapeamos los DTOs al ViewModel que la vista necesita
            var productosViewModel = productosDto.Select(p => new ProductoViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                ImagenUrl = string.IsNullOrEmpty(p.ImagenUrl) ? "/images/Default.jpeg" : $"/images/{p.ImagenUrl}",
                // El resto de las propiedades que tu _ProductCard.cshtml necesite
            }).ToList(); // Convertimos a Lista para la vista

            // 4. Obtenemos las categorías para el menú de filtros
            var todasLasCategorias = await _unitOfWork.Categorias.GetAllAsync();
            ViewData["Categorias"] = todasLasCategorias.Select(c => c.Nombre).Distinct().OrderBy(c => c).ToList();

            // 5. Pasamos los filtros actuales a la vista
            ViewData["OrdenActual"] = orden;
            ViewData["CategoriaActual"] = categoria;

            return View(productosViewModel);
        }

        // Acción para devolver los productos como JSON
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _unitOfWork.Productos.GetProductosConStockPositivoAsync();
            return Json(products);
        }
    }
}
