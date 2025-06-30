using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class ProductoProveedorRepository : GenericRepository<ProductoProveedor>, IProductoProveedorRepository
    {
        public ProductoProveedorRepository(AppDbContext context) : base(context)
        {}
        public async Task<ProductoProveedor> GetByProductAndSupplierAsync(int productoId, int proveedorId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pp => pp.IdProducto == productoId && pp.IdProveedor == proveedorId);
        }
        public async Task<IEnumerable<ProductoProveedor>> GetByProveedorWithProductoAsync(int proveedorId)
        {
            return await _dbSet
                .Where(pp => pp.IdProveedor == proveedorId)
                .Include(pp => pp.Producto) // Incluimos el producto asociado
                .ToListAsync();
        }
    }
}
