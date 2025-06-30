using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public class PedidoStatusCheckService : IPedidoStatusCheckService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PedidoStatusCheckService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CheckPendingPedidosAsync()
        {
            try
            {
                // 1. Buscamos todos los pedidos pendientes usando nuestro nuevo método especializado
                var pedidosPendientes = await _unitOfWork.Pedidos.GetPendingOrdersWithDetailsAsync(DateTime.Now);

                if (!pedidosPendientes.Any()) return;

                // 2. Buscamos al usuario admin con nuestro otro método especializado
                var adminUser = await _unitOfWork.Usuarios.GetAdminUserAsync();
                if (adminUser == null)
                {
                    Debug.WriteLine("No se encontró un usuario ADMIN para asignarle notificaciones de pedidos.");
                    return;
                }

                // 3. Obtenemos las notificaciones existentes para no crear duplicados (esto se queda igual)
                var notificacionesExistentes = (await _unitOfWork.Notificaciones.GetByConditionAsync(
                    n => n.Tipo == "PedidoPendiente" && !n.Leida && n.IdUsuario == adminUser.Id
                )).ToDictionary(n => n.PedidoId);

                foreach (var pedido in pedidosPendientes)
                {
                    if (notificacionesExistentes.ContainsKey(pedido.Id)) continue;

                    var nuevaNotificacion = new Notificacion
                    {
                        IdUsuario = adminUser.Id,
                        PedidoId = pedido.Id,
                        Tipo = "PedidoPendiente",
                        Titulo = $"Pedido Pendiente: {pedido.NumeroPedido}",

                        // --- LÍNEA MODIFICADA Y ROBUSTA ---
                        // Usamos el operador '?' para acceder de forma segura a la propiedad.
                        // Usamos el operador '??' para dar un valor por defecto si el nombre es nulo o no existe.
                        Mensaje = $"La entrega del proveedor '{pedido.Proveedor?.RazonSocial ?? "Desconocido"}' estimada para el {pedido.FechaEstimadaEntrega:d} debe ser verificada.",

                        FechaCreacion = DateTime.Now,
                        Leida = false
                    };
                    await _unitOfWork.Notificaciones.AddAsync(nuevaNotificacion);
                }

                await _unitOfWork.CompleteAsync();
                AppEvents.RaiseNotificationsChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error crítico en PedidoStatusCheckService: {ex.Message}");
            }
        }
    }
}
