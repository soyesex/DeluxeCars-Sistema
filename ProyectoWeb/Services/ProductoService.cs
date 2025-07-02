using Aplicacion.Models;
using Aplicacion.Models.Interfaces;
using Aplicacion.Models.ViewModels;
using DeluxeCars.DataAccess;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductoViewModel>> GetAll()
        {
            // 1. OBTENER LAS ENTIDADES COMPLETAS:
            // Usamos un método del repositorio que traiga los productos con su categoría.
            var productos = await _unitOfWork.Productos.GetAllWithCategoriaAsync();

            var viewModels = new List<ProductoViewModel>();

            // 2. CONSTRUIR EL VIEWMODEL PARA CADA ENTIDAD:
            foreach (var p in productos)
            {
                // Para cada producto, calculamos su stock actual.
                var stock = await _unitOfWork.Productos.GetCurrentStockAsync(p.Id);

                // Solo agregamos el producto a la lista si tiene stock positivo.
                if (stock > 0)
                {
                    viewModels.Add(new ProductoViewModel
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Precio = p.Precio,
                        Stock = stock, // Asignamos el stock que acabamos de calcular.
                        Descripcion = p.Descripcion,
                        Estado = p.Estado,
                        NombreCategoria = p.Categoria?.Nombre, // Acceso seguro a la categoría.
                        OriginalEquipmentManufacture = p.OriginalEquipamentManufacture,
                        ImagenUrl = p.ImagenUrl,
                        CategoriaId = p.IdCategoria
                    });
                }
            }

            return viewModels;
        }

        public async Task<bool> Add(ProductoViewModel model)
        {
            if (model == null) return false;

            var producto = new Producto
            {
                Nombre = model.Nombre,
                IdCategoria = model.CategoriaId,
                Descripcion = model.Descripcion,
                Estado = model.Estado,
                // 3. CORRECCIÓN DE NOMBRE: Usar el nombre de propiedad corregido en el ViewModel.
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

            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null) return false;

            producto.Estado = false;
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public async Task<ProductoViewModel> GetById(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(id);
            if (producto == null) return null;

            return new ProductoViewModel
            {
                Id = producto.Id,
                CategoriaId = producto.IdCategoria,
                Descripcion = producto.Descripcion,
                Estado = producto.Estado,
                ImagenUrl = producto.ImagenUrl,
                Nombre = producto.Nombre,
                NombreCategoria = producto.Categoria?.Nombre,
                // 3. CORRECCIÓN DE NOMBRE:
                OriginalEquipmentManufacture = producto.OriginalEquipamentManufacture,
                Precio = producto.Precio,
                Stock = await _unitOfWork.Productos.GetCurrentStockAsync(id)
            };
        }

        public async Task<bool> Update(ProductoViewModel model)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(model.Id);
            if (producto == null) return false;

            producto.Descripcion = model.Descripcion;
            producto.Estado = model.Estado;
            producto.Nombre = model.Nombre;
            // 3. CORRECCIÓN DE NOMBRE:
            producto.OriginalEquipamentManufacture = model.OriginalEquipmentManufacture;
            producto.Precio = model.Precio;
            producto.IdCategoria = model.CategoriaId;
            producto.ImagenUrl = model.ImagenUrl;

            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public Task<IEnumerable<ProductoViewModel>> Search(string filtro)
        {
            throw new NotImplementedException("Se debe implementar la búsqueda en el repositorio para un rendimiento óptimo.");
        }

        public Task<IEnumerable<ProductoViewModel>> GetLimitedProducts(int count = 3)
        {
            throw new NotImplementedException("Se debe implementar en el repositorio si es necesario.");
        }
    }
}

