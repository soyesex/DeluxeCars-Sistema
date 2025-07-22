
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Producto/
        public async Task<IActionResult> Index()
        {
            // 1. Obtenemos las entidades base en una consulta
            var productos = await _unitOfWork.Productos.GetAllWithCategoriaAsync();
            if (!productos.Any())
            {
                return View(new List<ProductoViewModel>());
            }

            // 2. Obtenemos TODOS los stocks necesarios en UNA SOLA consulta
            var idsDeProductos = productos.Select(p => p.Id);
            var stocksDiccionario = await _unitOfWork.Productos.GetCurrentStocksAsync(idsDeProductos);

            // 3. Mapeamos a los ViewModels en memoria, sin más consultas a la BD
            var viewModels = productos.Select(p => new ProductoViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                // Buscamos el stock en el diccionario localmente. Es instantáneo.
                Stock = stocksDiccionario.GetValueOrDefault(p.Id, 0),
                Descripcion = p.Descripcion,
                Estado = p.Estado,
                NombreCategoria = p.Categoria?.Nombre,
                OriginalEquipmentManufacture = p.OriginalEquipamentManufacture,
                ImagenUrl = string.IsNullOrEmpty(p.ImagenUrl)
                ? "/images/default.jpeg"
                : "/" + p.ImagenUrl
            }).ToList();

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
                string? rutaImagenRelativa = null;

                // --- LÓGICA PARA GUARDAR LA IMAGEN ---
                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    // 1. Definir la carpeta donde se guardarán las imágenes
                    string carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");

                    // Esta única línea es suficiente. Crea el directorio si no existe.
                    Directory.CreateDirectory(carpetaImagenes);

                    // 2. Crear un nombre de archivo único para evitar conflictos
                    string nombreArchivoUnico = Guid.NewGuid().ToString() + Path.GetExtension(model.ImagenArchivo.FileName);
                    string rutaFisicaCompleta = Path.Combine(carpetaImagenes, nombreArchivoUnico);

                    // 3. Guardar el archivo en el servidor
                    using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
                    {
                        await model.ImagenArchivo.CopyToAsync(stream);
                    }

                    // 4. Guardar solo la ruta relativa en la base de datos
                    rutaImagenRelativa = Path.Combine("images", "productos", nombreArchivoUnico).Replace('\\', '/');
                }

                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    IdCategoria = model.IdCategoria,
                    Descripcion = model.Descripcion,
                    Estado = true,
                    OriginalEquipamentManufacture = model.OriginalEquipmentManufacture,
                    Precio = model.Precio,
                    ImagenUrl = rutaImagenRelativa
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
                // --- LÓGICA PARA ACTUALIZAR LA IMAGEN ---
                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                        // Opcional: Eliminar la imagen anterior si existe
                    if (!string.IsNullOrEmpty(producto.ImagenUrl))
                    {
                        string rutaImagenAnterior = Path.Combine(_webHostEnvironment.WebRootPath, producto.ImagenUrl.Replace('/', '\\'));
                        if (System.IO.File.Exists(rutaImagenAnterior))
                        {
                            System.IO.File.Delete(rutaImagenAnterior);
                        }
                    }


                    // Guardar la nueva imagen (misma lógica que en Crear)
                    string carpetaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
                    Directory.CreateDirectory(carpetaImagenes);
                    string nombreArchivoUnico = Guid.NewGuid().ToString() + Path.GetExtension(model.ImagenArchivo.FileName);
                    string rutaFisicaCompleta = Path.Combine(carpetaImagenes, nombreArchivoUnico);
                    using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
                    {
                        await model.ImagenArchivo.CopyToAsync(stream);
                    }
                    producto.ImagenUrl = Path.Combine("images", "productos", nombreArchivoUnico).Replace('\\', '/');
            }

                // Actualizamos las propiedades de la entidad desde el modelo
                producto.Nombre = model.Nombre;
                producto.IdCategoria = model.IdCategoria;
                producto.Descripcion = model.Descripcion;
                producto.Estado = model.Estado;
                producto.OriginalEquipamentManufacture = model.OriginalEquipmentManufacture;
                producto.Precio = model.Precio;
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
