using DeluxeCarsDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public interface IStockAlertService
    { /// <summary>
      /// Verifica el stock de un producto y, si está por debajo del mínimo,
      /// crea una notificación persistente.
      /// </summary>
      /// <param name="productoId">El ID del producto a verificar.</param>
      /// <param name="unitOfWork">La instancia de UnitOfWork de la transacción actual para consistencia de datos.</param>
        Task CheckAndCreateStockAlertAsync(int productoId, IUnitOfWork unitOfWork);
    }
}
