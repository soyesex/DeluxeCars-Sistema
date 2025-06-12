using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RegistroViewModel : ViewModelBase
    {
        // Campos Privados
        private string _nombreUsuario;
        private string _telefonoUsuario;
        private string _emailUsuario;
        private SecureString _password;
        private SecureString _confirmPassword;
        private string _rolUsuario; // Temporalmente como string para coincidir con tu TextBox
        private string _errorMessage;
        private bool _isViewVisible = true;
        public ObservableCollection<Roles> RolesDisponibles { get; set; }
        public Roles RolSeleccionado { get; set; }
        private readonly IRolesRepository _rolRepository; // Repositorio para obtener roles
        private readonly IUsuarioRepository _usuarioRepository; // Repositorio para registrar usuarios
        public event Action RegistrationCancelled;

        // Propiedades Públicas (enlazadas a la Vista)
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                _nombreUsuario = value;
                OnPropertyChanged(nameof(NombreUsuario));
            }
        }

        public string TelefonoUsuario
        {
            get => _telefonoUsuario;
            set
            {
                _telefonoUsuario = value;
                OnPropertyChanged(nameof(TelefonoUsuario));
            }
        }

        public string EmailUsuario
        {
            get => _emailUsuario;
            set
            {
                _emailUsuario = value;
                OnPropertyChanged(nameof(EmailUsuario));
            }
        }

        public SecureString Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public SecureString ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string RolUsuario
        {
            get => _rolUsuario;
            set
            {
                _rolUsuario = value;
                OnPropertyChanged(nameof(RolUsuario));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsViewVisible
        {
            get => _isViewVisible;
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // Comandos
        public ICommand RegistrarCommand { get; }
        public ICommand NavigateBackToLoginCommand { get; }

        // Constructor
        public RegistroViewModel(IUsuarioRepository usuarioRepository, IRolesRepository rolesRepository)
        {
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolesRepository;

            // Inicializamos el comando
            RegistrarCommand = new ViewModelCommand(ExecuteRegistrarCommand, CanExecuteRegistrarCommand);
            NavigateBackToLoginCommand = new ViewModelCommand(ExecuteBackToLoginCommand);

            RolesDisponibles = new ObservableCollection<Roles>();
            CargarRoles(); // Llama a un método que llena la lista
        }

        private void ExecuteBackToLoginCommand(object obj)
        {
            RegistrationCancelled?.Invoke();
        }

        private async void CargarRoles()
        {
            // Lógica para obtener los roles de la base de datos y añadirlos a RolesDisponibles
            var roles = await _rolRepository.GetAllAsync();
            foreach (var rol in roles)
            {
                RolesDisponibles.Add(rol);
            }
        }

        // Lógica del Comando
        private bool CanExecuteRegistrarCommand(object obj)
        {
            // El botón de registrar estará habilitado solo si todos los campos requeridos no están vacíos.
            return !string.IsNullOrWhiteSpace(NombreUsuario) &&
                   !string.IsNullOrWhiteSpace(EmailUsuario) &&
                   Password != null && Password.Length > 0 &&
                   ConfirmPassword != null && ConfirmPassword.Length > 0;
        }

        private async void ExecuteRegistrarCommand(object obj)
        {
            ErrorMessage = "";

            // --- VALIDACIÓN #1: Asegurarnos de que se ha seleccionado un Rol ---
            if (RolSeleccionado == null)
            {
                ErrorMessage = "Por favor, seleccione un rol para el usuario.";
                return;
            }

            if (Password.Unsecure() != ConfirmPassword.Unsecure())
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return;
            }

            try
            {
                var newUser = new Usuario
                {
                    Nombre = this.NombreUsuario,
                    Telefono = this.TelefonoUsuario,
                    Email = this.EmailUsuario,
                    // Asignamos el Id del rol que SÍ seleccionó el usuario
                    IdRol = this.RolSeleccionado.Id
                };

                // Usamos el repositorio que nos fue inyectado
                await _usuarioRepository.RegisterUser(newUser, Password.Unsecure());

                ErrorMessage = "¡Usuario registrado con éxito!";
            }
            catch (Exception ex)
            {
                // --- AQUÍ ESTÁ LA MEJORA CLAVE ---
                // Construimos un mensaje de error mucho más detallado
                // que incluye los mensajes de todas las excepciones internas.
                var fullErrorMessage = new StringBuilder();
                fullErrorMessage.AppendLine($"Error principal: {ex.Message}");

                Exception innerEx = ex.InnerException;
                int level = 1;
                while (innerEx != null)
                {
                    fullErrorMessage.AppendLine($"  -> Error Interno (Nivel {level}): {innerEx.Message}");
                    innerEx = innerEx.InnerException;
                    level++;
                }

                // Mostramos el informe completo en nuestra UI
                ErrorMessage = fullErrorMessage.ToString();
            }
        }
    }
}
