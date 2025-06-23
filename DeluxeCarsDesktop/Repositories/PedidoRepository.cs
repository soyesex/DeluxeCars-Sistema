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
        // --- MÉTODO DE BÚSQUEDA POTENCIADO ---
        public async Task<IEnumerable<Pedido>> GetPedidosByCriteriaAsync(DateTime startDate, DateTime endDate, int? proveedorId, EstadoPedido? estado)
        {
            var query = _context.Pedidos
                                .Include(p => p.Proveedor) // Incluimos datos del proveedor
                                .Include(p => p.Usuario)   // Incluimos datos del usuario
                                .AsQueryable(); // Empezamos con IQueryable para construir la consulta dinámicamente

            // --- REFINAMIENTO CLAVE: MANEJO PRECISO DE FECHAS ---
            // Para incluir todos los eventos del día 'endDate', buscamos hasta el inicio del día siguiente.
            var effectiveEndDate = endDate.Date.AddDays(1);
            query = query.Where(p => p.FechaEmision >= startDate.Date && p.FechaEmision < effectiveEndDate);

            // --- Filtro por Proveedor (Sin cambios, ya estaba bien) ---
            if (proveedorId.HasValue && proveedorId.Value > 0)
            {
                query = query.Where(p => p.IdProveedor == proveedorId.Value);
            }

            // --- NUEVO FILTRO: POR ESTADO ---
            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado.Value);
            }

            return await query.OrderByDescending(p => p.FechaEmision).AsNoTracking().ToListAsync();
        }

        public Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId)
        {
            // Si no la vas a usar, es mejor dejar la excepción. Si la necesitas, la implementamos.
            throw new NotImplementedException();
        }

        public async Task<Pedido> GetPedidoWithDetailsAsync(int pedidoId)
        {
            // Tu método existente está perfecto.
            return await _context.Pedidos
                        .Include(p => p.Proveedor)
                            .ThenInclude(pr => pr.Municipio)      // <-- AÑADE ESTO
                                .ThenInclude(m => m.Departamento) // <-- Y AÑADE ESTO
                        .Include(p => p.Usuario)
                        .Include(p => p.DetallesPedidos)
                            .ThenInclude(d => d.Producto)
                        .AsNoTracking() // Es buena práctica para un reporte de solo lectura
                        .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }
    }
}
