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
    public class RegistrarPagoClienteViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        private Factura _facturaEnCuestion;

        // Propiedades de solo lectura
        public string NumeroFactura => _facturaEnCuestion?.NumeroFactura;
        public string NombreCliente => _facturaEnCuestion?.Cliente?.Nombre;
        public decimal SaldoActual => _facturaEnCuestion?.SaldoPendiente ?? 0;

        // Propiedades para la entrada de datos
        private decimal _montoARecibir;
        public decimal MontoARecibir
        {
            get => _montoARecibir;
            set { SetProperty(ref _montoARecibir, value); (GuardarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private DateTime _fechaDelPago = DateTime.Now;
        public DateTime FechaDelPago { get => _fechaDelPago; set => SetProperty(ref _fechaDelPago, value); }

        public ObservableCollection<MetodoPago> MetodosDePago { get; private set; }
        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado { get => _metodoPagoSeleccionado; set { SetProperty(ref _metodoPagoSeleccionado, value); (GuardarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        private string _referencia;
        public string Referencia { get => _referencia; set => SetProperty(ref _referencia, value); }
        private string _notas;
        public string Notas { get => _notas; set => SetProperty(ref _notas, value); }

        public ICommand GuardarPagoCommand { get; }
        public Action CloseAction { get; set; }

        public RegistrarPagoClienteViewModel(IUnitOfWork unitOfWork, INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            MetodosDePago = new ObservableCollection<MetodoPago>();
            GuardarPagoCommand = new ViewModelCommand(async _ => await ExecuteGuardarPago(), _ => CanExecuteGuardarPago());
        }

        public async Task LoadAsync(int facturaId)
        {
            _facturaEnCuestion = await _unitOfWork.Facturas.GetFacturaWithDetailsAsync(facturaId);
            if (_facturaEnCuestion == null)
            {
                _notificationService.ShowError("No se pudo cargar la factura seleccionada.");
                CloseAction?.Invoke();
                return;
            }

            var metodos = await _unitOfWork.MetodosPago.GetByConditionAsync(m => m.AplicaParaVentas && m.Disponible);
            MetodosDePago.Clear();
            foreach (var metodo in metodos) { MetodosDePago.Add(metodo); }
            MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();

            // Esto ahora funciona correctamente, mostrando el saldo REAL.
            MontoARecibir = _facturaEnCuestion.SaldoPendiente;

            OnPropertyChanged(nameof(NumeroFactura));
            OnPropertyChanged(nameof(NombreCliente));
            OnPropertyChanged(nameof(SaldoActual));
        }

        private bool CanExecuteGuardarPago()
        {
            return MontoARecibir > 0 &&
                   MontoARecibir <= SaldoActual &&
                   MetodoPagoSeleccionado != null &&
                   _facturaEnCuestion != null;
        }

        private async Task ExecuteGuardarPago()
        {
            if (!CanExecuteGuardarPago()) return;

            try
            {
                // 1. Crear el "Recibo de Caja" (PagoCliente)
                var nuevoPago = new PagoCliente
                {
                    IdCliente = _facturaEnCuestion.IdCliente,
                    IdMetodoPago = MetodoPagoSeleccionado.Id,
                    IdUsuario = _currentUserService.CurrentUserId.Value,
                    MontoRecibido = this.MontoARecibir,
                    FechaPago = this.FechaDelPago,
                    Referencia = this.Referencia,
                    Notas = this.Notas,
                    FacturasCubiertas = new Collection<PagoClienteFactura>()
                };

                // 2. Crear la "Grapa" (PagoClienteFactura)
                var enlace = new PagoClienteFactura
                {
                    IdFactura = _facturaEnCuestion.Id,
                    PagoCliente = nuevoPago
                };

                // 3. Unir las piezas
                nuevoPago.FacturasCubiertas.Add(enlace);

                // 4. Añadir el nuevo pago al contexto
                await _unitOfWork.PagosClientes.AddAsync(nuevoPago);

                // 5. Guardar la transacción
                await _unitOfWork.CompleteAsync();

                _notificationService.ShowSuccess("Pago de cliente registrado exitosamente.");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al guardar el pago: {ex.Message}");
            }
        }
    }
}
