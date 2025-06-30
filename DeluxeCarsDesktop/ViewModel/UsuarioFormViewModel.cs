using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class UsuarioFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private Usuario _usuarioActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding ---
        public string TituloVentana { get; private set; }
        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set
            {
                SetProperty(ref _nombre, value);
                (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        public byte[] ProfilePicture { get; set; }
        public string Telefono { get; set; }
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        public bool Activo { get; set; }
        private SecureString _password;
        public SecureString Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        private SecureString _confirmPassword;
        public SecureString ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<Rol> RolesDisponibles { get; private set; }
        private Rol _rolSeleccionado;
        public Rol RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                // 1. Actualiza la propiedad.
                SetProperty(ref _rolSeleccionado, value);

                // 2. Notifica al comando en la siguiente línea.
                (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Propiedad para controlar la visibilidad de los campos de contraseña
        public bool IsCreateMode => !_esModoEdicion;

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }
        public ICommand SeleccionarFotoCommand { get; }

        public UsuarioFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            SeleccionarFotoCommand = new ViewModelCommand(ExecuteSeleccionarFotoCommand);
            RolesDisponibles = new ObservableCollection<Rol>();
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand, CanExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(p => CloseAction?.Invoke());
        }

        public async Task LoadAsync(int entityId)
        {
            await LoadRolesAsync();
            if (entityId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _usuarioActual = new Usuario();
                TituloVentana = "Nuevo Usuario";
                Activo = true;
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _usuarioActual = await _unitOfWork.Usuarios.GetByIdAsync(entityId);
                if (_usuarioActual != null)
                {
                    TituloVentana = "Editar Usuario";
                    Nombre = _usuarioActual.Nombre;
                    Telefono = _usuarioActual.Telefono;
                    Email = _usuarioActual.Email;
                    Activo = _usuarioActual.Activo;
                    RolSeleccionado = RolesDisponibles.FirstOrDefault(r => r.Id == _usuarioActual.IdRol);
                }
            }
            // Notificamos a la UI de todos los cambios
            OnPropertyChanged(string.Empty);
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();

                // --- INICIO DE LA CORRECCIÓN ---
                // 1. Limpiamos la colección existente (la que la UI está observando).
                RolesDisponibles.Clear();

                // 2. Añadimos los roles uno por uno. La UI se actualizará con cada 'Add'.
                foreach (var rol in roles.OrderBy(r => r.Nombre))
                {
                    RolesDisponibles.Add(rol);
                }
                // --- FIN DE LA CORRECCIÓN ---
            }
            catch (Exception ex)
            {
                // Es una buena idea manejar errores aquí también
                MessageBox.Show($"No se pudieron cargar los roles de usuario: {ex.Message}", "Error de Carga");
            }
        }

        private bool CanExecuteGuardarCommand(object obj)
        {
            // Validación 1: Campos básicos (esta parte está bien)
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Email) || RolSeleccionado == null)
            {
                return false;
            }

            // --- INICIO DE LA CORRECCIÓN ---
            if (_esModoEdicion)
            {
                // MODO EDICIÓN:
                // Primero, verificamos que las propiedades de contraseña no sean nulas antes de acceder a .Length.
                if ((Password != null && Password.Length > 0) || (ConfirmPassword != null && ConfirmPassword.Length > 0))
                {
                    // Si el usuario ha empezado a escribir en CUALQUIERA de los campos de contraseña, 
                    // entonces AMBOS deben existir y sus valores deben coincidir.
                    if (Password == null || ConfirmPassword == null || Password.Unsecure() != ConfirmPassword.Unsecure())
                    {
                        return false;
                    }
                }
            }
            else // MODO CREACIÓN
            {
                // Para usuarios nuevos, ambas contraseñas son obligatorias y deben coincidir.
                if (Password == null || Password.Length == 0 || ConfirmPassword == null || ConfirmPassword.Length == 0)
                {
                    return false;
                }

                if (Password.Unsecure() != ConfirmPassword.Unsecure())
                {
                    return false;
                }
            }
            // --- FIN DE LA CORRECCIÓN ---

            // Si todas las validaciones pasan, el botón se habilita.
            return true;
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            _usuarioActual.Nombre = Nombre;
            _usuarioActual.Telefono = Telefono;
            _usuarioActual.Email = Email;
            _usuarioActual.Activo = Activo;
            _usuarioActual.IdRol = RolSeleccionado.Id;
            _usuarioActual.ProfilePicture = this.ProfilePicture;

            try
            {
                if (_esModoEdicion)
                {
                    // Si el usuario proveyó una nueva contraseña, la actualizamos.
                    if (Password != null && Password.Length > 0)
                    {
                        // Lógica para actualizar contraseña de un usuario existente
                        await _unitOfWork.Usuarios.UpdateUserPassword(_usuarioActual.Id, Password.Unsecure());
                    }
                    await _unitOfWork.Usuarios.UpdateAsync(_usuarioActual);
                }
                else
                {
                    await _unitOfWork.Usuarios.RegisterUser(_usuarioActual, Password.Unsecure());
                }

                await _unitOfWork.CompleteAsync();
                MessageBox.Show("Usuario guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el usuario. Es posible que el email ya exista.\n\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSeleccionarFotoCommand(object obj)
        {
            // 1. Creamos un diálogo para abrir archivos
            var dialog = new OpenFileDialog();

            // 2. Filtramos para que solo se puedan seleccionar imágenes
            dialog.Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            // 3. Mostramos el diálogo. Si el usuario selecciona un archivo y da "OK"...
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 4. Leemos todos los bytes del archivo seleccionado
                    byte[] imageData = File.ReadAllBytes(dialog.FileName);

                    // 5. Asignamos los datos a nuestra propiedad. La UI se actualizará
                    //    automáticamente para mostrar la nueva imagen gracias al Binding.
                    ProfilePicture = imageData;
                    OnPropertyChanged(nameof(ProfilePicture));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al leer el archivo de imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}