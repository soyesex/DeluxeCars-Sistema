using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ClientesViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private bool _isSearching = false;
        private CancellationTokenSource _debounceCts;
        // --- Propiedades para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                // Usamos el método que devuelve un bool para que el if funcione
                if (SetPropertyAndCheck(ref _searchText, value))
                {
                    _ = TriggerDebouncedSearch();
                }
            }
        }

        public ObservableCollection<Cliente> Clientes { get; private set; }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                SetProperty(ref _clienteSeleccionado, value);
                (EditarClienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (ToggleEstadoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (CrearFacturaCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); // --- AÑADIDO ---
            }
        }

        // --- Propiedades para KPIs (NUEVO) ---
        private int _clientesActivos;
        public int ClientesActivos { get => _clientesActivos; private set => SetProperty(ref _clientesActivos, value); }

        private int _nuevosClientesMes;
        public int NuevosClientesMes { get => _nuevosClientesMes; private set => SetProperty(ref _nuevosClientesMes, value); }


        // --- Propiedades de Paginación ---
        private int _numeroDePagina = 1;
        public int NumeroDePagina
        {
            get => _numeroDePagina;
            set { SetProperty(ref _numeroDePagina, value); _ = FiltrarClientes(); }
        }

        private int _tamañoDePagina = 15;
        public int TamañoDePagina
        {
            get => _tamañoDePagina;
            set { SetProperty(ref _tamañoDePagina, value); _ = FiltrarClientes(); }
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            private set { SetProperty(ref _totalItems, value); OnPropertyChanged(nameof(TotalPaginas)); }
        }

        public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / TamañoDePagina);

        public ObservableCollection<string> TiposCliente { get; private set; }
        public ObservableCollection<Municipio> CiudadesDisponibles { get; private set; }

        private string _tipoClienteSeleccionado;
        public string TipoClienteSeleccionado
        {
            get => _tipoClienteSeleccionado;
            set
            {
                // TAMBIÉN usa el debouncer
                if (SetPropertyAndCheck(ref _tipoClienteSeleccionado, value))
                {
                    _ = TriggerDebouncedSearch();
                }
            }
        }

        private Municipio _ciudadSeleccionada;
        public Municipio CiudadSeleccionada
        {
            get => _ciudadSeleccionada;
            set
            {
                // Y TAMBIÉN usa el debouncer
                if (SetPropertyAndCheck(ref _ciudadSeleccionada, value))
                {
                    _ = TriggerDebouncedSearch();
                }
            }
        }


        // --- Comandos ---
        public ICommand NuevoClienteCommand { get; }
        public ICommand EditarClienteCommand { get; }
        public ICommand ToggleEstadoCommand { get; }
        public ICommand CrearFacturaCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand IrAPaginaSiguienteCommand { get; }
        public ICommand IrAPaginaAnteriorCommand { get; }
        public ICommand AplicarFiltrosCommand { get; }

        public ClientesViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Clientes = new ObservableCollection<Cliente>();

            TiposCliente = new ObservableCollection<string> { "Todos", "Taller", "Persona Natural" };
            CiudadesDisponibles = new ObservableCollection<Municipio>();
            
            AplicarFiltrosCommand = new ViewModelCommand(async _ => await FiltrarClientes()); // <-- AÑADE ESTO

            NuevoClienteCommand = new ViewModelCommand(async _ => await ExecuteNuevoCliente());
            EditarClienteCommand = new ViewModelCommand(async _ => await ExecuteEditarCliente(), _ => CanExecuteEditToggle());
            ToggleEstadoCommand = new ViewModelCommand(async _ => await ExecuteToggleEstado(), _ => CanExecuteEditToggle());
            CrearFacturaCommand = new ViewModelCommand(async p => await ExecuteCrearFactura(), p => CanExecuteEditToggle());
            LimpiarFiltrosCommand = new ViewModelCommand(async _ => await ExecuteLimpiarFiltros());
            // DESPUÉS
            IrAPaginaSiguienteCommand = new ViewModelCommand(async _ => { if (NumeroDePagina < TotalPaginas) { NumeroDePagina++; await FiltrarClientes(); } });
            IrAPaginaAnteriorCommand = new ViewModelCommand(async _ => { if (NumeroDePagina > 1) { NumeroDePagina--; await FiltrarClientes(); } });
        }

        public async Task LoadAsync()
        {
            await LoadFiltrosAsync();
            await ExecuteLimpiarFiltros();
            await LoadKpisAsync();
        }
        private async Task LoadFiltrosAsync()
        {
            CiudadesDisponibles.Clear();
            CiudadesDisponibles.Add(new Municipio { Id = 0, Nombre = "Todas las Ciudades" });
            var ciudadesRepo = await _unitOfWork.Municipios.GetAllAsync();
            foreach (var ciudad in ciudadesRepo.OrderBy(c => c.Nombre))
            {
                CiudadesDisponibles.Add(ciudad);
            }
        }
        private async Task FiltrarClientes()
        {
            if (_isSearching) return;
            _isSearching = true;

            try
            {
                // NOTA: Necesitaremos crear el método SearchAsync en el IClienteRepository
                var pagedResult = await _unitOfWork.Clientes.SearchAsync(
                SearchText,
                TipoClienteSeleccionado,
                CiudadSeleccionada?.Id, // Usamos el Id de la ciudad seleccionada
                NumeroDePagina,
                TamañoDePagina);

                TotalItems = pagedResult.TotalCount;
                Clientes.Clear();
                foreach (var cliente in pagedResult.Items)
                {
                    Clientes.Add(cliente);
                }

                (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al filtrar clientes: {ex.Message}");
                MessageBox.Show("Ocurrió un error al cargar los clientes.", "Error");
            }
            finally
            {
                _isSearching = false;
            }
        }
        private async Task LoadKpisAsync()
        {
            try
            {
                ClientesActivos = await _unitOfWork.Clientes.CountActiveAsync();
                NuevosClientesMes = await _unitOfWork.Clientes.CountNewThisMonthAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar KPIs de clientes: {ex.Message}");
            }
        }
        private async Task ExecuteCrearFactura()
        {
            // Verificación de seguridad, aunque el CanExecute ya lo impide.
            if (ClienteSeleccionado == null) return;

            // ¡Aquí está! Llamamos a la nueva versión de NavigateTo
            // Le decimos que navegue a FacturacionViewModel y le pasamos un entero (int)
            // que es el ID del cliente seleccionado.
            await _navigationService.NavigateTo<FacturacionViewModel, int>(ClienteSeleccionado.Id);
        }
        private async Task ExecuteLimpiarFiltros()
        {
            // 1. Asignamos el valor directamente a los campos privados para
            //    EVITAR disparar los setters y las múltiples llamadas a FiltrarClientes().
            _searchText = string.Empty;
            _tipoClienteSeleccionado = TiposCliente.FirstOrDefault();
            _ciudadSeleccionada = CiudadesDisponibles.FirstOrDefault();
            _numeroDePagina = 1; // ¡Importante! También resetea la página a la primera.

            // 2. Notificamos a la UI que todas estas propiedades han cambiado
            //    para que los ComboBox y el TextBox se limpien visualmente.
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(TipoClienteSeleccionado));
            OnPropertyChanged(nameof(CiudadSeleccionada));
            OnPropertyChanged(nameof(NumeroDePagina));

            // 3. Ahora, llamamos a FiltrarClientes UNA SOLA VEZ con todos los filtros ya reseteados.
            await FiltrarClientes();
        }

        private bool CanExecuteEditToggle() => ClienteSeleccionado != null;

        private async Task ExecuteNuevoCliente()
        {
            await _navigationService.OpenFormWindow(FormType.Cliente, 0);
            await FiltrarClientes();
        }

        private async Task ExecuteEditarCliente()
        {
            await _navigationService.OpenFormWindow(FormType.Cliente, ClienteSeleccionado.Id);
            await FiltrarClientes();
        }

        private async Task ExecuteToggleEstado()
        {
            string accion = ClienteSeleccionado.Estado ? "desactivar" : "activar";
            var result = MessageBox.Show($"¿Seguro que deseas {accion} al cliente '{ClienteSeleccionado.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var clienteEnDB = await _unitOfWork.Clientes.GetByIdAsync(ClienteSeleccionado.Id);
                clienteEnDB.Estado = !clienteEnDB.Estado;
                await _unitOfWork.CompleteAsync();
                await FiltrarClientes();
            }
        }

        private async Task TriggerDebouncedSearch()
        {
            try
            {
                // Cancela cualquier búsqueda anterior que estuviera esperando en el retardo
                _debounceCts?.Cancel();
                _debounceCts = new CancellationTokenSource();

                // Espera 500ms. Si el usuario escribe otra letra, este delay se cancelará.
                // Puedes ajustar este tiempo (300-800ms es lo usual).
                await Task.Delay(500, _debounceCts.Token);

                // Si el retardo no fue cancelado, ejecuta el filtro.
                await FiltrarClientes();
            }
            catch (TaskCanceledException)
            {
                // Esto es normal y esperado, no hagas nada.
                // Simplemente significa que el usuario siguió escribiendo.
            }
        }

    }
}
