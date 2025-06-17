using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
    {
        public ProductoRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Producto>> GetAssociatedProductsAsync(int proveedorId)
        {
            return await _context.ProductoProveedores
                .Where(pp => pp.IdProveedor == proveedorId)
                // 1. PRIMERO, incluimos el Producto y, DENTRO de él, su Categoría.
                .Include(pp => pp.Producto)
                    .ThenInclude(p => p.Categoria)
                // 2. AHORA, al final, seleccionamos el Producto que queremos devolver.
                .Select(pp => pp.Producto)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetUnassociatedProductsAsync(int proveedorId)
        {
            var associatedProductIds = await _context.ProductoProveedores
                .Where(pp => pp.IdProveedor == proveedorId)
                .Select(pp => pp.IdProducto)
                .ToListAsync();

            return await _context.Productos
                .Where(p => !associatedProductIds.Contains(p.Id))
                .Include(p => p.Categoria)
                .ToListAsync();
        }
        // ÚNICA RESPONSABILIDAD: Implementar los métodos especializados.
        public async Task<IEnumerable<Producto>> SearchProductsBySupplierAsync(int proveedorId, string searchTerm)
        {
            var query = from pp in _context.ProductoProveedores
                        join p in _context.Productos on pp.IdProducto equals p.Id
                        where pp.IdProveedor == proveedorId && p.Nombre.Contains(searchTerm)
                        select p;

            // Añadimos AsNoTracking() para que EF solo lea los datos sin "vigilarlos"
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetAllWithCategoriaAsync()
        {
            // La consulta final y limpia para traer productos con su categoría
            return await _context.Productos
                                 .Include(p => p.Categoria)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetLowStockProductsAsync(int stockThreshold)
        {
            return await _dbSet
                .Where(p => p.Stock < stockThreshold)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

