using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CategoriaFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades de Estado ---
        private Categoria _categoriaActual;
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

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        // --- Acción para cerrar la ventana ---
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public CategoriaFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(ExecuteCancelarCommand);
        }

        /// <summary>
        /// Método de inicialización para cargar una categoría existente o preparar una nueva.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a editar, o 0 para crear una nueva.</param>
        public async Task LoadAsync(int categoriaId)
        {
            if (categoriaId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _categoriaActual = new Categoria();
                TituloVentana = "Nueva Categoría";
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _categoriaActual = await _unitOfWork.Categorias.GetByIdAsync(categoriaId);
                if (_categoriaActual != null)
                {
                    TituloVentana = "Editar Categoría";
                    Nombre = _categoriaActual.Nombre;
                    Descripcion = _categoriaActual.Descripcion;
                }
                else
                {
                    MessageBox.Show("No se encontró la categoría solicitada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("El nombre de la categoría no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Actualización del modelo ---
            _categoriaActual.Nombre = Nombre;
            _categoriaActual.Descripcion = Descripcion;

            try
            {
                // --- Lógica de persistencia ---
                if (_esModoEdicion)
                {
                    await _unitOfWork.Categorias.UpdateAsync(_categoriaActual);
                }
                else
                {
                    await _unitOfWork.Categorias.AddAsync(_categoriaActual);
                }

                // --- Confirmación de la transacción ---
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Categoría guardada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(); // Cierra la ventana del formulario
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar la categoría: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelarCommand(object obj)
        {
            // Simplemente cierra la ventana sin guardar.
            CloseAction?.Invoke();
        }
    }
}
