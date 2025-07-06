using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
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
        private bool _isSearching = false;

        // --- Propiedades para Filtros ---
        private string _searchText;
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public ObservableCollection<Cliente> ClientesDisponibles { get; private set; }
        public Cliente ClienteFiltro { get; set; }
        public ObservableCollection<EstadoPagoFactura?> EstadosPago { get; }
        public EstadoPagoFactura? EstadoPagoFiltro { get; set; }

        // --- Colección de Datos y Selección ---
        private ObservableCollection<Factura> _facturas;
        public ObservableCollection<Factura> Facturas { get => _facturas; private set => SetProperty(ref _facturas, value); }

        private Factura _facturaSeleccionada;
        public Factura FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set
            {
                SetProperty(ref _facturaSeleccionada, value);
                ActualizarEstadoComandos();
            }
        }

        // --- Paginación ---
        public int NumeroDePagina { get; set; } = 1;
        public int TamañoDePagina { get; set; } = 15;
        private int _totalItems;
        public int TotalItems { get => _totalItems; private set => SetProperty(ref _totalItems, value); }
        public int TotalPaginas => (TotalItems == 0) ? 1 : (int)Math.Ceiling((double)TotalItems / TamañoDePagina);

        // --- Comandos ---
        public ICommand AplicarFiltrosCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand AnularFacturaCommand { get; }
        public ICommand RegistrarPagoCommand { get; }
        public ICommand LiquidarCreditoCommand { get; }
        public ICommand IrAPaginaSiguienteCommand { get; }
        public ICommand IrAPaginaAnteriorCommand { get; }
        public ICommand NuevaFacturaCommand { get; }
        public event Action OnRequestNuevaFactura;

        public FacturasHistorialViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Facturas = new ObservableCollection<Factura>();
            ClientesDisponibles = new ObservableCollection<Cliente>();
            EstadosPago = new ObservableCollection<EstadoPagoFactura?> { null, EstadoPagoFactura.Pendiente, EstadoPagoFactura.Abonada, EstadoPagoFactura.Pagada };

            // --- Instanciación de Comandos Corregida ---
            AplicarFiltrosCommand = new ViewModelCommand(async _ => await AplicarFiltros());
            LimpiarFiltrosCommand = new ViewModelCommand(async _ => await ExecuteLimpiarFiltros());
            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetallesCommand, CanExecuteActions);
            AnularFacturaCommand = new ViewModelCommand(async _ => await ExecuteAnularFacturaCommand(), CanExecuteActions);
            RegistrarPagoCommand = new ViewModelCommand(async _ => await ExecuteRegistrarPago(), CanExecuteRegistrarPago);
            LiquidarCreditoCommand = new ViewModelCommand(async _ => await ExecuteLiquidarCredito(), CanExecuteLiquidarCredito);
            NuevaFacturaCommand = new ViewModelCommand(_ => OnRequestNuevaFactura?.Invoke());

            IrAPaginaSiguienteCommand = new ViewModelCommand(async _ => { if (NumeroDePagina < TotalPaginas) { NumeroDePagina++; await AplicarFiltros(); } }, _ => NumeroDePagina < TotalPaginas);
            IrAPaginaAnteriorCommand = new ViewModelCommand(async _ => { if (NumeroDePagina > 1) { NumeroDePagina--; await AplicarFiltros(); } }, _ => NumeroDePagina > 1);
        }

        public async Task LoadAsync()
        {
            await CargarFiltrosDisponibles();
            await AplicarFiltros();
        }

        private async Task CargarFiltrosDisponibles()
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            ClientesDisponibles.Clear();
            ClientesDisponibles.Add(new Cliente { Id = 0, Nombre = "Todos los Clientes" });
            foreach (var cliente in clientes.OrderBy(c => c.Nombre))
            {
                ClientesDisponibles.Add(cliente);
            }
            ClienteFiltro = ClientesDisponibles.FirstOrDefault();
            OnPropertyChanged(nameof(ClienteFiltro));
        }

        private async Task AplicarFiltros()
        {
            if (_isSearching) return;
            _isSearching = true;
            try
            {
                var criteria = new FacturaSearchCriteria
                {
                    SearchText = this.SearchText,
                    FechaInicio = this.FechaInicio,
                    FechaFin = this.FechaFin,
                    ClienteId = (this.ClienteFiltro?.Id > 0) ? this.ClienteFiltro.Id : null,
                    EstadoPago = this.EstadoPagoFiltro,
                    PageNumber = this.NumeroDePagina,
                    PageSize = this.TamañoDePagina
                };

                var pagedResult = await _unitOfWork.Facturas.SearchAsync(criteria);
                Facturas = new ObservableCollection<Factura>(pagedResult.Items);
                TotalItems = pagedResult.TotalCount;

                OnPropertyChanged(nameof(TotalPaginas));
                OnPropertyChanged(nameof(NumeroDePagina));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar facturas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isSearching = false;
                ActualizarEstadoComandos();
            }
        }

        private async Task ExecuteLimpiarFiltros()
        {
            SearchText = string.Empty;
            FechaInicio = null;
            FechaFin = null;
            ClienteFiltro = ClientesDisponibles.FirstOrDefault();
            EstadoPagoFiltro = null;
            NumeroDePagina = 1;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(FechaInicio));
            OnPropertyChanged(nameof(FechaFin));
            OnPropertyChanged(nameof(ClienteFiltro));
            OnPropertyChanged(nameof(EstadoPagoFiltro));

            await AplicarFiltros();
        }

        private void ActualizarEstadoComandos()
        {
            (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (AnularFacturaCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (RegistrarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (LiquidarCreditoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
        }

        // --- Lógica de los Comandos ---

        private bool CanExecuteActions(object obj) => FacturaSeleccionada != null;
        private bool CanExecuteRegistrarPago(object obj) => FacturaSeleccionada != null && FacturaSeleccionada.EstadoPago != EstadoPagoFactura.Pagada;
        private bool CanExecuteLiquidarCredito(object obj) => FacturaSeleccionada != null && FacturaSeleccionada.SaldoPendiente < 0;

        private void ExecuteVerDetallesCommand(object obj)
        {
            MessageBox.Show($"Mostrando detalles para la factura: {FacturaSeleccionada.NumeroFactura}", "Información");
        }

        private async Task ExecuteAnularFacturaCommand()
        {
            if (!CanExecuteActions(null)) return;
            await _navigationService.OpenFormWindow(FormType.NotaDeCredito, FacturaSeleccionada.Id);
            await AplicarFiltros();
        }

        private async Task ExecuteRegistrarPago()
        {
            if (!CanExecuteRegistrarPago(null)) return;
            await _navigationService.OpenFormWindow(FormType.RegistrarPagoCliente, FacturaSeleccionada.Id);
            await AplicarFiltros();
        }

        private async Task ExecuteLiquidarCredito()
        {
            if (!CanExecuteLiquidarCredito(null)) return;

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
            finally
            {
                await AplicarFiltros();
            }
        }
    }
}