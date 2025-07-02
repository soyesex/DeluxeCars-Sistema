using Aplicacion.Models.Interfaces;
using Aplicacion.Models.ViewModels;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Aplicacion.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string filtro)
        {
            var productos = await _unitOfWork.Productos.GetAllWithCategoriaAsync();
            var viewModels = new List<ProductoViewModel>();

            foreach (var p in productos)
            {
                var stock = await _unitOfWork.Productos.GetCurrentStockAsync(p.Id);

                // Si quieres mostrar TODOS los productos (incluso sin stock) en el admin,
                // quita la siguiente línea de 'if'. Si solo quieres ver los que tienen stock, déjala.
                if (stock > 0)
                {
                    viewModels.Add(new ProductoViewModel
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Precio = p.Precio,
                        Stock = stock,
                        Descripcion = p.Descripcion,
                        Estado = p.Estado,
                        NombreCategoria = p.Categoria?.Nombre,
                        OriginalEquipmentManufacture = p.OriginalEquipamentManufacture,
                        ImagenUrl = p.ImagenUrl,
                        CategoriaId = p.IdCategoria
                    });
                }
            }

            // 6. Aplicamos el filtro sobre la lista de ViewModels ya construida.
            if (!filtro.IsNullOrEmpty())
            {
                ViewBag.FiltroActual = filtro;
                var filtroLower = filtro.ToLower();
                viewModels = viewModels.Where(vm =>
                    (vm.Nombre != null && vm.Nombre.ToLower().Contains(filtroLower)) ||
                    (vm.Descripcion != null && vm.Descripcion.ToLower().Contains(filtroLower)) ||
                    (vm.OriginalEquipmentManufacture != null && vm.OriginalEquipmentManufacture.ToLower().Contains(filtroLower))
                ).ToList();
            }

            return View(viewModels);
        }
    }
}
