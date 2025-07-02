using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace Aplicacion.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductoViewModel>> GetAllWithPositiveStockAsync()
        {
            var dtos = await _unitOfWork.Productos.GetProductosConStockPositivoAsync();
            return dtos.Select(p => new ProductoViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Stock = p.StockActual,
                Descripcion = p.Descripcion,
                ImagenUrl = string.IsNullOrEmpty(p.ImagenUrl) ? "Default.jpeg" : p.ImagenUrl,
            });
        }

        public async Task<IEnumerable<ProductoViewModel>> GetFeaturedProductsAsync(int count = 3)
        {
            var dtos = await _unitOfWork.Productos.GetProductosConStockPositivoAsync();
            return dtos
                .OrderByDescending(p => p.Id)
                .Take(count)
                .Select(p => new ProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Stock = p.StockActual,
                    Descripcion = p.Descripcion,
                    ImagenUrl = string.IsNullOrEmpty(p.ImagenUrl) ? "Default.jpeg" : p.ImagenUrl
                });
        }

        public async Task<ProductoViewModel> GetByIdAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(id);
            if (producto == null) return null;
            return new ProductoViewModel
            {
                Id = producto.Id,
                IdCategoria = producto.IdCategoria,
                Descripcion = producto.Descripcion,
                Estado = producto.Estado,
                ImagenUrl = producto.ImagenUrl,
                Nombre = producto.Nombre,
                NombreCategoria = producto.Categoria?.Nombre,
                OriginalEquipmentManufacture = producto.OriginalEquipamentManufacture,
                Precio = producto.Precio,
                Stock = await _unitOfWork.Productos.GetCurrentStockAsync(id)
            };
        }

        public async Task<bool> CreateAsync(ProductoViewModel model)
        {
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
                await _unitOfWork.MovimientosInventario.AddAsync(new MovimientoInventario
                {
                    Producto = producto,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = "ENTRADA",
                    Cantidad = model.Stock,
                    CostoUnitario = model.Precio
                });
            }
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> UpdateAsync(ProductoViewModel model)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(model.Id);
            if (producto == null) return false;

            producto.Nombre = model.Nombre;
            producto.IdCategoria = model.IdCategoria;
            //... resto de las propiedades a actualizar

            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null) return false;
            producto.Estado = false;
            return await _unitOfWork.CompleteAsync() > 0;
        }

        // --- MÉTODOS AÑADIDOS PARA IMPLEMENTAR LA INTERFAZ COMPLETA ---
        public async Task<int> CountAllAsync()
        {
            // Asumiendo que tu repositorio tiene un método CountAsync
            return await _unitOfWork.Productos.CountAsync();
        }

        public async Task<int> CountLowStockAsync(int umbral = 10)
        {
            // Asumiendo que tu repositorio tiene un método para esto
            return await _unitOfWork.Productos.CountAsync(p => p.StockMinimo.HasValue && (_unitOfWork.Context.MovimientosInventario.Where(m => m.IdProducto == p.Id).Sum(m => m.Cantidad) < p.StockMinimo));
        }
    }
}

