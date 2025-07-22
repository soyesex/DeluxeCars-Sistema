using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsShared.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RegistrarPagoProveedorViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;

        private Pedido _pedidoEnCuestion;

        // Propiedades de solo lectura para la UI
        public string NumeroPedido => _pedidoEnCuestion?.NumeroPedido;
        public string NombreProveedor => _pedidoEnCuestion?.Proveedor?.RazonSocial;
        public decimal SaldoActual => _pedidoEnCuestion?.SaldoPendiente ?? 0;

        // Propiedades enlazables para la entrada de datos
        private decimal _montoAPagar;
        public decimal MontoAPagar
        {
            get => _montoAPagar;
            set { SetProperty(ref _montoAPagar, value); (GuardarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private DateTime _fechaDelPago = DateTime.Now;
        public DateTime FechaDelPago { get => _fechaDelPago; set => SetProperty(ref _fechaDelPago, value); }

        public ObservableCollection<MetodoPago> MetodosDePago { get; private set; }
        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set { SetProperty(ref _metodoPagoSeleccionado, value); (GuardarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _referencia;
        public string Referencia { get => _referencia; set => SetProperty(ref _referencia, value); }

        private string _notas;
        public string Notas { get => _notas; set => SetProperty(ref _notas, value); }

        public ICommand GuardarPagoCommand { get; }
        public Action CloseAction { get; set; }

        public RegistrarPagoProveedorViewModel(IUnitOfWork unitOfWork, INotificationService notificationService, ICurrentUserService currentUserService,
                                           IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _emailService = emailService;

            MetodosDePago = new ObservableCollection<MetodoPago>();
            GuardarPagoCommand = new ViewModelCommand(async _ => await ExecuteGuardarPago(), _ => CanExecuteGuardarPago());
        }

        public async Task LoadAsync(int pedidoId)
        {
            // Llama al nuevo método específico y robusto
            _pedidoEnCuestion = await _unitOfWork.Pedidos.GetForPaymentProcessingAsync(pedidoId);

            if (_pedidoEnCuestion == null)
            {
                _notificationService.ShowError("No se pudo cargar el pedido seleccionado.");
                CloseAction?.Invoke();
                return;
            }

            // El resto del código funcionará correctamente ahora
            var metodos = await _unitOfWork.MetodosPago.GetByConditionAsync(m => m.AplicaParaCompras && m.Disponible);
            MetodosDePago.Clear();
            foreach (var metodo in metodos)
            {
                MetodosDePago.Add(metodo);
            }
            MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();

            // Esta asignación ahora tendrá el valor correcto
            MontoAPagar = _pedidoEnCuestion.SaldoPendiente;

            // Y estas notificaciones mostrarán los datos correctos en la vista
            OnPropertyChanged(nameof(NumeroPedido));
            OnPropertyChanged(nameof(NombreProveedor));
            OnPropertyChanged(nameof(SaldoActual));
        }

        private bool CanExecuteGuardarPago()
        {
            // Validaciones para activar el botón de guardar:
            return MontoAPagar > 0 &&
                   MontoAPagar <= SaldoActual && // No se puede pagar más de lo que se debe.
                   MetodoPagoSeleccionado != null &&
                   _pedidoEnCuestion != null;
        }

        private async Task ExecuteGuardarPago()
        {
            if (!CanExecuteGuardarPago()) return; // Doble validación

            try
            {
                // --- PASO 1: Crear el Recibo de Pago (PagoProveedor) ---
                var nuevoPago = new PagoProveedor
                {
                    IdProveedor = _pedidoEnCuestion.IdProveedor,
                    IdMetodoPago = MetodoPagoSeleccionado.Id,
                    IdUsuario = _currentUserService.CurrentUserId.Value,
                    MontoPagado = this.MontoAPagar,
                    FechaPago = this.FechaDelPago,
                    Referencia = this.Referencia,
                    Notas = this.Notas,
                    PedidosCubiertos = new Collection<PagoProveedorPedido>() // Inicializamos la colección de "grapas"
                };

                // --- PASO 2: Crear la Grapa de Unión (PagoProveedorPedido) ---
                var enlace = new PagoProveedorPedido
                {
                    IdPedido = _pedidoEnCuestion.Id,
                    PagoProveedor = nuevoPago
                };

                // --- PASO 3: Añadir la grapa a la colección del pago ---
                // Al hacer esto, Entity Framework entenderá la relación completa.
                nuevoPago.PedidosCubiertos.Add(enlace);

                // --- PASO 4: Añadir el nuevo pago al contexto ---
                await _unitOfWork.PagosProveedores.AddAsync(nuevoPago);

                // --- PASO 5: Guardar todos los cambios en una sola transacción ---
                await _unitOfWork.CompleteAsync();

                try
                {
                    // Pasamos tanto el pago como el pedido afectado
                    await _emailService.EnviarEmailPagoRealizado(nuevoPago, _pedidoEnCuestion);
                    _notificationService.ShowSuccess("Pago registrado y notificación enviada exitosamente.");
                }
                catch (Exception ex)
                {
                    // El pago se guardó, pero el correo falló.
                    _notificationService.ShowWarning($"Pago registrado, pero falló el envío del correo: {ex.Message}");
                }

                _notificationService.ShowSuccess("Pago registrado exitosamente.");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al guardar el pago: {ex.Message}");
            }
        }
    }
}
