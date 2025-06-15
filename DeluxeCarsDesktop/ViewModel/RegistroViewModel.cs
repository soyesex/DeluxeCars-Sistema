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
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RegistroViewModel : ViewModelBase
    {
        // --- Dependencia ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Campos Privados ---
        private string _nombreUsuario;
        private string _telefonoUsuario;
        private string _emailUsuario;
        private SecureString _password;
        private SecureString _confirmPassword;
        private string _errorMessage;
        private bool _isViewVisible = true;

        public ObservableCollection<Rol> RolesDisponibles { get; private set; }
        private Rol _rolSeleccionado;

        public event Action RegistrationCancelled;
        public Action CloseAction { get => RegistrationCancelled; set => RegistrationCancelled = value; }

        // --- Propiedades Públicas (Se respetan los nombres de tu XAML) ---
        public string NombreUsuario { get => _nombreUsuario; set => SetProperty(ref _nombreUsuario, value); }
        public string TelefonoUsuario { get => _telefonoUsuario; set => SetProperty(ref _telefonoUsuario, value); }
        public string EmailUsuario { get => _emailUsuario; set => SetProperty(ref _emailUsuario, value); }
        public SecureString Password { get => _password; set => SetProperty(ref _password, value); }
        public SecureString ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
        public Rol RolSeleccionado { get => _rolSeleccionado; set => SetProperty(ref _rolSeleccionado, value); }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        public bool IsViewVisible { get => _isViewVisible; set => SetProperty(ref _isViewVisible, value); }

        // --- Comandos (Se respetan los nombres de tu XAML) ---
        public ICommand RegistrarCommand { get; }
        public ICommand NavigateBackToLoginCommand { get; }

        // --- Constructor (Ahora pide IUnitOfWork) ---
        public RegistroViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RegistrarCommand = new ViewModelCommand(ExecuteRegistrarCommand, CanExecuteRegistrarCommand);
            NavigateBackToLoginCommand = new ViewModelCommand(ExecuteBackToLoginCommand);

            RolesDisponibles = new ObservableCollection<Rol>();
            CargarRoles();
        }

        private void ExecuteBackToLoginCommand(object obj)
        {
            RegistrationCancelled?.Invoke();
        }

        private async void CargarRoles()
        {
            try
            {
                // Ahora usa el UnitOfWork para obtener los roles
                var roles = await _unitOfWork.Roles.GetAllAsync();
                foreach (var rol in roles.OrderBy(r => r.Nombre))
                {
                    RolesDisponibles.Add(rol);
                }
                // Seleccionamos "Empleado" por defecto, si existe.
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
                   Password != null && Password.Length > 0 &&
                   ConfirmPassword != null && ConfirmPassword.Length > 0 &&
                   RolSeleccionado != null; // Se añade validación de rol
        }

        private async void ExecuteRegistrarCommand(object obj)
        {
            ErrorMessage = "";
            if (Password.Unsecure() != ConfirmPassword.Unsecure())
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return;
            }
            if (RolSeleccionado == null)
            {
                ErrorMessage = "Por favor, seleccione un rol.";
                return;
            }

            var newUser = new Usuario
            {
                Nombre = this.NombreUsuario,
                Telefono = this.TelefonoUsuario,
                Email = this.EmailUsuario,
                IdRol = this.RolSeleccionado.Id,
                Activo = true
            };

            try
            {
                // Usamos el repositorio a través del UnitOfWork
                await _unitOfWork.Usuarios.RegisterUser(newUser, Password.Unsecure());

                // ¡PASO CRUCIAL AÑADIDO! Guardamos los cambios en la base de datos.
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("¡Usuario registrado con éxito!", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(); // Cierra la ventana
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
    }
}
