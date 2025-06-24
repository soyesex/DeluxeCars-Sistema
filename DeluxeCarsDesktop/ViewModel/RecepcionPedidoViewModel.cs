using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RecepcionPedidoViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessengerService _messengerService;
        private Pedido _pedidoActual;

        public string Titulo => $"Recepcionar Pedido N° {_pedidoActual?.NumeroPedido}";
        public ObservableCollection<RecepcionPedidoItemViewModel> ItemsARecepcionar { get; }

        public ICommand ConfirmarRecepcionCommand { get; }
        public Action CloseAction { get; set; }

        public RecepcionPedidoViewModel(IUnitOfWork unitOfWork, IMessengerService messengerService)
        {
            _unitOfWork = unitOfWork;
            _messengerService = messengerService;

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

        private async Task ExecuteConfirmarRecepcion()
        {
            // Opcional: Confirmación del usuario
            var result = MessageBox.Show("¿Está seguro de que desea confirmar la recepción de esta mercancía? Esta acción actualizará el inventario y no se puede deshacer.",
                                         "Confirmar Recepción", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                // Iteramos sobre los items que el usuario vio en pantalla
                foreach (var item in ItemsARecepcionar)
                {
                    if (item.CantidadRecibida > 0) // Solo creamos movimiento si se recibió algo
                    {
                        var movimiento = new MovimientoInventario
                        {
                            IdProducto = item.DetalleOriginal.IdProducto,
                            Cantidad = item.CantidadRecibida, // La cantidad que el usuario ingresó
                            TipoMovimiento = "Entrada por Compra",
                            Fecha = DateTime.Now,
                            MotivoAjuste = $"Recepción de Pedido N° {_pedidoActual.NumeroPedido}", // Trazabilidad perfecta
                            CostoUnitario = item.DetalleOriginal.PrecioUnitario,
                            IdReferencia = _pedidoActual.Id
                        };
                        await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
                    }
                }

                // Actualizamos el estado y la fecha del pedido principal
                _pedidoActual.Estado = EstadoPedido.Recibido;
                _pedidoActual.FechaRecepcionReal = DateTime.Now;

                // Guardamos TODOS los cambios (nuevos movimientos + actualización del pedido)
                // en una única transacción atómica.
                await _unitOfWork.CompleteAsync();

                _messengerService.Publish(new InventarioCambiadoMessage());

                MessageBox.Show("¡Mercancía recepcionada! El inventario ha sido actualizado.", "Éxito");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error crítico al procesar la recepción: {ex.Message}", "Error");
            }
        }
    }
}
