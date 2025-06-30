using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    // PENDIENTE POR REVISAR
    public class DepartamentoFormViewModel : ViewModelBase, IFormViewModel
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades de Estado ---
        private Departamento _departamentoActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding a la UI ---

        private string _tituloVentana;
        public string TituloVentana
        {
            get => _tituloVentana;
            set => SetProperty(ref _tituloVentana, value); // SetProperty es un helper de ViewModelBase
        }

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        // --- Acción para cerrar la ventana ---
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public DepartamentoFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(ExecuteCancelarCommand);
        }
        /// <summary>
        /// Método de inicialización para cargar una categoría existente o preparar una nueva.
        /// </summary>
        /// <param name="departamentoId">El ID de la categoría a editar, o 0 para crear una nueva.</param>
        public async Task LoadAsync(int departamentoId)
        {
            if (departamentoId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _departamentoActual = new Departamento();
                TituloVentana = "Nuevo Departamento";
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _departamentoActual = await _unitOfWork.Departamentos.GetByIdAsync(departamentoId);
                if (_departamentoActual != null)
                {
                    TituloVentana = "Editar Departamento";
                    Nombre = _departamentoActual.Nombre;
                }
                else
                {
                    MessageBox.Show("No se encontró la departamento solicitado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseAction?.Invoke();
                }
            }
        }
        // --- Lógica de los Comandos ---

        private async void ExecuteGuardarCommand(object obj)
        {
            // --- Validación ---
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("El nombre del departamento no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Actualización del modelo ---
            _departamentoActual.Nombre = Nombre;

            try
            {
                // --- Lógica de persistencia ---
                if (_esModoEdicion)
                {
                    await _unitOfWork.Departamentos.UpdateAsync(_departamentoActual);
                }
                else
                {
                    await _unitOfWork.Departamentos.AddAsync(_departamentoActual);
                }

                // --- Confirmación de la transacción ---
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Departamento guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(); // Cierra la ventana del formulario
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el departamento: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExecuteCancelarCommand(object obj)
        {
            // Simplemente cierra la ventana sin guardar.
            CloseAction?.Invoke();
        }
    }
}
