using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ClienteFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades de Estado ---
        private Cliente _clienteActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding a la UI ---

        private string _tituloVentana;
        public string TituloVentana
        {
            get => _tituloVentana;
            set => SetProperty(ref _tituloVentana, value);
        }

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string _telefono;
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private bool _estado;
        public bool Estado
        {
            get => _estado;
            set => SetProperty(ref _estado, value);
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        // --- Acción para cerrar la ventana ---
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public ClienteFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(ExecuteCancelarCommand);
        }
        /// <summary>
        /// Inicializa el ViewModel para un cliente específico (editar) o para uno nuevo.
        /// </summary>
        /// <param name="clienteId">El ID del cliente a editar, o 0 para crear uno nuevo.</param>
        public async Task LoadAsync(int clienteId)
        {
            if (clienteId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _clienteActual = new Cliente();
                TituloVentana = "Nuevo Cliente";
                Estado = true; // Los clientes nuevos están activos por defecto.
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _clienteActual = await _unitOfWork.Clientes.GetByIdAsync(clienteId);
                if (_clienteActual != null)
                {
                    TituloVentana = "Editar Cliente";
                    Nombre = _clienteActual.Nombre;
                    Telefono = _clienteActual.Telefono;
                    Email = _clienteActual.Email;
                    Estado = _clienteActual.Estado;
                }
                else
                {
                    MessageBox.Show("No se encontró el cliente solicitado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseAction?.Invoke();
                }
            }
            // Notificamos a la UI que los títulos y datos iniciales han cambiado
            OnPropertyChanged(nameof(TituloVentana));
            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Telefono));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Estado));
        }

        // --- Lógica de los Comandos ---

        private async void ExecuteGuardarCommand(object obj)
        {
            // --- Validación de Datos ---
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Por favor, ingrese una dirección de correo electrónico válida.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Actualización del Modelo ---
            _clienteActual.Nombre = Nombre;
            _clienteActual.Telefono = Telefono;
            _clienteActual.Email = Email;
            _clienteActual.Estado = Estado;

            try
            {
                // --- Persistencia en la Base de Datos ---
                if (_esModoEdicion)
                {
                    await _unitOfWork.Clientes.UpdateAsync(_clienteActual);
                }
                else
                {
                    await _unitOfWork.Clientes.AddAsync(_clienteActual);
                }

                // --- Confirmación de la Transacción ---
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Cliente guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(); // Cierra el formulario
            }
            catch (Exception ex)
            {
                // Manejo de errores, por ejemplo, si el email ya existe (debido a la restricción UNIQUE)
                MessageBox.Show($"Ocurrió un error al guardar el cliente. Es posible que el correo electrónico ya esté en uso.\n\nError: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExecuteCancelarCommand(object obj)
        {
            CloseAction?.Invoke();
        }
    }
}
