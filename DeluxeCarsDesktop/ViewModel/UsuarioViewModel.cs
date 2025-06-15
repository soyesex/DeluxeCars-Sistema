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
    public class UsuarioViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private List<Usuario> _todosLosUsuarios;

        // --- Propiedades para Binding ---
        public bool IsViewVisible { get; set; } = true; // No necesita OnPropertyChanged si no cambia dinámicamente

        private string _searchText;
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); FiltrarUsuarios(); } }

        private ObservableCollection<Usuario> _usuarios;
        public ObservableCollection<Usuario> Usuarios { get => _usuarios; private set => SetProperty(ref _usuarios, value); }

        private Usuario _usuarioSeleccionado;
        public Usuario UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set
            {
                SetProperty(ref _usuarioSeleccionado, value);
                (EditarUsuarioCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (CambiarPasswordCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (ToggleEstadoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Comandos ---
        public ICommand NuevoUsuarioCommand { get; }
        public ICommand EditarUsuarioCommand { get; }
        public ICommand CambiarPasswordCommand { get; }
        public ICommand ToggleEstadoCommand { get; }

        public UsuarioViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todosLosUsuarios = new List<Usuario>();
            Usuarios = new ObservableCollection<Usuario>();

            NuevoUsuarioCommand = new ViewModelCommand(async p => await ExecuteNuevoUsuarioCommand());
            EditarUsuarioCommand = new ViewModelCommand(async p => await ExecuteEditarUsuarioCommand(), p => CanExecuteActions());
            CambiarPasswordCommand = new ViewModelCommand(async p => await ExecuteCambiarPasswordCommand(), p => CanExecuteActions());
            ToggleEstadoCommand = new ViewModelCommand(async p => await ExecuteToggleEstadoCommand(), p => CanExecuteActions());

            LoadUsuariosAsync();
        }

        private async Task LoadUsuariosAsync()
        {
            try
            {
                // Usamos el método especializado del repositorio
                var usuariosDesdeRepo = await _unitOfWork.Usuarios.GetAllWithRolAsync();
                _todosLosUsuarios = usuariosDesdeRepo.ToList();
                FiltrarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los usuarios: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarUsuarios()
        {
            var itemsFiltrados = _todosLosUsuarios;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(u =>
                    u.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            Usuarios = new ObservableCollection<Usuario>(itemsFiltrados.OrderBy(u => u.Nombre));
        }

        private bool CanExecuteActions() => UsuarioSeleccionado != null;

        private async Task ExecuteNuevoUsuarioCommand()
        {
            await _navigationService.OpenFormWindow(Utils.FormType.Usuario, 0);
            await LoadUsuariosAsync();
        }

        private async Task ExecuteEditarUsuarioCommand()
        {
            await _navigationService.OpenFormWindow(Utils.FormType.Usuario, UsuarioSeleccionado.Id);
            await LoadUsuariosAsync();
        }

        private async Task ExecuteCambiarPasswordCommand()
        {
            await _navigationService.OpenFormWindow(Utils.FormType.CambiarPassword, UsuarioSeleccionado.Id);
            // No recargamos la lista, ya que cambiar la contraseña no altera los datos visibles.
        }

        private async Task ExecuteToggleEstadoCommand()
        {
            var usuario = UsuarioSeleccionado;
            string accion = usuario.Activo ? "desactivar" : "activar";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} al usuario '{usuario.Nombre}'?", "Confirmar Cambio", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                usuario.Activo = !usuario.Activo;
                await _unitOfWork.Usuarios.UpdateAsync(usuario);
                await _unitOfWork.CompleteAsync();
                FiltrarUsuarios(); // Refrescar UI
            }
            catch (Exception ex)
            {
                usuario.Activo = !usuario.Activo; // Revertir
                MessageBox.Show($"Error al cambiar estado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
