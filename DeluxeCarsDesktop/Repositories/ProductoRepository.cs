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
        private readonly AppDbContext _context;
        public ProductoRepository(AppDbContext context) : base(context)
        {
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
