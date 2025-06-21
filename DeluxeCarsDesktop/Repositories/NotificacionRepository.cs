using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeluxeCarsDesktop.Repositories
{
    public class NotificacionRepository : GenericRepository<Notificacion>, INotificacionRepository
    {
        public NotificacionRepository(AppDbContext context) : base(context)
        {
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
