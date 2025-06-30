using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DeluxeCarsEntities;

namespace DeluxeCarsDesktop.ViewModel
{
    public class UsuarioViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;
        private List<Usuario> _todosLosUsuarios;
        public bool IsAdmin => _currentUserService.IsAdmin;

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

        public UsuarioViewModel(IUnitOfWork unitOfWork, INavigationService navigationService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _currentUserService = currentUserService;

            _todosLosUsuarios = new List<Usuario>();
            Usuarios = new ObservableCollection<Usuario>();

            NuevoUsuarioCommand = new ViewModelCommand(async p => await ExecuteNuevoUsuarioCommand());
            EditarUsuarioCommand = new ViewModelCommand(async p => await ExecuteEditarUsuarioCommand(), p => CanExecuteActions());
            CambiarPasswordCommand = new ViewModelCommand(async p => await ExecuteCambiarPasswordCommand(), p => CanExecuteActions());
            ToggleEstadoCommand = new ViewModelCommand(async p => await ExecuteToggleEstadoCommand(), p => CanExecuteActions());
        }

        public async Task LoadAsync()
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
            await LoadAsync();
        }

        private async Task ExecuteEditarUsuarioCommand()
        {
            await _navigationService.OpenFormWindow(Utils.FormType.Usuario, UsuarioSeleccionado.Id);
            await LoadAsync();
        }

        private async Task ExecuteCambiarPasswordCommand()
        {
            await _navigationService.OpenFormWindow(Utils.FormType.CambiarPassword, UsuarioSeleccionado.Id);
            // No recargamos la lista, ya que cambiar la contraseña no altera los datos visibles.
        }

        private async Task ExecuteToggleEstadoCommand()
        {
            var usuario = UsuarioSeleccionado;
            if (usuario == null) return;

            string accion = usuario.Activo ? "desactivar" : "activar";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} al usuario '{usuario.Nombre}'?", "Confirmar Cambio", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                // 1. Cambiamos el estado en el objeto que tenemos en memoria.
                usuario.Activo = !usuario.Activo;

                // 2. --- CORRECCIÓN ---
                // Como la lista se cargó con 'AsNoTracking', el objeto 'usuario' no está
                // siendo rastreado. Con 'UpdateAsync' le decimos a EF que empiece a 
                // rastrearlo y que lo marque como 'Modificado'.
                await _unitOfWork.Usuarios.UpdateAsync(usuario);

                // 3. Ahora 'CompleteAsync' sabrá que hay un cambio que guardar.
                await _unitOfWork.CompleteAsync();

                // 4. Actualiza la UI. Una forma simple es forzar la notificación en el objeto.
                //    Esto hará que el CheckBox en el DataGrid se actualice visualmente.
                OnPropertyChanged(nameof(UsuarioSeleccionado.Activo));
            }
            catch (Exception ex)
            {
                // Revertimos el cambio en la UI si el guardado falla
                usuario.Activo = !usuario.Activo;
                MessageBox.Show($"Error al cambiar estado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
