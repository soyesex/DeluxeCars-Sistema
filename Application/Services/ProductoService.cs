using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using Aplicacion.Core.Models;
using Aplicacion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ApplicationDbContext _context;
        public ProductoService(ApplicationDbContext context) 
        {
            _context = context;
        }
        public async Task<IEnumerable<ProductoViewModel>> GetAll()
        {
            try
            {
                var productoService = await _context.Producto
                    .Include(x => x.Categoria)
                    .Select(x => new ProductoViewModel
                    {
                        Id = x.Id,
                        Stock = x.Stock,
                        CategoriaId = x.CategoriaId,
                        Descripcion = x.Descripcion,
                        Estado = x.Estado,
                        FechaIngreso = x.FechaIngreso,
                        Nombre = x.Nombre,
                        NombreCategoria = x.Categoria.Nombre,
                        OrigininalEquipmentManufacture = x.OrigninalEquipmentManufacture,
                        Precio = x.Precio,
                        ImagenUrl = x.ImagenUrl ?? "/images/default.jpg" // Asegúrate de que la entidad Producto tenga esta propiedad
                    }).ToListAsync();

                return productoService;
            }
            catch (Exception)
            {
                return new List<ProductoViewModel>();
            }
        }
        public async Task<IEnumerable<ProductoViewModel>> Search(string filtro)
        {
            try
            {
                var consulta = _context.Producto.AsQueryable();

                if (!string.IsNullOrEmpty(filtro))
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro) || p.Categoria.Nombre.Contains(filtro));
                }
                return await consulta.Select(p => new ProductoViewModel
                {
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Descripcion = p.Descripcion,
                    Estado = p.Estado,
                    Stock = p.Stock,
                }).ToListAsync();
            }
            catch (Exception ex) 
            {
                throw new Exception($"Error en la busquedad de un Producto {ex}");
            }
        }

        public async Task<bool> Add(ProductoViewModel model)
        {
            try
            {
                if (model is not null)
                {
                    var producto = new Producto
                    {
                        Nombre = model.Nombre,
                        CategoriaId = model.CategoriaId,
                        Descripcion = model.Descripcion,
                        Estado = model.Estado,
                        OrigninalEquipmentManufacture = model.OrigininalEquipmentManufacture,
                        FechaIngreso = model.FechaIngreso,
                        Precio = model.Precio,
                        Stock = model.Stock
                    };
                    _context.Producto.Add(producto);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
                    
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al añadir un Producto{ex}");
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var producto = await _context.Producto.FindAsync(id);
                if (producto != null)
                {
                    _context.Producto.Remove(producto);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex) 
            {
                throw new Exception($"Error al eliminar un Producto{ex}");
            }
        }

        public async Task<ProductoViewModel> GetById(int id)
        {
            try
            {
                var producto = await _context.Producto.FindAsync(id);
                if (producto != null)
                {
                    return new ProductoViewModel
                    {
                        CategoriaId = producto.CategoriaId,
                        Descripcion = producto.Descripcion,
                        Estado = producto.Estado,
                        FechaIngreso = producto.FechaIngreso,
                        Id = producto.Id,
                        ImagenUrl = producto.ImagenUrl,
                        Nombre = producto.Nombre,
                        OrigininalEquipmentManufacture = producto.OrigninalEquipmentManufacture,
                        Precio = producto.Precio,
                        Stock = producto.Stock
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al conseguir un producto por el Id {ex}");
            }
        }

        public async Task<bool> Update(ProductoViewModel model)
        {
            try
            {
                var producto = await _context.Producto.FindAsync(model.Id);
                if (producto != null)
                {
                    producto.Descripcion = model.Descripcion;
                    producto.Estado = model.Estado;
                    producto.Stock = model.Stock;
                    producto.FechaIngreso = model.FechaIngreso;
                    producto.Nombre = model.Nombre;
                    producto.OrigninalEquipmentManufacture = producto.OrigninalEquipmentManufacture;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar el producto {ex}");
            }
        }
    }
}
