using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
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
        public IEnumerable<ProductoViewModel> GetAll()
        {
            try
            {
                var productoService = _context.Producto
                .Include(x => x.Categoria)
                .Select(x => new ProductoViewModel
                {
                    Id = x.Id,
                    CategoriaId = x.CategoriaId,
                    Descripcion = x.Descripcion,
                    Estado = x.Estado,
                    FechaIngreso = x.FechaIngreso,
                    Nombre = x.Nombre,
                    NombreCategoria = x.Categoria.Nombre,
                    OrigininalEquipmentManufacture = x.OrigninalEquipmentManufacture,
                    Precio = x.Precio,
                }).ToList();

                return productoService;
            }
            catch (Exception) 
            { 
                return new List<ProductoViewModel>();
            }
        }
        public IEnumerable<ProductoViewModel> Search(string filtro)
        {
            try
            {
                var consulta = _context.Producto.AsQueryable();

                if (!string.IsNullOrEmpty(filtro))
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro) || p.Categoria.Nombre.Contains(filtro));
                }
                return consulta.Select(p => new ProductoViewModel
                {
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Descripcion = p.Descripcion,
                    Estado = p.Estado,
                    Stock = p.Stock,
                }).ToList();
            }
            catch (Exception ex) 
            {
                throw new Exception($"Error en la busquedad de un Producto {ex}");
            }
        }

        public Task<bool> Add(ProductoViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ProductoViewModel GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(ProductoViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
