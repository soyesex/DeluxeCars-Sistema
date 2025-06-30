using Aplicacion.Core.Models;
using Aplicacion.Data;
using Aplicacion.Models.Interfaces;
using Aplicacion.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Services
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
                var productos = await _context.Productos
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
                        ImagenUrl = File.Exists($"wwwroot/css{x.ImagenUrl}") ? $"wwwroot/css{x.ImagenUrl}" : "/images/Default.jpeg"
                    }).ToListAsync();

                if (productos.Count == 0)
                {
                    Console.WriteLine("⚠ No se encontraron productos en la base de datos.");
                }

                return productos;
            }
            catch (Exception)
            {
                return new List<ProductoViewModel>();
            }
        }
        public async Task<IEnumerable<ProductoViewModel>> GetLimitedProducts(int count = 3)
        {
            var productos = await _context.Productos
                .Include(x => x.Categoria)
                .OrderBy(x => x.Nombre) // Puedes ordenar según alguna lógica deseada
                .Take(count)
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
                    ImagenUrl = File.Exists($"wwwroot/{x.ImagenUrl}") ? x.ImagenUrl : "/images/Default.jpeg"
                }).ToListAsync();

            return productos;
        }

        public async Task<IEnumerable<ProductoViewModel>> Search(string filtro)
        {
            try
            {
                var consulta = _context.Productos.AsQueryable();

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
                    _context.Productos.Add(producto);
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
                var producto = await _context.Productos.FindAsync(id);
                if (producto != null)
                {
                    _context.Productos.Remove(producto);
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
                var producto = await _context.Productos.FindAsync(id);
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
                var producto = await _context.Productos.FindAsync(model.Id);
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
