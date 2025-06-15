using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Utils;
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
    public class UsuarioFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private Usuario _usuarioActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding ---
        public string TituloVentana { get; private set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
        public SecureString Password { get; set; }
        public SecureString ConfirmPassword { get; set; }
        public ObservableCollection<Rol> RolesDisponibles { get; private set; }
        public Rol RolSeleccionado { get; set; }

        // Propiedad para controlar la visibilidad de los campos de contraseña
        public bool IsCreateMode => !_esModoEdicion;

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }

        public UsuarioFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            var roles = await _unitOfWork.Roles.GetAllAsync();
            RolesDisponibles = new ObservableCollection<Rol>(roles.OrderBy(r => r.Nombre));
        }

        private bool CanExecuteGuardarCommand(object obj)
        {
            if (_esModoEdicion)
                return !string.IsNullOrWhiteSpace(Nombre) && !string.IsNullOrWhiteSpace(Email) && RolSeleccionado != null;
            else
                return !string.IsNullOrWhiteSpace(Nombre) && !string.IsNullOrWhiteSpace(Email) && RolSeleccionado != null && Password != null && Password.Length > 0;
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            if (!_esModoEdicion && (Password.Unsecure() != ConfirmPassword.Unsecure()))
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _usuarioActual.Nombre = Nombre;
            _usuarioActual.Telefono = Telefono;
            _usuarioActual.Email = Email;
            _usuarioActual.Activo = Activo;
            _usuarioActual.IdRol = RolSeleccionado.Id;

            try
            {
                if (_esModoEdicion)
                {
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
    }
}