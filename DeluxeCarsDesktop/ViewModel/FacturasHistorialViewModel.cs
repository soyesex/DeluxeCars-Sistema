using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
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
    public class FacturasHistorialViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private List<Factura> _todasLasFacturas;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FiltrarFacturas();
            }
        }

        private ObservableCollection<Factura> _facturas;
        public ObservableCollection<Factura> Facturas
        {
            get => _facturas;
            private set => SetProperty(ref _facturas, value);
        }

        private Factura _facturaSeleccionada;
        public Factura FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set
            {
                SetProperty(ref _facturaSeleccionada, value);
                (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (AnularFacturaCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (RegistrarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (LiquidarCreditoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); // <-- AÑADE ESTA LÍNEA
            }
        }


        public event Action OnRequestNuevaFactura;
        public ICommand NuevaFacturaCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand AnularFacturaCommand { get; }
        public ICommand RefrescarCommand { get; }
        public ICommand RegistrarPagoCommand { get; }
        public ICommand LiquidarCreditoCommand { get; }
        public FacturasHistorialViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todasLasFacturas = new List<Factura>();
            Facturas = new ObservableCollection<Factura>();

            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetallesCommand, CanExecuteActions);
            AnularFacturaCommand = new ViewModelCommand(ExecuteAnularFacturaCommand, CanExecuteActions);
            RefrescarCommand = new ViewModelCommand(async p => await LoadAsync());
            RegistrarPagoCommand = new ViewModelCommand(ExecuteRegistrarPago, CanExecuteRegistrarPago);
            NuevaFacturaCommand = new ViewModelCommand(p => OnRequestNuevaFactura?.Invoke());
            LiquidarCreditoCommand = new ViewModelCommand(async _ => await ExecuteLiquidarCredito(), _ => CanExecuteLiquidarCredito()); 
        }

        public async Task LoadAsync()
        {
            try
            {
                // Llamamos al nuevo método que incluye los detalles
                var facturasDesdeRepo = await _unitOfWork.Facturas.GetAllWithClienteYMetodoPagoAsync();
                _todasLasFacturas = facturasDesdeRepo.ToList();
                FiltrarFacturas(); // Tu lógica de filtrado
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar las facturas: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarFacturas()
        {
            IEnumerable<Factura> itemsFiltrados = _todasLasFacturas;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLower();
                itemsFiltrados = itemsFiltrados.Where(f =>
                    f.NumeroFactura.ToLower().Contains(lowerSearchText) ||
                    (f.Cliente?.Nombre.ToLower().Contains(lowerSearchText) ?? false) || // Se busca en el nombre del cliente
                    f.Total.ToString().Contains(SearchText) // Se busca en el total como texto
                );
            }

            Facturas = new ObservableCollection<Factura>(itemsFiltrados.OrderByDescending(f => f.FechaEmision));
        }
        private bool CanExecuteActions(object obj) => FacturaSeleccionada != null;
        private bool CanExecuteRegistrarPago(object obj)
        {
            return FacturaSeleccionada != null && FacturaSeleccionada.EstadoPago != EstadoPagoFactura.Pagada;
        }
        private async void ExecuteRegistrarPago(object obj)
        {
            // 1. Verificamos que se pueda ejecutar (buena práctica)
            if (!CanExecuteRegistrarPago(obj)) return;

            // 2. Quitamos el MessageBox y descomentamos la línea de navegación
            await _navigationService.OpenFormWindow(FormType.RegistrarPagoCliente, FacturaSeleccionada.Id);

            // 3. Refrescamos la lista después de que la ventana se cierre
            await LoadAsync();
        }
        private void ExecuteVerDetallesCommand(object obj)
        {
            // Lógica para abrir una ventana que muestre los detalles de la FacturaSeleccionada
            MessageBox.Show($"Mostrando detalles para la factura: {FacturaSeleccionada.NumeroFactura}", "Información");
        }

        private async void ExecuteAnularFacturaCommand(object obj)
        {
            if (!CanExecuteActions(obj)) return;

            // ANTES: Mostraba un MessageBox
            // MessageBox.Show("Funcionalidad de anular (Nota de Crédito) pendiente de implementación.", "Información");

            // AHORA: Llama al servicio de navegación para abrir el nuevo formulario
            await _navigationService.OpenFormWindow(FormType.NotaDeCredito, FacturaSeleccionada.Id);

            // Al volver, refrescamos por si la factura cambió de estado
            await LoadAsync();
        }

        private bool CanExecuteLiquidarCredito()
        {
            // El botón solo estará activo si hay una factura seleccionada Y su saldo es negativo.
            return FacturaSeleccionada != null && FacturaSeleccionada.SaldoPendiente < 0;
        }

        private async Task ExecuteLiquidarCredito()
        {
            if (!CanExecuteLiquidarCredito()) return;

            // Pedimos confirmación al usuario
            var confirmacion = MessageBox.Show(
                $"Esto registrará un reembolso por el crédito de {FacturaSeleccionada.SaldoPendiente:C} y el saldo de la factura volverá a ser $0.00.\n\n¿Desea continuar?",
                "Confirmar Liquidación de Saldo a Favor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.No) return;

            try
            {
                // Creamos una transacción POSITIVA que cancela exactamente el crédito negativo.
                var pagoReembolso = new PagoCliente
                {
                    IdCliente = FacturaSeleccionada.IdCliente,
                    IdMetodoPago = FacturaSeleccionada.IdMetodoPago, // Asumimos el mismo método
                    IdUsuario = _unitOfWork.Usuarios.GetAdminUserAsync().Result.Id, // Asignamos al admin por defecto

                    // ¡LA CLAVE! El monto es el saldo pendiente negativo, multiplicado por -1 para hacerlo positivo.
                    MontoRecibido = FacturaSeleccionada.SaldoPendiente * -1,

                    FechaPago = DateTime.Now,
                    Referencia = $"Liquidación de crédito para Factura {FacturaSeleccionada.NumeroFactura}",
                    FacturasCubiertas = new List<PagoClienteFactura>()
                };

                // Creamos la "grapa" para vincularlo a la factura original
                var enlace = new PagoClienteFactura { IdFactura = FacturaSeleccionada.Id, PagoCliente = pagoReembolso };
                pagoReembolso.FacturasCubiertas.Add(enlace);

                await _unitOfWork.PagosClientes.AddAsync(pagoReembolso);
                await _unitOfWork.CompleteAsync();

                // Refrescamos la lista para ver el resultado
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al liquidar el crédito: {ex.Message}", "Error");
            }
        }
    }
}