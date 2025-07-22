using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class NotificacionRepository : GenericRepository<Notificacion>, INotificacionRepository
    {
        public NotificacionRepository(AppDbContext context) : base(context)
        {
        }
        // --- IMPLEMENTACIÓN DEL NUEVO MÉTODO ---
        public async Task<IEnumerable<Notificacion>> GetActiveNotificationsWithDetailsAsync(int userId)
        {
            return await _context.Notificaciones
                .Where(n => n.IdUsuario == userId && !n.Leida && n.Tipo != "LowStockSummary")
                .Include(n => n.Pedido) // Incluye la entidad Pedido relacionada
                    .ThenInclude(p => p.Proveedor) // Y DENTRO de Pedido, incluye al Proveedor
                .OrderByDescending(n => n.FechaCreacion)
                .AsNoTracking() // Buena práctica para consultas de solo lectura
                .ToListAsync();
        }
        public async Task<Notificacion> GetUnreadSummaryAlertAsync(string tipo, int idUsuario)
        {
            return await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.IdUsuario == idUsuario && n.Tipo == tipo && !n.Leida);
        }
        public async Task<IEnumerable<Notificacion>> GetNotificacionesNoLeidasAsync(int idUsuario)
        {
            return await _context.Notificaciones
                                 .Where(n => n.IdUsuario == idUsuario && !n.Leida)
                                 .OrderByDescending(n => n.FechaCreacion)
                                 .ToListAsync();
        }

        public async Task MarcarComoLeidasAsync(IEnumerable<int> idsNotificaciones)
        {
            // Si no hay IDs, no hacemos nada.
            if (idsNotificaciones == null || !idsNotificaciones.Any())
            {
                return;
            }

            var notificaciones = await _context.Notificaciones
                                               .Where(n => idsNotificaciones.Contains(n.Id))
                                               .ToListAsync();

            foreach (var notificacion in notificaciones)
            {
                notificacion.Leida = true;
            }
            // NOTA: No llamamos a SaveChangesAsync aquí. Eso es responsabilidad del UnitOfWork.
        }
    }
}
