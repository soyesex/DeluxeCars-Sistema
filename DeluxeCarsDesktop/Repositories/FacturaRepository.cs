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
    public class FacturaRepository : GenericRepository<Factura>, IFacturaRepository
    {
        public FacturaRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Factura>> GetAllWithClienteYMetodoPagoAsync()
        {
            return await _context.Facturas
                         .Include(f => f.Cliente)
                         .Include(f => f.MetodoPago)
                         .IgnoreQueryFilters() // <-- AÑADE ESTA LÍNEA
                         .AsNoTracking()
                         .OrderByDescending(f => f.FechaEmision)
                         .ToListAsync();
        }
        public async Task<Factura> GetFacturaWithDetailsAsync(int facturaId)
        {
            return await _dbSet
                .Include(f => f.Cliente) // Incluye el cliente relacionado
                .Include(f => f.Usuario) // Incluye el usuario (vendedor)
                .Include(f => f.DetallesFactura) // Incluye las líneas de detalle
                .FirstOrDefaultAsync(f => f.Id == facturaId);
        }

        public async Task<IEnumerable<Factura>> GetFacturasByClienteAsync(int clienteId)
        {
            return await _dbSet
                .Where(f => f.IdCliente == clienteId)
                .OrderByDescending(f => f.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Factura>> GetFacturasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(f => f.FechaEmision >= startDate && f.FechaEmision <= endDate)
                .Include(f => f.Cliente)
                .OrderByDescending(f => f.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
