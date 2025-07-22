using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CategoriasViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias (¡CAMBIO!) ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Estado Interno ---
        // Lista maestra que actúa como caché para el filtrado en memoria.
        private List<Categoria> _todasLasCategorias;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FiltrarCategorias();
            }
        }

        private ObservableCollection<Categoria> _categorias;
        public ObservableCollection<Categoria> Categorias
        {
            get => _categorias;
            private set
            {
                _categorias = value;
                OnPropertyChanged(nameof(Categorias));
            }
        }

        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                _categoriaSeleccionada = value;
                OnPropertyChanged(nameof(CategoriaSeleccionada));
                // Notificar a los comandos que su estado de "ejecutable" puede haber cambiado.
                (EditarCategoriaCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (EliminarCategoriaCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Comandos ---
        public ICommand NuevaCategoriaCommand { get; }
        public ICommand EditarCategoriaCommand { get; }
        public ICommand EliminarCategoriaCommand { get; }

        // --- Constructor (¡CAMBIO!) ---
        public CategoriasViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Categorias = new ObservableCollection<Categoria>();
            _todasLasCategorias = new List<Categoria>();

            NuevaCategoriaCommand = new ViewModelCommand(ExecuteNuevaCategoriaCommand);
            EditarCategoriaCommand = new ViewModelCommand(ExecuteEditarCategoriaCommand, CanExecuteEditDelete);
            EliminarCategoriaCommand = new ViewModelCommand(ExecuteEliminarCategoriaCommand, CanExecuteEditDelete);
        }

        // --- Métodos de Lógica ---
        public async Task LoadAsync()
        {
            try
            {
                // Se usa el UnitOfWork para acceder al repositorio de Categorías.
                var categoriasDesdeRepo = await _unitOfWork.Categorias.GetAllAsync();
                _todasLasCategorias = categoriasDesdeRepo.ToList();
                FiltrarCategorias();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR CATEGORÍAS: {ex.Message}");
                MessageBox.Show("No se pudieron cargar las categorías desde la base de datos.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarCategorias()
        {
            IEnumerable<Categoria> itemsFiltrados = _todasLasCategorias;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLower();
                itemsFiltrados = itemsFiltrados.Where(c =>
                    c.Nombre.ToLower().Contains(lowerSearchText) ||
                    (c.Descripcion != null && c.Descripcion.ToLower().Contains(lowerSearchText))
                );
            }

            // Manera más eficiente de actualizar la colección en la UI.
            Categorias = new ObservableCollection<Categoria>(itemsFiltrados.OrderBy(c => c.Nombre));
        }

        private bool CanExecuteEditDelete(object obj)
        {
            return CategoriaSeleccionada != null;
        }

        private async void ExecuteNuevaCategoriaCommand(object obj)
        {
            // La lógica para gestionar el resultado del formulario se puede añadir más adelante.
            await _navigationService.OpenFormWindow(Utils.FormType.Categoria, 0);
            await LoadAsync(); // Recargamos por ahora.
        }

        private async void ExecuteEditarCategoriaCommand(object obj)
        {
            // 1. Llama al servicio de navegación para abrir el formulario en modo "Edición",
            //    pasando el ID de la categoría seleccionada.
            await _navigationService.OpenFormWindow(Utils.FormType.Categoria, CategoriaSeleccionada.Id);

            // 2. Al igual que antes, esta línea espera a que el formulario se cierre para refrescar la lista.
            await LoadAsync();
        }

        // --- Método de Eliminar (¡CAMBIO CLAVE!) ---
        private async void ExecuteEliminarCategoriaCommand(object obj)
        {
            var categoriaAEliminar = CategoriaSeleccionada;

            var result = MessageBox.Show($"¿Estás seguro de que deseas eliminar la categoría '{categoriaAEliminar.Nombre}'? Esta acción no se puede deshacer.", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            try
            {
                // 1. Se le dice a EF que se quiere eliminar la entidad seleccionada.
                // Se pasa la entidad completa, no solo el ID.
                await _unitOfWork.Categorias.RemoveAsync(categoriaAEliminar);

                // 2. Se confirman los cambios en la base de datos. ¡Este paso es crucial!
                await _unitOfWork.CompleteAsync();

                // 3. (Mejora de UX) Se actualiza la UI eliminando el item de las listas locales.
                _todasLasCategorias.Remove(categoriaAEliminar);
                FiltrarCategorias(); // Refresca la lista visible.

                MessageBox.Show("Categoría eliminada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Este error suele ocurrir si la categoría está siendo usada por algún producto.
                Debug.WriteLine($"!!! ERROR AL ELIMINAR CATEGORÍA: {ex.Message}");
                MessageBox.Show($"No se pudo eliminar la categoría. Es probable que esté en uso por uno o más productos.\n\nError técnico: {ex.Message}", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
