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
        // --- Dependencias (Sin cambios) ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Estado Interno (Sin cambios) ---
        private Categoria _categoriaActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding a la UI ---

        public string TituloVentana { get; private set; } = "Gestión de Categorías"; // Título fijo

        // --- Propiedades para el Formulario de Entrada ---
        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set { SetProperty(ref _nombre, value); (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _descripcion;
        public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

        // --- NUEVAS PROPIEDADES PARA EL DATAGRID ---
        public ObservableCollection<Categoria> ListaCategorias { get; private set; }
        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set { SetProperty(ref _categoriaSeleccionada, value); (EditarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EditarCommand { get; } // NUEVO
        public ICommand NuevoCommand { get; }   // NUEVO

        // --- Acción para cerrar la ventana (Sin cambios) ---
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public CategoriaFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ListaCategorias = new ObservableCollection<Categoria>();

            // Inicialización de comandos
            GuardarCommand = new ViewModelCommand(async (p) => await ExecuteGuardarCommand(), (p) => CanExecuteGuardarCommand());
            CancelarCommand = new ViewModelCommand((p) => CloseAction?.Invoke());
            EditarCommand = new ViewModelCommand(ExecuteEditarCommand, (p) => CategoriaSeleccionada != null);
            NuevoCommand = new ViewModelCommand(ExecuteNuevoCommand);

            // Inicializamos en modo "Nuevo"
            ExecuteNuevoCommand(null);
        }

        // --- Lógica de Carga de Datos ---
        // Este método se llama cuando el formulario anfitrión lo inicializa.
        public async Task LoadAsync(int ignoredId)
        {
            await CargarCategoriasAsync();
        }

        private async Task CargarCategoriasAsync()
        {
            var categorias = await _unitOfWork.Categorias.GetAllAsync();
            ListaCategorias.Clear();
            foreach (var cat in categorias.OrderBy(c => c.Nombre))
            {
                ListaCategorias.Add(cat);
            }
        }

        // --- Lógica de los Comandos ---

        private void ExecuteNuevoCommand(object obj)
        {
            _esModoEdicion = false;
            _categoriaActual = new Categoria();

            // Limpiamos los campos del formulario
            Nombre = string.Empty;
            Descripcion = string.Empty;
            CategoriaSeleccionada = null; // Deseleccionamos la grid
        }

        private void ExecuteEditarCommand(object obj)
        {
            if (CategoriaSeleccionada == null) return;

            _esModoEdicion = true;
            _categoriaActual = CategoriaSeleccionada; // Apuntamos a la entidad seleccionada

            // Cargamos sus datos en el formulario
            Nombre = _categoriaActual.Nombre;
            Descripcion = _categoriaActual.Descripcion;
        }

        private bool CanExecuteGuardarCommand()
        {
            return !string.IsNullOrWhiteSpace(Nombre);
        }

        private async Task ExecuteGuardarCommand()
        {
            if (!CanExecuteGuardarCommand()) return;

            // Actualizamos la entidad en memoria con los datos del formulario
            _categoriaActual.Nombre = Nombre;
            _categoriaActual.Descripcion = Descripcion;

            try
            {
                if (_esModoEdicion)
                {
                    await _unitOfWork.Categorias.UpdateAsync(_categoriaActual);
                }
                else
                {
                    await _unitOfWork.Categorias.AddAsync(_categoriaActual);
                }

                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Categoría guardada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refrescamos la lista en la UI y reiniciamos el formulario
                await CargarCategoriasAsync();
                ExecuteNuevoCommand(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
