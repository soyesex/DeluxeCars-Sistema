using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ProveedorFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        // --- Dependencias y Estado ---
        private readonly IUnitOfWork _unitOfWork;
        private Proveedor _proveedorActual;
        private bool _esModoEdicion;
        private List<Municipio> _todosLosMunicipios;

        // --- Propiedades para Binding ---
        private string _tituloVentana;
        public string TituloVentana { get => _tituloVentana; set => SetProperty(ref _tituloVentana, value); }

        private string _razonSocial;
        public string RazonSocial { get => _razonSocial; set => SetProperty(ref _razonSocial, value); }

        private string _nit;
        public string NIT { get => _nit; set => SetProperty(ref _nit, value); }

        private string _telefono;
        public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private bool _estado;
        public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

        // Propiedades para ComboBoxes en cascada
        public ObservableCollection<Departamento> Departamentos { get; private set; }
        public ObservableCollection<Municipio> Municipios { get; private set; }

        private Departamento _departamentoSeleccionado;
        public Departamento DepartamentoSeleccionado
        {
            get => _departamentoSeleccionado;
            set
            {
                SetProperty(ref _departamentoSeleccionado, value);
                // Cuando un departamento es seleccionado, filtramos los municipios.
                CargarMunicipiosPorDepartamento();
            }
        }

        private Municipio _municipioSeleccionado;
        public Municipio MunicipioSeleccionado { get => _municipioSeleccionado; set => SetProperty(ref _municipioSeleccionado, value); }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }

        public ProveedorFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            Departamentos = new ObservableCollection<Departamento>();
            Municipios = new ObservableCollection<Municipio>();

            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(p => CloseAction?.Invoke());
        }

        public async Task LoadAsync(int proveedorId)
        {
            await LoadUbicacionesAsync();

            if (proveedorId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _proveedorActual = new Proveedor();
                TituloVentana = "Nuevo Proveedor";
                Estado = true;
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _proveedorActual = await _unitOfWork.Proveedores.GetByIdWithLocationAsync(proveedorId);
                if (_proveedorActual != null)
                {
                    TituloVentana = "Editar Proveedor";
                    RazonSocial = _proveedorActual.RazonSocial;
                    NIT = _proveedorActual.NIT;
                    Telefono = _proveedorActual.Telefono;
                    Email = _proveedorActual.Email;
                    Estado = _proveedorActual.Estado;

                    // Seleccionar valores en los ComboBoxes
                    if (_proveedorActual.Municipio?.Departamento != null)
                    {
                        DepartamentoSeleccionado = Departamentos.FirstOrDefault(d => d.Id == _proveedorActual.Municipio.Departamento.Id);
                    }
                    // La selección del depto cargará los municipios, ahora seleccionamos el correcto.
                    MunicipioSeleccionado = Municipios.FirstOrDefault(m => m.Id == _proveedorActual.IdMunicipio);
                }
            }
        }

        private async Task LoadUbicacionesAsync()
        {
            var depts = await _unitOfWork.Departamentos.GetAllAsync();
            var munis = await _unitOfWork.Municipios.GetAllAsync();
            _todosLosMunicipios = munis.ToList();
            Departamentos = new ObservableCollection<Departamento>(depts.OrderBy(d => d.Nombre));
        }

        private void CargarMunicipiosPorDepartamento()
        {
            var municipioSeleccionadoAnteriormente = MunicipioSeleccionado;
            Municipios.Clear();
            if (DepartamentoSeleccionado != null)
            {
                var municipiosFiltrados = _todosLosMunicipios.Where(m => m.IdDepartamento == DepartamentoSeleccionado.Id);
                foreach (var m in municipiosFiltrados.OrderBy(m => m.Nombre))
                {
                    Municipios.Add(m);
                }
            }
            // Intentar re-seleccionar el municipio si aún existe en la nueva lista
            MunicipioSeleccionado = Municipios.FirstOrDefault(m => m.Id == municipioSeleccionadoAnteriormente?.Id);
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(RazonSocial) || string.IsNullOrWhiteSpace(NIT) || MunicipioSeleccionado == null)
            {
                MessageBox.Show("Razón Social, NIT y Municipio son obligatorios.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _proveedorActual.RazonSocial = RazonSocial;
            _proveedorActual.NIT = NIT;
            _proveedorActual.Telefono = Telefono;
            _proveedorActual.Email = Email;
            _proveedorActual.Estado = Estado;
            _proveedorActual.IdMunicipio = MunicipioSeleccionado.Id;

            try
            {
                if (_esModoEdicion)
                {
                    await _unitOfWork.Proveedores.UpdateAsync(_proveedorActual);
                }
                else
                {
                    await _unitOfWork.Proveedores.AddAsync(_proveedorActual);
                }
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Proveedor guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el proveedor. Es posible que el NIT o Email ya existan.\n\nError: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
