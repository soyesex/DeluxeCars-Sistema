using DeluxeCarsUI.Model;
using DeluxeCarsUI.Repositories;
using DeluxeCarsUI.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsUI.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        private IUserRepository userRepository;

        //properties
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
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
        public ICommand RememberPasswordCommand {  get; }
        public LoginViewModel()
        {
            userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(ExecuteRecoverPassCommand);
        }
        private void ExecuteRecoverPassCommand(object obj)
        {
            // 1) Instanciamos la ventana de recuperación
            /*var recoverWindow = new DeluxeCarsUI.View.RecoverPasswordView();

            // 2) La mostramos como diálogo (modal) para que el usuario ingrese datos
            recoverWindow.Owner = Application.Current.Windows
                                     .OfType<LoginView>()
                                     .FirstOrDefault(); // que quede centrada respecto al LoginView
            recoverWindow.ShowDialog();*/
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if(string.IsNullOrWhiteSpace(Username) || Username.Length < 3 ||
                Password == null || Password.Length <3)
                validData = false;
            else
                validData = true;
            return validData;

        }
        private void ExecuteLoginCommand(object obj)
        {
            var isValidUser= userRepository.AuthenticateUser(new NetworkCredential(Username, Password));
            if(isValidUser)
            {
                // Establecer identidad para el hilo actual
                Thread.CurrentPrincipal = new GenericPrincipal(
                    new GenericIdentity(Username), null);

                // Guardar el nombre de usuario en Settings para sesion persistente
                Properties.Settings.Default.SavedUsername = Username;
                Properties.Settings.Default.Save();

                // Ocultar la vista de login (dispara el evento en App.xaml.cs)
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "Nombre de usuario o contraseña invalido";
            }
        }
    }
}
