using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        // 1. Inyectamos IUnitOfWork
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                // 2. Llamamos a los métodos del repositorio a través de UnitOfWork
                //    Asegúrate de que estos métodos existan en tu IProductoRepository
                TotalProductos = await _unitOfWork.Productos.CountAllAsync(),
                ProductosBajoStock = await _unitOfWork.Productos.CountLowStockProductsAsync()
            };
            return View(viewModel);
        }
    }
}
