using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
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
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private SecureString _password;
        public SecureString Password { get => _password; set => SetProperty(ref _password, value); }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        private bool _isViewVisible = true;
        public bool IsViewVisible { get => _isViewVisible; set => SetProperty(ref _isViewVisible, value); }

        // --- Eventos y Comandos ---
        public event Action LoginSuccess;
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowRegisterViewCommand { get; }

        // --- Constructor (CAMBIO EN LA FIRMA) ---
        public LoginViewModel(IUnitOfWork unitOfWork, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;

            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(ExecuteRecoverPassCommand);
            ShowRegisterViewCommand = new ViewModelCommand(ExecuteShowRegisterView);
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(Username) && Password != null && Password.Length > 0;
        }

        private async void ExecuteLoginCommand(object obj)
        {
            ErrorMessage = ""; // Limpiar error
            try
            {
                // CAMBIO: Usamos el repositorio a través del UnitOfWork
                var authenticatedUser = await _unitOfWork.Usuarios.AuthenticateUser(Username, new NetworkCredential(string.Empty, Password).Password);

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
                    ErrorMessage = "❌ Este usuario ha sido desactivado.";
                }
                else
                {
                    ErrorMessage = "❌ Email o contraseña inválidos.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
            }
        }

        private void ExecuteRecoverPassCommand(object obj)
        {
            // La lógica para la recuperación de contraseña iría aquí.
            ErrorMessage = "Funcionalidad de recuperar contraseña no implementada.";
        }

        private void ExecuteShowRegisterView(object obj)
        {
            // Esta lógica para mostrar la vista de registro está bien.
            var registerView = _serviceProvider.GetService<RegistroView>();
            var registerViewModel = registerView.DataContext as RegistroViewModel;

            if (registerViewModel != null)
            {
                registerViewModel.RegistrationCancelled += () =>
                {
                    registerView.Close();
                };
            }
            registerView.ShowDialog();
        }
    }
}
