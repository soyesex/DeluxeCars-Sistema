using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Models.Notifications;
using DeluxeCarsDesktop.ViewModel;
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
        private readonly ICurrentUserService _currentUserService;
        // Ya no necesita INotificationService ni INavigationService
        
        public StockAlertService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public async Task CheckAndCreateStockAlertAsync(int productoId, IUnitOfWork unitOfWork)
        {
            // Este método ahora solo se preocupa por la persistencia de datos.
            try
            {
                var producto = await unitOfWork.Productos.GetByIdAsync(productoId);
                if (producto == null || !producto.StockMinimo.HasValue || producto.StockMinimo.Value <= 0) return;

                var currentStock = await unitOfWork.Productos.GetCurrentStockAsync(productoId);
                // Si el stock está bien, no hacemos nada.
                if (currentStock >= producto.StockMinimo.Value) return;

                if (_currentUserService.CurrentUser == null) return;
                int userId = _currentUserService.CurrentUser.Id;
                string tipoAlerta = "LowStockSummary";

                int totalProductosBajoStock = await unitOfWork.Productos.CountLowStockProductsAsync();
                var notificacionExistenteDB = await unitOfWork.Notificaciones.GetUnreadSummaryAlertAsync(tipoAlerta, userId);
                int ultimoConteo = notificacionExistenteDB?.DataCount ?? -1;

                // La regla de negocio clave: ¿empeoró la situación?
                bool esAlertaNueva = totalProductosBajoStock > ultimoConteo;

                if (notificacionExistenteDB != null)
                {
                    notificacionExistenteDB.Mensaje = $"Hay {totalProductosBajoStock} producto(s) con bajo stock...";
                    notificacionExistenteDB.FechaCreacion = DateTime.Now;
                    notificacionExistenteDB.DataCount = totalProductosBajoStock;
                    if (esAlertaNueva)
                    {
                        notificacionExistenteDB.Leida = false; // ¡Despertamos la alerta!
                    }
                }
                else if (totalProductosBajoStock > 0)
                {
                    var nuevaNotificacion = new Notificacion
                    {
                        Titulo = "Inventario Crítico",
                        Mensaje = $"Hay {totalProductosBajoStock} producto(s) con bajo stock...",
                        Tipo = tipoAlerta,
                        FechaCreacion = DateTime.Now,
                        Leida = false,
                        IdUsuario = userId,
                        DataCount = totalProductosBajoStock
                    };
                    await unitOfWork.Notificaciones.AddAsync(nuevaNotificacion);
                }

                await unitOfWork.CompleteAsync();
                AppEvents.RaiseNotificationsChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR en el sistema de alertas de stock: {ex.Message}");
            }
        }
    }
}
