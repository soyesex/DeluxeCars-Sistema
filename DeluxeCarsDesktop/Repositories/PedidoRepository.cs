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
    public class PedidoRepository : GenericRepository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(AppDbContext context) : base(context)
        { }
        public async Task<IEnumerable<Pedido>> GetPedidosByCriteriaAsync(DateTime startDate, DateTime endDate, int? proveedorId)
        {
            var query = _context.Pedidos
                                .Include(p => p.Proveedor) // Incluimos datos del proveedor
                                .Include(p => p.Usuario)   // Incluimos datos del usuario
                                .Where(p => p.FechaEmision >= startDate && p.FechaEmision <= endDate);

            if (proveedorId.HasValue && proveedorId.Value > 0)
            {
                query = query.Where(p => p.IdProveedor == proveedorId.Value);
            }

            return await query.OrderByDescending(p => p.FechaEmision).AsNoTracking().ToListAsync();
        }

        public Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId)
        {
            throw new NotImplementedException();
        }

        // --- MÉTODO CORREGIDO Y MEJORADO ---
        public async Task<Pedido> GetPedidoWithDetailsAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Proveedor)
                .Include(p => p.Usuario)
                .Include(p => p.DetallesPedidos) // Incluye la colección de detalles
                    .ThenInclude(d => d.Producto) // Y DENTRO de cada detalle, incluye el Producto
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }
    }
}
