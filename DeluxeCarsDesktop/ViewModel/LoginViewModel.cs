using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO: Ahora usamos UnitOfWork
        private readonly IServiceProvider _serviceProvider;

        // --- Propiedades para Binding ---
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                // CADA VEZ QUE CAMBIA EL USUARIO, ACTUALIZAMOS EL ESTADO DEL BOTÓN
                UpdateLoginButtonState();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                // CADA VEZ QUE CAMBIA LA CONTRASEÑA, ACTUALIZAMOS EL ESTADO DEL BOTÓN
                UpdateLoginButtonState();
            }
        }

        private bool _isLoginEnabled;
        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            set => SetProperty(ref _isLoginEnabled, value);
        }

        // --- Eventos y Comandos ---
        public event Action LoginSuccess;
        public event Action ShowPasswordRecoveryViewRequested;
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowRegisterViewCommand { get; }
        public event Action ShowRegisterViewRequested;

        // --- Constructor (CAMBIO EN LA FIRMA) ---
        public LoginViewModel(IUnitOfWork unitOfWork, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;

            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            ShowRegisterViewCommand = new ViewModelCommand(ExecuteShowRegisterView);
            RecoverPasswordCommand = new ViewModelCommand(ExecuteRecoverPasswordCommand);

            UpdateLoginButtonState();
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(Password);
        }
        private void UpdateLoginButtonState()
        {
            // La nueva propiedad IsLoginEnabled es igual al resultado de CanExecute
            IsLoginEnabled = CanExecuteLoginCommand(null);
        }
        private async void ExecuteLoginCommand(object obj)
        {
            if (!Utils.ValidationHelper.IsValidEmail(Username))
            {
                this.ErrorMessage = "❌ Por favor, introduce un formato de correo válido.";
                return; // Detenemos la ejecución si el formato no es válido.
            }

            ErrorMessage = ""; // Limpiar error
            try
            {
                // CAMBIO: Usamos el repositorio a través del UnitOfWork
                var authenticatedUser = await _unitOfWork.Usuarios.AuthenticateUser(Username, Password);

                if (authenticatedUser != null && authenticatedUser.Activo)
                {
                    // Guardar el nombre de usuario para sesión persistente
                    Properties.Settings.Default.SavedUsername = Username;
                    Properties.Settings.Default.Save();

                    // Establecer identidad para el hilo actual
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);

                    // Lanzar el evento para que App.xaml.cs muestre el MainView
                    LoginSuccess?.Invoke();
                }
                else if (authenticatedUser != null && !authenticatedUser.Activo)
                {
                    await ShowTemporaryErrorMessage("❌ Este usuario ha sido desactivado.", 7);
                }
                else
                {
                    await ShowTemporaryErrorMessage("❌ Email o contraseña inválidos.", 7);
                }
            }
            catch (Exception ex)
            {
                await ShowTemporaryErrorMessage($"Error: {ex.Message}", 10);
            }
        }

        private void ExecuteRecoverPasswordCommand(object obj)
        {
            // El ViewModel solo notifica la intención. No crea vistas.
            ShowPasswordRecoveryViewRequested?.Invoke();
        }

        private void ExecuteShowRegisterView(object obj)
        {
            ShowRegisterViewRequested?.Invoke();
        }
    }
}
