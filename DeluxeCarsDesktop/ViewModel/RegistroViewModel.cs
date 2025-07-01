using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
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
using System.Windows.Threading;

namespace DeluxeCarsDesktop.ViewModel
{
    // Implementamos IDisposable para limpiar la suscripción al evento
    public class RegistroViewModel : ViewModelBase, IDisposable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISnackbarMessageQueue _messageQueue;
        private readonly IPinLockoutService _pinLockoutService;

        // --- Campos Privados (Mucho más pocos ahora) ---
        private string _nombreUsuario;
        private string _emailUsuario;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private bool _isViewVisible = true;
        private Rol _rolSeleccionado;
        private bool _isPinDialogOpen;
        private string _adminPin;
        private bool _pinVerificadoConExito = false;
        private string _telefonoCrudo;
        private string _telefonoFormateado;

        public ObservableCollection<Rol> RolesDisponibles { get; private set; }
        public event Action RegistrationCancelled;
        public Action CloseAction { get => RegistrationCancelled; set => RegistrationCancelled = value; }

        // --- Propiedades Públicas ---
        public string NombreUsuario { get => _nombreUsuario; set { SetProperty(ref _nombreUsuario, value); (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }
        public string EmailUsuario { get => _emailUsuario; set { SetProperty(ref _emailUsuario, value); (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }
        public string Password { get => _password; set { SetProperty(ref _password, value); (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }
        public string ConfirmPassword { get => _confirmPassword; set { SetProperty(ref _confirmPassword, value); (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        public bool IsViewVisible { get => _isViewVisible; set => SetProperty(ref _isViewVisible, value); }
        public bool IsPinDialogOpen { get => _isPinDialogOpen; set => SetProperty(ref _isPinDialogOpen, value); }
        public string AdminPin { get => _adminPin; set { SetProperty(ref _adminPin, value); (ValidarPinCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }
        public bool IsPinInputEnabled => !_pinLockoutService.IsLockedOut; // Derivada del servicio

        public string TelefonoFormateado
        {
            get => _telefonoFormateado;
            set
            {
                if (string.IsNullOrEmpty(value)) { _telefonoCrudo = string.Empty; SetProperty(ref _telefonoFormateado, string.Empty); return; }
                _telefonoCrudo = new string(value.Where(char.IsDigit).ToArray());
                string textoFormateado = _telefonoCrudo;
                if (_telefonoCrudo.Length > 3 && _telefonoCrudo.Length <= 6) textoFormateado = $"{_telefonoCrudo.Substring(0, 3)} {_telefonoCrudo.Substring(3)}";
                else if (_telefonoCrudo.Length > 6) textoFormateado = $"{_telefonoCrudo.Substring(0, 3)} {_telefonoCrudo.Substring(3, 3)} {_telefonoCrudo.Substring(6)}";
                SetProperty(ref _telefonoFormateado, textoFormateado);
            }
        }

        public Rol RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                if (_rolSeleccionado == value) return;
                var rolAnterior = _rolSeleccionado;
                SetProperty(ref _rolSeleccionado, value);
                if (value?.Nombre == "Administrador" && !_pinVerificadoConExito)
                {
                    MostrarDialogoPin();
                }
                (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        public event Action FocusPinBoxRequested;
        public ICommand RegistrarCommand { get; }
        public ICommand NavigateBackToLoginCommand { get; }
        public ICommand ValidarPinCommand { get; }
        public ICommand CancelarPinCommand { get; }

        public RegistroViewModel(IUnitOfWork unitOfWork, ISnackbarMessageQueue messageQueue, IPinLockoutService pinLockoutService)
        {
            _unitOfWork = unitOfWork;
            _messageQueue = messageQueue;
            _pinLockoutService = pinLockoutService;
            _pinLockoutService.StateChanged += OnLockoutStateChanged;

            RolesDisponibles = new ObservableCollection<Rol>();
            RegistrarCommand = new ViewModelCommand(ExecuteRegistrarCommand, CanExecuteRegistrarCommand);
            NavigateBackToLoginCommand = new ViewModelCommand(ExecuteBackToLoginCommand);
            ValidarPinCommand = new ViewModelCommand(async _ => await ExecuteValidarPinCommand(), CanExecuteValidarPin);
            CancelarPinCommand = new ViewModelCommand(ExecuteCancelarPin);
        }

        private void OnLockoutStateChanged()
        {
            OnPropertyChanged(nameof(IsPinInputEnabled));
            (ValidarPinCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            UpdateDialogErrorMessage();
        }

        private void UpdateDialogErrorMessage()
        {
            if (_pinLockoutService.IsLockedOut)
            {
                var tiempo = _pinLockoutService.RemainingLockoutTime;
                ErrorMessage = $"Demasiados intentos. Intente de nuevo en {tiempo.Minutes}m {tiempo.Seconds}s.";
            }
            else
            {
                ErrorMessage = $"Necesita autorización. {_pinLockoutService.RemainingAttempts} intentos restantes.";
            }
        }

        public async Task InitializeAsync()
        {
            // Primero nos aseguramos de que el servicio de bloqueo esté listo
            await _pinLockoutService.InitializeAsync();

            // Luego cargamos los roles
            await CargarRoles();
        }
        private void ExecuteBackToLoginCommand(object obj) => RegistrationCancelled?.Invoke();

        private async Task CargarRoles()
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();
                RolesDisponibles.Clear();
                foreach (var rol in roles.OrderBy(r => r.Nombre)) RolesDisponibles.Add(rol);
                _rolSeleccionado = RolesDisponibles.FirstOrDefault(r => r.Nombre == "Empleado");
                OnPropertyChanged(nameof(RolSeleccionado));
            }
            catch (Exception ex) { ErrorMessage = $"No se pudieron cargar los roles: {ex.Message}"; }
        }

        private bool CanExecuteRegistrarCommand(object obj)
        {
            bool camposValidos = !string.IsNullOrWhiteSpace(NombreUsuario) && !string.IsNullOrWhiteSpace(EmailUsuario) && Utils.ValidationHelper.IsValidEmail(EmailUsuario) && !string.IsNullOrEmpty(Password) && Password == ConfirmPassword && RolSeleccionado != null;
            if (!camposValidos) return false;
            if (RolSeleccionado.Nombre == "Administrador" && !_pinVerificadoConExito) return false;
            return true;
        }

        private async void ExecuteRegistrarCommand(object obj)
        {
            if (RolSeleccionado.Nombre == "Administrador" && !_pinVerificadoConExito) { ErrorMessage = "Se requiere validación con PIN para registrar un Administrador."; return; }
            ErrorMessage = "";
            var newUser = new Usuario { Nombre = this.NombreUsuario, Telefono = _telefonoCrudo, Email = this.EmailUsuario, IdRol = this.RolSeleccionado.Id, Activo = true };
            try
            {
                await _unitOfWork.Usuarios.RegisterUser(newUser, Password);
                await _unitOfWork.CompleteAsync();
                MessageBox.Show("¡Usuario registrado con éxito!", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex) { ErrorMessage = $"Error al registrar: {ex.InnerException?.Message ?? ex.Message}"; }
        }

        private void MostrarDialogoPin()
        {
            UpdateDialogErrorMessage();
            AdminPin = string.Empty;
            IsPinDialogOpen = true;
        }

        private void ExecuteCancelarPin(object obj)
        {
            IsPinDialogOpen = false;
            RolSeleccionado = RolesDisponibles.FirstOrDefault(r => r.Nombre == "Empleado");
            _messageQueue.Enqueue("Selección de Administrador cancelada.");
        }

        private bool CanExecuteValidarPin(object obj)
        {
            return !_pinLockoutService.IsLockedOut && !string.IsNullOrEmpty(AdminPin);
        }

        private async Task ExecuteValidarPinCommand()
        {
            bool esValido = await _unitOfWork.ValidarPinAdministradorAsync(AdminPin);
            if (esValido)
            {
                _pinVerificadoConExito = true;
                IsPinDialogOpen = false;
                await _pinLockoutService.Reset(); // Usamos await y mantenemos el Reset aquí
                ErrorMessage = "";
                _messageQueue.Enqueue("✅ Autorización correcta.");
                (RegistrarCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
            else
            {
                await _pinLockoutService.RecordFailedAttempt(); // Usamos await
                AdminPin = string.Empty;
                FocusPinBoxRequested?.Invoke();
            }
        }

        // CORRECTA IMPLEMENTACIÓN DE IDISPOSABLE
        public void Dispose()
        {
            // Nos desuscribimos del evento para evitar fugas de memoria
            if (_pinLockoutService != null)
            {
                _pinLockoutService.StateChanged -= OnLockoutStateChanged;
            }
        }
    }
}
