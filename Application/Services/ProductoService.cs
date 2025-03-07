using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using Aplicacion.Infrastructure.Data;
using Aplicacion.Models;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Application.Services
{
    public class ProductoService : IProducto
    {
        private readonly ApplicationDbContext _context;
        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductoViewModel>> GetAllAsync()
        {
            var productos = await _context.Producto.Include(x => x.Categoria)
                .Select(x => new ProductoViewModel
                {
                    CategoriaId = x.CategoriaId,
                    Descripcion = x.Descripcion,
                    Estado = x.Estado,
                    FechaIngreso = x.FechaIngreso,
                    Id = x.Id,
                    Nombre = x.Nombre,
                    OrigininalEquipmentManufacture = x.OrigninalEquipmentManufacture,
                    Precio = x.Precio,
                    NombreCategoria = x.Categoria.Nombre,
                }).ToListAsync();
            return productos;
        }
        public async Task<ProductoViewModel> GetByIdAsync(int id)
        {
            var producto = await _context.Producto
                .Include(x => x.Categoria)
                .FirstOrDefaultAsync(x => x.Id == id) ??
                throw new KeyNotFoundException($"Producto con Id {id} no encontrado");

            return new ProductoViewModel
            {
                CategoriaId = producto.CategoriaId,
                Descripcion = producto.Descripcion,
                Estado = producto.Estado,
                FechaIngreso = producto.FechaIngreso,
                Id = producto.Id,
                Nombre = producto.Nombre,
                OrigininalEquipmentManufacture = producto.OrigninalEquipmentManufacture,
                Precio = producto.Precio
            };
        }
        public async Task AddAsync(ProductoViewModel entity)
        {
            var producto = new Producto
            {
                OrigninalEquipmentManufacture = entity.OrigininalEquipmentManufacture,
                Nombre = entity.Nombre,
                FechaIngreso = entity.FechaIngreso,
                Descripcion = entity.Descripcion,
                CategoriaId = entity.CategoriaId,
                Estado = entity.Estado,
                Precio = entity.Precio,
            };
            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(ProductoViewModel entity)
        {
            var producto = await _context.Producto.FindAsync(entity.Id);

            if (producto is not null)
            {
                producto.Nombre = entity.Nombre;
                producto.Descripcion = entity.Descripcion;
                producto.OrigninalEquipmentManufacture = entity.OrigininalEquipmentManufacture;
                producto.Precio = entity.Precio;
                producto.Estado = entity.Estado;
                producto.CategoriaId = entity.CategoriaId;
                producto.FechaIngreso = entity.FechaIngreso;

                _context.Producto.Update(producto);
                await _context.SaveChangesAsync();
            }
            else throw new KeyNotFoundException();
        }
        public async Task DeleteAsync(int id)
        {
            var producto = await _context.Producto.FindAsync(id);

            if (producto is not null)
            {
                _context.Producto.Remove(producto);
                await _context.SaveChangesAsync();
            }
            else throw new KeyNotFoundException();
        }

    }
}
