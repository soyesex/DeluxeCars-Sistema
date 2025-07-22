using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RecepcionPedidoViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessengerService _messengerService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private Pedido _pedidoActual;

        public string Titulo => $"Recepcionar Pedido N° {_pedidoActual?.NumeroPedido}";
        public ObservableCollection<RecepcionPedidoItemViewModel> ItemsARecepcionar { get; }

        public ICommand ConfirmarRecepcionCommand { get; }
        public Action CloseAction { get; set; }

        public RecepcionPedidoViewModel(IUnitOfWork unitOfWork, IMessengerService messengerService, INotificationService notificationService,
                                    IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _messengerService = messengerService;
            _notificationService = notificationService;
            _emailService = emailService;

            ItemsARecepcionar = new ObservableCollection<RecepcionPedidoItemViewModel>();
            ConfirmarRecepcionCommand = new ViewModelCommand(async (p) => await ExecuteConfirmarRecepcion());
        }

        public async Task LoadAsync(int entityId)
        {
            _pedidoActual = await _unitOfWork.Pedidos.GetPedidoWithDetailsAsync(entityId);
            if (_pedidoActual == null) return;

            ItemsARecepcionar.Clear();
            foreach (var detalle in _pedidoActual.DetallesPedidos)
            {
                ItemsARecepcionar.Add(new RecepcionPedidoItemViewModel(detalle));
            }
            OnPropertyChanged(nameof(Titulo));
        }

        // Reemplaza este método completo en tu RecepcionPedidoViewModel.cs

        private async Task ExecuteConfirmarRecepcion()
        {
            var result = MessageBox.Show("¿Está seguro de que desea confirmar la recepción de esta mercancía? Esta acción actualizará el inventario.",
                                         "Confirmar Recepción", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                // CAMBIO: Ya no necesitamos las variables locales para los totales.
                // int cantidadTotalPedida = 0; <--- ELIMINADA
                // int cantidadTotalRecibida = 0; <--- ELIMINADA

                foreach (var item in ItemsARecepcionar)
                {
                    // CAMBIO CLAVE 1: Acumulamos la cantidad recibida.
                    // Sumamos lo que ya se había recibido antes (si es nulo, es 0) con lo que se está recibiendo ahora.
                    item.DetalleOriginal.CantidadRecibida = (item.DetalleOriginal.CantidadRecibida ?? 0) + item.CantidadRecibida;
                    item.DetalleOriginal.NotaRecepcion = item.NotaRecepcion; // Esto se mantiene igual, sobreescribe la nota.

                    // Solo creamos movimiento de inventario si se recibió algo en ESTA transacción.
                    if (item.CantidadRecibida > 0)
                    {
                        var movimiento = new MovimientoInventario
                        {
                            IdProducto = item.DetalleOriginal.IdProducto,
                            Cantidad = item.CantidadRecibida, // La cantidad de ESTA recepción.
                            TipoMovimiento = "Entrada por Compra",
                            Fecha = DateTime.Now,
                            MotivoAjuste = $"Recepción de Pedido N° {_pedidoActual.NumeroPedido}",
                            CostoUnitario = item.DetalleOriginal.PrecioUnitario,
                            IdReferencia = item.DetalleOriginal.Id
                        };
                        await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
                    }
                }

                // CAMBIO CLAVE 2: Calculamos el estado final usando los datos ACTUALIZADOS del pedido.
                var totalOrdenado = _pedidoActual.DetallesPedidos.Sum(d => d.Cantidad);
                var totalRecibidoAcumulado = _pedidoActual.DetallesPedidos.Sum(d => d.CantidadRecibida ?? 0);

                if (totalRecibidoAcumulado >= totalOrdenado)
                {
                    _pedidoActual.Estado = EstadoPedido.Recibido; // ¡Ahora sí se marcará como recibido!
                }
                else if (totalRecibidoAcumulado > 0)
                {
                    _pedidoActual.Estado = EstadoPedido.RecibidoParcialmente;
                }
                // Si totalRecibidoAcumulado es 0, no cambiamos el estado (sigue 'Aprobado').

                _pedidoActual.FechaRecepcionReal = DateTime.Now;

                // Guardamos TODOS los cambios en una única transacción atómica.
                await _unitOfWork.CompleteAsync();

                try
                {
                    // Solo si el pedido está Recibido o Parcialmente, notificamos.
                    if (_pedidoActual.Estado == EstadoPedido.Recibido || _pedidoActual.Estado == EstadoPedido.RecibidoParcialmente)
                    {
                        await _emailService.EnviarEmailPedidoRecibido(_pedidoActual);
                    }
                    _notificationService.ShowSuccess("¡Mercancía recepcionada y notificación enviada!");
                }
                catch (Exception ex)
                {
                    // El inventario se actualizó, pero el correo falló.
                    _notificationService.ShowWarning($"Recepción guardada, pero falló el envío del correo: {ex.Message}");
                }

                _messengerService.Publish(new InventarioCambiadoMessage());
                _notificationService.ShowSuccess("¡Mercancía recepcionada! El inventario ha sido actualizado.");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ocurrió un error crítico al procesar la recepción: {ex.Message}");
            }
        }
    }
}
