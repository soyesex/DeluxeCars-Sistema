using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
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
    public class RolViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Estado Interno ---
        private List<Rol> _todosLosRoles;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); FiltrarRoles(); } }

        private ObservableCollection<Rol> _roles;
        public ObservableCollection<Rol> Roles { get => _roles; private set => SetProperty(ref _roles, value); }

        private Rol _rolSeleccionado;
        public Rol RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                SetProperty(ref _rolSeleccionado, value);
                (EditarRolCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (EliminarRolCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Comandos ---
        public ICommand NuevoRolCommand { get; }
        public ICommand EditarRolCommand { get; }
        public ICommand EliminarRolCommand { get; }

        // --- Constructor ---
        public RolViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todosLosRoles = new List<Rol>();
            Roles = new ObservableCollection<Rol>();

            NuevoRolCommand = new ViewModelCommand(async p => await ExecuteNuevoRolCommand());
            EditarRolCommand = new ViewModelCommand(async p => await ExecuteEditarRolCommand(), p => CanExecuteActions());
            EliminarRolCommand = new ViewModelCommand(async p => await ExecuteEliminarRolCommand(), p => CanExecuteActions());
        }

        // --- Métodos de Lógica ---
        public async Task LoadAsync()
        {
            try
            {
                var rolesDesdeRepo = await _unitOfWork.Roles.GetAllAsync();
                _todosLosRoles = rolesDesdeRepo.ToList();
                FiltrarRoles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los roles: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarRoles()
        {
            var itemsFiltrados = _todosLosRoles;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(r =>
                    r.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (r.Descripcion != null && r.Descripcion.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            Roles = new ObservableCollection<Rol>(itemsFiltrados.OrderBy(r => r.Nombre));
        }

        private bool CanExecuteActions() => RolSeleccionado != null;

        private async Task ExecuteNuevoRolCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Rol, 0);
            await LoadAsync();
        }

        private async Task ExecuteEditarRolCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Rol, RolSeleccionado.Id);
            await LoadAsync();
        }

        private async Task ExecuteEliminarRolCommand()
        {
            var rolAEliminar = RolSeleccionado;

            // Evitar que se eliminen roles críticos del sistema (si aplica)
            if (rolAEliminar.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("El rol 'Administrador' no puede ser eliminado.", "Acción no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"¿Estás seguro de que deseas eliminar el rol '{rolAEliminar.Nombre}'?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            try
            {
                await _unitOfWork.Roles.RemoveAsync(rolAEliminar);
                await _unitOfWork.CompleteAsync();

                _todosLosRoles.Remove(rolAEliminar);
                FiltrarRoles();

                MessageBox.Show("Rol eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar el rol. Es probable que esté asignado a uno o más usuarios.\n\nError: {ex.Message}", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
