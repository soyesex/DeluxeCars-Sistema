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
        //Fields
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible = true;
        public event Action LoginSuccess;

        private readonly IServiceProvider _serviceProvider;

        private IUsuarioRepository _usuarioRepository;

        //properties
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));  // 👈 aquí
            }
        }
        public SecureString Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }
        public ICommand ShowRegisterViewCommand { get; }
        public LoginViewModel(IUsuarioRepository usuarioRepository, IServiceProvider serviceProvider)
        {
            _usuarioRepository = usuarioRepository;
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(ExecuteRecoverPassCommand);
            ShowRegisterViewCommand = new ViewModelCommand(ExecuteShowRegisterView);
            _serviceProvider = serviceProvider;
        }
        private void ExecuteShowRegisterView(object obj)
        {
            var registerView = _serviceProvider.GetService<RegistroView>();
            registerView.ShowDialog();
        }
        private void ExecuteRecoverPassCommand(object obj)
        {
            // 1) Instanciamos la ventana de recuperación
            //var recoverWindow = new DeluxeCarsUI.View.RecoverPasswordView();

            //// 2) La mostramos como diálogo (modal) para que el usuario ingrese datos
            //recoverWindow.Owner = Application.Current.Windows
            //                         .OfType<LoginView>()
            //                         .FirstOrDefault(); // que quede centrada respecto al LoginView
            //recoverWindow.ShowDialog();
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 ||
                Password == null || Password.Length < 3)
                validData = false;
            else
                validData = true;
            return validData;

        }
        // La lógica de Login se vuelve asíncrona y usa el nuevo repositorio
        private async void ExecuteLoginCommand(object obj)
        {
            ErrorMessage = ""; // Limpiar error
            try
            {
                // Usamos el metodo que devuelven un objeto Usuario, no un bool
                var authenticatedUser = await _usuarioRepository.AuthenticateUser(Username, Password.Unsecure());

                if (authenticatedUser != null)
                {
                    // Si el método devuelve un objeto Usuario, significa que el login fue exitoso.
                    // ...lógica de éxito...
                    // Establecer identidad para el hilo actual
                    Thread.CurrentPrincipal = new GenericPrincipal(
                        new GenericIdentity(Username), null);

                    // Guardar el nombre de usuario en Settings para sesion persistente
                    Properties.Settings.Default.SavedUsername = Username;
                    Properties.Settings.Default.Save();

                    // Ocultar la vista de login (dispara el evento en App.xaml.cs)
                    LoginSuccess?.Invoke();
                }
                else
                {
                    // Si devuelve null, significa que el email o la contraseña eran incorrectos.
                    ErrorMessage = "Email o contraseña inválido.";
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
                return;
            }


            //var isValidUser = userRepository.AuthenticateUser(new NetworkCredential(Username, Password));
            //if (isValidUser)
            //{
            //    
            //}
            //else
            //{
            //    ErrorMessage = "Nombre de usuario o contraseña invalido";
            //}
        }
    }
}
