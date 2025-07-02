
using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Aplicacion.Presentation.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        // 1. Inyectamos IUnitOfWork como ÚNICA dependencia para el acceso a datos.
        private readonly IUnitOfWork _unitOfWork;

        public ProductoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Producto/
        public async Task<IActionResult> Index()
        {
            // 2. Obtenemos las entidades y calculamos su stock para la vista de admin.
            var productos = await _unitOfWork.Productos.GetAllWithCategoriaAsync();
            var viewModels = new List<ProductoViewModel>();

            foreach (var p in productos)
            {
                viewModels.Add(new ProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Stock = await _unitOfWork.Productos.GetCurrentStockAsync(p.Id),
                    Descripcion = p.Descripcion,
                    Estado = p.Estado,
                    NombreCategoria = p.Categoria?.Nombre,
                    OriginalEquipmentManufacture = p.OriginalEquipamentManufacture,
                    ImagenUrl = p.ImagenUrl
                });
            }
            return View(viewModels);
        }

        // GET: /Producto/Crear
        public async Task<IActionResult> Crear()
        {
            // 3. Para el formulario, obtenemos las categorías directamente del repositorio.
            ViewBag.Categorias = new SelectList(await _unitOfWork.Categorias.GetAllAsync(), "Id", "Nombre");
            // Se pasa un ViewModel vacío para inicializar el formulario
            return View(new ProductoViewModel());
        }

        // POST: /Producto/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProductoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 4. Al crear, manejamos la entidad y el movimiento de inventario.
                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    IdCategoria = model.IdCategoria,
                    Descripcion = model.Descripcion,
                    Estado = true,
                    OriginalEquipamentManufacture = model.OriginalEquipmentManufacture,
                    Precio = model.Precio,
                    ImagenUrl = model.ImagenUrl
                };

                await _unitOfWork.Productos.AddAsync(producto);

                if (model.Stock > 0)
                {
                    var movimiento = new MovimientoInventario
                    {
                        Producto = producto,
                        Fecha = DateTime.UtcNow,
                        TipoMovimiento = "ENTRADA",
                        Cantidad = model.Stock,
                        CostoUnitario = model.Precio
                    };
                    await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
                }

                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categorias = new SelectList(await _unitOfWork.Categorias.GetAllAsync(), "Id", "Nombre", model.IdCategoria);
            return View(model);
        }

        // GET: /Producto/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null) return NotFound();

            // Mapeamos la entidad a un ViewModel para enviarlo a la vista
            var model = new ProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                IdCategoria = producto.IdCategoria,
                Descripcion = producto.Descripcion,
                Estado = producto.Estado,
                OriginalEquipmentManufacture = producto.OriginalEquipamentManufacture,
                Precio = producto.Precio,
                ImagenUrl = producto.ImagenUrl,
                Stock = await _unitOfWork.Productos.GetCurrentStockAsync(id) // El stock no es editable, solo se muestra
            };

            ViewBag.Categorias = new SelectList(await _unitOfWork.Categorias.GetAllAsync(), "Id", "Nombre", model.IdCategoria);
            return View(model);
        }

        // POST: /Producto/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ProductoViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);
                if (producto == null) return NotFound();

                // Actualizamos las propiedades de la entidad desde el modelo
                producto.Nombre = model.Nombre;
                producto.IdCategoria = model.IdCategoria;
                producto.Descripcion = model.Descripcion;
                producto.Estado = model.Estado;
                producto.OriginalEquipamentManufacture = model.OriginalEquipmentManufacture;
                producto.Precio = model.Precio;
                producto.ImagenUrl = model.ImagenUrl;
                // NOTA: El stock no se modifica aquí, se hace con movimientos de inventario.

                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categorias = new SelectList(await _unitOfWork.Categorias.GetAllAsync(), "Id", "Nombre", model.IdCategoria);
            return View(model);
        }

        // POST: /Producto/Eliminar/5 (Solo necesitamos la acción POST para el borrado lógico)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // 5. Implementamos BORRADO LÓGICO
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto != null)
            {
                producto.Estado = false;
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Producto/Detalles/5
        [AllowAnonymous]
        public async Task<IActionResult> Detalles(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(id);
            if (producto == null) return NotFound();

            var model = new ProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
                NombreCategoria = producto.Categoria?.Nombre,
                ImagenUrl = producto.ImagenUrl,
                Stock = await _unitOfWork.Productos.GetCurrentStockAsync(id)
            };
            return View(model);
        }
    }
}
