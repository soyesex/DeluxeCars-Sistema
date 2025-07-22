using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string TituloVentana { get => _tituloVentana; set => SetProperty(ref _tituloVentana, value); }

        private string _nombre;
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

        private string _tipoCliente;
        public string TipoCliente { get => _tipoCliente; set => SetProperty(ref _tipoCliente, value); }

        private string _tipoIdentificacion;
        public string TipoIdentificacion { get => _tipoIdentificacion; set => SetProperty(ref _tipoIdentificacion, value); }

        private string _identificacion;
        public string Identificacion { get => _identificacion; set => SetProperty(ref _identificacion, value); }

        private string _direccion;
        public string Direccion { get => _direccion; set => SetProperty(ref _direccion, value); }

        private int? _idCiudad;
        public int? IdCiudad { get => _idCiudad; set => SetProperty(ref _idCiudad, value); }

        private string _telefono;
        public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private bool _estado;
        public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

        // --- Colecciones para ComboBoxes ---
        public ObservableCollection<string> TiposClienteDisponibles { get; }
        public ObservableCollection<string> TiposIdentificacionDisponibles { get; }
        public ObservableCollection<Municipio> CiudadesDisponibles { get; }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        // --- Acción para cerrar la ventana ---
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public ClienteFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            TiposClienteDisponibles = new ObservableCollection<string> { "Taller", "Persona Natural" };
            TiposIdentificacionDisponibles = new ObservableCollection<string> { "CC", "NIT", "CE" };
            CiudadesDisponibles = new ObservableCollection<Municipio>();

            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(ExecuteCancelarCommand);
        }
        public async Task LoadAsync(int clienteId)
        {
            var ciudadesRepo = await _unitOfWork.Municipios.GetAllAsync();
            CiudadesDisponibles.Clear();
            foreach(var ciudad in ciudadesRepo.OrderBy(c => c.Nombre))
            {
                CiudadesDisponibles.Add(ciudad);
            }

            if (clienteId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _clienteActual = new Cliente { FechaCreacion = DateTime.Now };
                TituloVentana = "Nuevo Cliente";
                Estado = true; 
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _clienteActual = await _unitOfWork.Clientes.GetByIdAsync(clienteId);
                if (_clienteActual != null)
                {
                    TituloVentana = "Editar Cliente";
                    // Cargar datos del modelo a las propiedades del ViewModel
                    Nombre = _clienteActual.Nombre;
                    TipoCliente = _clienteActual.TipoCliente;
                    TipoIdentificacion = _clienteActual.TipoIdentificacion;
                    Identificacion = _clienteActual.Identificacion;
                    Direccion = _clienteActual.Direccion;
                    IdCiudad = _clienteActual.IdCiudad;
                    Telefono = _clienteActual.Telefono;
                    Email = _clienteActual.Email;
                    Estado = _clienteActual.Estado;
                }
                else
                {
                    MessageBox.Show("No se encontró el cliente solicitado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Nombre y Correo Electrónico son obligatorios.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                 MessageBox.Show("Por favor, ingrese una dirección de correo electrónico válida.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                 return;
            }

            // Actualizar el modelo con los datos del ViewModel
            _clienteActual.Nombre = Nombre;
            _clienteActual.TipoCliente = TipoCliente;
            _clienteActual.TipoIdentificacion = TipoIdentificacion;
            _clienteActual.Identificacion = Identificacion;
            _clienteActual.Direccion = Direccion;
            _clienteActual.IdCiudad = IdCiudad;
            _clienteActual.Telefono = Telefono;
            _clienteActual.Email = Email;
            _clienteActual.Estado = Estado;

            try
            {
                if (_esModoEdicion)
                {
                    // EF Core rastrea la entidad, no necesitas llamar a Update
                }
                else
                {
                    await _unitOfWork.Clientes.AddAsync(_clienteActual);
                }

                await _unitOfWork.CompleteAsync();
                MessageBox.Show("Cliente guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(); // Cierra y avisa que fue exitoso
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExecuteCancelarCommand(object obj)
        {
            CloseAction?.Invoke();
        }
    }
}
