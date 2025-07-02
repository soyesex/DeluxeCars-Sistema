using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface INotificacionRepository : IGenericRepository<Notificacion>
    {
        Task<IEnumerable<Notificacion>> GetNotificacionesNoLeidasAsync(int idUsuario);
        Task MarcarComoLeidasAsync(IEnumerable<int> idsNotificaciones);
        Task<Notificacion> GetUnreadSummaryAlertAsync(string tipo, int idUsuario);
        Task<IEnumerable<Notificacion>> GetActiveNotificationsWithDetailsAsync(int userId);
    }
}
