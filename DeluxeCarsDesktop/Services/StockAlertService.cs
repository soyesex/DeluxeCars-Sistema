using DeluxeCarsDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public class StockAlertService : IStockAlertService
    {
        private readonly INotificationService _notificationService;

        public StockAlertService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task CheckAndCreateStockAlertAsync(int productoId, IUnitOfWork unitOfWork)
        {
            try
            {
                var producto = await unitOfWork.Productos.GetByIdAsync(productoId);

                // Si no hay producto o no tiene un mínimo configurado, no hacemos nada.
                if (producto == null || !producto.StockMinimo.HasValue || producto.StockMinimo.Value <= 0)
                {
                    return;
                }

                // Usamos el método que ya calcula el stock real
                var currentStock = await unitOfWork.Productos.GetCurrentStockAsync(productoId);

                // La lógica de negocio principal
                if (currentStock < producto.StockMinimo.Value)
                {
                    // ¡El stock está por debajo del mínimo! Creamos una notificación.
                    string title = "Alerta de Stock Bajo";
                    string message = $"El producto '{producto.Nombre}' (ID: {productoId}) tiene solo {currentStock} unidades, por debajo del mínimo de {producto.StockMinimo.Value}.";

                    // Usamos nuestro sistema de notificaciones para crear una alerta de tipo "Advertencia".
                    // NOTA: Para que esto funcione, necesitamos un pequeño ajuste en INotificationService.
                    _notificationService.ShowWarning(message); // Por ahora solo el mensaje
                }
            }
            catch (System.Exception ex)
            {
                // Si algo falla aquí, no debe detener la transacción principal (venta, ajuste).
                // Solo lo registramos para futura depuración.
                Debug.WriteLine($"ERROR al verificar alerta de stock para producto ID {productoId}: {ex.Message}");
            }
        }

    }
}
