using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Azure.Core.HttpHeader;
using DeluxeCars.DataAccess.Repositories.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class MunicipioFormViewModel : ViewModelBase, IFormViewModel
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Estado Interno ---
        private Municipio _municipioActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding ---
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

        private bool _estado;
        public bool Estado
        {
            get => _estado;
            set => SetProperty(ref _estado, value);
        }

        // Propiedades para el ComboBox de Departamentos
        private ObservableCollection<Departamento> _departamentos;
        public ObservableCollection<Departamento> Departamentos
        {
            get => _departamentos;
            private set => SetProperty(ref _departamentos, value);
        }

        private Departamento _departamentoSeleccionado;
        public Departamento DepartamentoSeleccionado
        {
            get => _departamentoSeleccionado;
            set => SetProperty(ref _departamentoSeleccionado, value);
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public MunicipioFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(ExecuteCancelarCommand);
        }

        /// <summary>
        /// Carga los datos necesarios para el formulario.
        /// </summary>
        /// <param name="municipioId">El ID del municipio a editar, o 0 para crear uno nuevo.</param>
        public async Task LoadAsync(int municipioId)
        {
            // Cargar siempre la lista de departamentos para el ComboBox
            await LoadDepartamentosAsync();

            if (municipioId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _municipioActual = new Municipio();
                TituloVentana = "Nuevo Municipio";
                Estado = true; // Por defecto, activo.
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _municipioActual = await _unitOfWork.Municipios.GetByIdAsync(municipioId);
                if (_municipioActual != null)
                {
                    TituloVentana = "Editar Municipio";
                    Nombre = _municipioActual.Nombre;
                    Estado = _municipioActual.Estado;
                    // Seleccionar el departamento actual en el ComboBox
                    DepartamentoSeleccionado = Departamentos.FirstOrDefault(d => d.Id == _municipioActual.IdDepartamento);
                }
                else
                {
                    MessageBox.Show("No se encontró el municipio solicitado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseAction?.Invoke();
                }
            }
        }

        private async Task LoadDepartamentosAsync()
        {
            try
            {
                var depts = await _unitOfWork.Departamentos.GetAllAsync();
                Departamentos = new ObservableCollection<Departamento>(depts.OrderBy(d => d.Nombre));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los departamentos: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            // --- Validación ---
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("El nombre del municipio es obligatorio.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DepartamentoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un departamento.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Actualización del Modelo ---
            _municipioActual.Nombre = Nombre;
            _municipioActual.Estado = Estado;
            _municipioActual.IdDepartamento = DepartamentoSeleccionado.Id; // Asignar el ID del padre

            try
            {
                // --- Persistencia ---
                if (_esModoEdicion)
                {
                    await _unitOfWork.Municipios.UpdateAsync(_municipioActual);
                }
                else
                {
                    await _unitOfWork.Municipios.AddAsync(_municipioActual);
                }

                await _unitOfWork.CompleteAsync(); // Confirmar transacción

                MessageBox.Show("Municipio guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el municipio: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelarCommand(object obj)
        {
            CloseAction?.Invoke();
        }
    } 
}
