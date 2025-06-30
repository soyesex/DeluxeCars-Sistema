using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
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
    public class ClientesViewModel : ViewModelBase
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;

        // --- Estado Interno ---
        private List<Cliente> _todosLosClientes;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                // Usando el helper que acabamos de explicar
                SetProperty(ref _searchText, value);
                FiltrarClientes();
            }
        }

        private ObservableCollection<Cliente> _clientes;
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            private set => SetProperty(ref _clientes, value);
        }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                SetProperty(ref _clienteSeleccionado, value);
                // Actualizamos el estado de los comandos cuando cambia la selección
                (EditarClienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        // --- Comandos ---
        public ICommand NuevoClienteCommand { get; }
        public ICommand EditarClienteCommand { get; }
        public ICommand ToggleEstadoCommand { get; }
        // public ICommand VerHistorialCommand { get; } // Para el futuro


        // --- Constructor ---
        public ClientesViewModel(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _currentUserService = currentUserService;

            _todosLosClientes = new List<Cliente>();
            Clientes = new ObservableCollection<Cliente>();

            // El comando de desactivar ahora también depende del rol del usuario
            ToggleEstadoCommand = new ViewModelCommand(
                async p => await ExecuteToggleEstadoCommand(),
                p => CanExecuteActions());

            NuevoClienteCommand = new ViewModelCommand(ExecuteNuevoClienteCommand);
            EditarClienteCommand = new ViewModelCommand(ExecuteEditarClienteCommand, CanExecuteEditDelete);
            ToggleEstadoCommand = new ViewModelCommand(async p => await ExecuteToggleEstadoCommand(), p => CanExecuteActions());

            LoadClientesAsync();
        }
        private bool CanExecuteActions()
        {
            // El botón solo está activo si hay un cliente seleccionado Y el usuario es admin.
            return ClienteSeleccionado != null && _currentUserService.IsAdmin;
        }

        // --- Métodos de Lógica ---
        private async Task LoadClientesAsync()
        {
            try
            {
                var clientesDesdeRepo = await _unitOfWork.Clientes.GetAllAsync();
                _todosLosClientes = clientesDesdeRepo.ToList();
                FiltrarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los clientes: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FiltrarClientes()
        {
            IEnumerable<Cliente> itemsFiltrados = _todosLosClientes;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLower();
                itemsFiltrados = itemsFiltrados.Where(c =>
                    c.Nombre.ToLower().Contains(lowerSearchText) ||
                    c.Email.ToLower().Contains(lowerSearchText) ||
                    (c.Telefono != null && c.Telefono.Contains(SearchText)) // Teléfono no necesita ToLower
                );
            }

            Clientes = new ObservableCollection<Cliente>(itemsFiltrados.OrderBy(c => c.Nombre));
        }
        private bool CanExecuteEditDelete(object obj)
        {
            return ClienteSeleccionado != null;
        }
        private async void ExecuteNuevoClienteCommand(object parameter)
        {
            // La lógica para refrescar la lista después de crear se puede mejorar en el futuro
            await _navigationService.OpenFormWindow(Utils.FormType.Cliente, 0);
            await LoadClientesAsync();
        }
        private async void ExecuteEditarClienteCommand(object obj)
        {
            // 1. Llama al servicio de navegación para abrir el formulario en modo "Edición",
            //    pasando el ID de la categoría seleccionada.
            await _navigationService.OpenFormWindow(Utils.FormType.Cliente, ClienteSeleccionado.Id);

            // 2. Al igual que antes, esta línea espera a que el formulario se cierre para refrescar la lista.
            await LoadClientesAsync();
        }

        private async Task ExecuteToggleEstadoCommand()
        {
            var cliente = ClienteSeleccionado;
            string accion = cliente.Estado ? "desactivar" : "activar";
            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} al cliente '{cliente.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                cliente.Estado = !cliente.Estado;
                await _unitOfWork.Clientes.UpdateAsync(cliente);
                await _unitOfWork.CompleteAsync();
                await LoadClientesAsync(); // Recargamos para que se refleje el cambio
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar el estado del cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                cliente.Estado = !cliente.Estado; // Revertimos el cambio en la UI
            }
        }
    }
}
