using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Utils;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RegistroViewModel : ViewModelBase
    {
        // --- Dependencia ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISnackbarMessageQueue _messageQueue;

        // --- Campos Privados ---
        private string _nombreUsuario;
        private string _emailUsuario;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private bool _isViewVisible = true;

        public ObservableCollection<Rol> RolesDisponibles { get; private set; }
        private Rol _rolSeleccionado;

        public event Action RegistrationCancelled;
        public Action CloseAction { get => RegistrationCancelled; set => RegistrationCancelled = value; }

        // --- Propiedades Públicas (Se respetan los nombres de tu XAML) ---
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                SetProperty(ref _nombreUsuario, value);
                // Le avisamos al comando que revise las reglas de nuevo
                (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        private string _telefonoCrudo;

        // Esta propiedad es la que se conectará al TextBox en la vista.
        private string _telefonoFormateado;
        public string TelefonoFormateado
        {
            get => _telefonoFormateado;
            set
            {
                // Guarda para evitar error si el valor es nulo o vacío
                if (string.IsNullOrEmpty(value))
                {
                    _telefonoCrudo = string.Empty;
                    SetProperty(ref _telefonoFormateado, string.Empty);
                    return;
                }

                // Paso 1: Calculamos SIEMPRE primero el valor crudo a partir de la entrada.
                string textoCrudoTemp = new string(value.Where(char.IsDigit).ToArray());
                _telefonoCrudo = textoCrudoTemp; // Actualizamos el campo privado

                // Paso 2: Aplicamos el formato usando el valor que acabamos de calcular.
                string textoFormateado = textoCrudoTemp;
                if (textoCrudoTemp.Length > 3 && textoCrudoTemp.Length <= 6)
                {
                    textoFormateado = $"{textoCrudoTemp.Substring(0, 3)} {textoCrudoTemp.Substring(3)}";
                }
                else if (textoCrudoTemp.Length > 6)
                {
                    // Aseguramos no pasarnos del largo total
                    string part3 = textoCrudoTemp.Substring(6);
                    textoFormateado = $"{textoCrudoTemp.Substring(0, 3)} {textoCrudoTemp.Substring(3, 3)} {part3}";
                }

                // Paso 3: Actualizamos la propiedad pública, lo que refrescará la UI.
                SetProperty(ref _telefonoFormateado, textoFormateado);
            }
        }
        public string EmailUsuario
        {
            get => _emailUsuario;
            set
            {
                SetProperty(ref _emailUsuario, value);
                // Le avisamos al comando que revise las reglas de nuevo
                (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }


        public Rol RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                // Guardamos el valor anterior en caso de que tengamos que revertir
                var rolAnterior = _rolSeleccionado;
                SetProperty(ref _rolSeleccionado, value);

                // Si el nuevo rol es "Administrador", iniciamos la verificación
                if (value?.Nombre == "Administrador")
                {
                    VerificarPinAdministrador(rolAnterior);
                }
            }
        }

        private bool _isPinDialogOpen;
        public bool IsPinDialogOpen
        {
            get => _isPinDialogOpen;
            set => SetProperty(ref _isPinDialogOpen, value);
        }

        private string _adminPin;
        public string AdminPin
        {
            get => _adminPin;
            set => SetProperty(ref _adminPin, value);
        }

        // Las propiedades de solo lectura o las que no afectan la validación no necesitan cambios.
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        public bool IsViewVisible { get => _isViewVisible; set => SetProperty(ref _isViewVisible, value); }

        // --- Comandos (Se respetan los nombres de tu XAML) ---
        public ICommand RegistrarCommand { get; }
        public ICommand NavigateBackToLoginCommand { get; }
        public ICommand ValidarPinCommand { get; }

        // --- Constructor (Ahora pide IUnitOfWork) ---
        public RegistroViewModel(IUnitOfWork unitOfWork, ISnackbarMessageQueue messageQueue)
        {
            _unitOfWork = unitOfWork;
            _messageQueue = messageQueue;

            RegistrarCommand = new ViewModelCommand(ExecuteRegistrarCommand, CanExecuteRegistrarCommand);
            NavigateBackToLoginCommand = new ViewModelCommand(ExecuteBackToLoginCommand);

            RolesDisponibles = new ObservableCollection<Rol>();

            ValidarPinCommand = new ViewModelCommand(p => VerificarPinAdministrador(null));
        }
        public async Task InitializeAsync()
        {
            await CargarRoles();
        }
        private void ExecuteBackToLoginCommand(object obj)
        {
            RegistrationCancelled?.Invoke();
        }
        private async Task CargarRoles()
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();
                RolesDisponibles.Clear(); // Buena práctica limpiar la colección antes de llenarla
                foreach (var rol in roles.OrderBy(r => r.Nombre))
                {
                    RolesDisponibles.Add(rol);
                }
                RolSeleccionado = RolesDisponibles.FirstOrDefault(r => r.Nombre == "Empleado");
            }
            catch (Exception ex)
            {
                ErrorMessage = "No se pudieron cargar los roles.";
                System.Diagnostics.Debug.WriteLine($"ERROR Cargando Roles: {ex.Message}");
            }
        }

        private bool CanExecuteRegistrarCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(NombreUsuario) &&
                    !string.IsNullOrWhiteSpace(EmailUsuario) &&
                    Utils.ValidationHelper.IsValidEmail(EmailUsuario) &&
                    !string.IsNullOrEmpty(Password) &&
                    Password == ConfirmPassword &&
                    RolSeleccionado != null;
        }


        private async void ExecuteRegistrarCommand(object obj)
        {
            ErrorMessage = "";

            var newUser = new Usuario
            {
                Nombre = this.NombreUsuario,
                Telefono = _telefonoCrudo,
                Email = this.EmailUsuario,
                IdRol = this.RolSeleccionado.Id,
                Activo = true
            };

            try
            {
                await _unitOfWork.Usuarios.RegisterUser(newUser, Password);
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("¡Usuario registrado con éxito!", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                // Tu excelente lógica para mostrar errores detallados
                var fullErrorMessage = new StringBuilder();
                fullErrorMessage.AppendLine($"Error principal: {ex.Message}");
                Exception innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    fullErrorMessage.AppendLine($"  -> Error Interno: {innerEx.Message}");
                    innerEx = innerEx.InnerException;
                }
                ErrorMessage = fullErrorMessage.ToString();
            }
        }

        private async void VerificarPinAdministrador(Rol rolParaRevertir)
        {
            // Mostramos el diálogo para pedir el PIN
            var result = await DialogHost.Show(this, "PinDialog");

            //if (result is bool boolResult && boolResult)
            //{
            //    // El usuario hizo clic en "Aceptar"
            //    bool esPinValido = await _unitOfWork.Configuracion.ValidateAdminPin(this.AdminPin);

            //    if (!esPinValido)
            //    {
            //        _messageQueue.Enqueue("PIN incorrecto. Se ha seleccionado el rol por defecto.");
            //        // Revertimos la selección al rol anterior o a Empleado por defecto
            //        RolSeleccionado = rolParaRevertir ?? RolesDisponibles.FirstOrDefault(r => r.Nombre == "Empleado");
            //    }
            //    // Si es válido, no hacemos nada, la selección se mantiene.
            //}
            //else
            //{
            //    // El usuario hizo clic en "Cancelar" o cerró el diálogo
            //    _messageQueue.Enqueue("La selección de Administrador ha sido cancelada.");
            //    RolSeleccionado = rolParaRevertir ?? RolesDisponibles.FirstOrDefault(r => r.Nombre == "Empleado");
            //}

            // Limpiamos el PIN por seguridad
            AdminPin = string.Empty;
        }
    }
}
