using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CategoriaFormViewModel : ViewModelBase
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly INavigationService _navigationService;

        private List<Categoria> _todasLasCategorias;

        private string _searchText;
        private ObservableCollection<Categoria> _categorias;
        private Categoria _categoriaSeleccionada;

        // PROPIEDADES PUBLICAS ENLAZADAS A LA VISTA
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    // Cada vez que cambia el texto, filtramos las categorías
                    FiltrarCategorias();
                }
            }
        }
        public ObservableCollection<Categoria> Categorias
        {
            get => _categorias;
            private set
            {
                _categorias = value;
                OnPropertyChanged(nameof(Categorias));
            }
        }

        // Propiedad para saber qué categoría ha seleccionado el usuario en el DataGrid.
        public Categoria CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                if (_categoriaSeleccionada != value)
                {
                    _categoriaSeleccionada = value;
                    OnPropertyChanged(nameof(CategoriaSeleccionada));
                }
            }
        }

        // Comandos
        public ICommand NuevaCategoriaCommand { get; }
        public ICommand EditarCategoriaCommand { get; }
        public ICommand EliminarCategoriaCommand { get; }

        public CategoriaFormViewModel(ICategoriaRepository categoriaRepository , INavigationService navigationService)
        {
            _categoriaRepository = categoriaRepository;
            _navigationService = navigationService;

            Categorias = new ObservableCollection<Categoria>();
            _todasLasCategorias = new List<Categoria>();

            // Inicializamos los comandos
            NuevaCategoriaCommand = new ViewModelCommand(ExecuteNuevaCategoriaCommand);
            // El comando de editar/eliminar solo se puede ejecutar si hay una categoría seleccionada.
            EditarCategoriaCommand = new ViewModelCommand(ExecuteEditarCategoriaCommand, CanExecuteEditDelete);
            EliminarCategoriaCommand = new ViewModelCommand(ExecuteEliminarCategoriaCommand, CanExecuteEditDelete);

            // Cargamos los datos iniciales al crear el ViewModel.
            LoadCategoriasAsync();
        }

        // Metodos privados
        private async void LoadCategoriasAsync()
        {
            try
            {
                var categoriasDesdeRepo = await _categoriaRepository.GetAllAsync();
                _todasLasCategorias = categoriasDesdeRepo.ToList();

                Categorias.Clear();
                foreach (var categoria in _todasLasCategorias)
                {
                    Categorias.Add(categoria);
                }
                Debug.WriteLine($"---> ¡Cargadas {Categorias.Count} categorías en la colección observable!");
            }
            catch (Exception ex)
            {
                // Manejo de errores, por ejemplo, mostrar un mensaje al usuario.
                Debug.WriteLine($"!!! ERROR AL CARGAR CATEGORÍAS: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void FiltrarCategorias()
        {
            // CAMBIO 3: La lógica de filtrado también modifica la colección, no la reemplaza.
            Categorias.Clear();

            IEnumerable<Categoria> itemsFiltrados;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = _todasLasCategorias;
            }
            else
            {
                itemsFiltrados = _todasLasCategorias.Where(c =>
                    c.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Descripcion != null && c.Descripcion.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }

            foreach (var categoria in itemsFiltrados)
            {
                Categorias.Add(categoria);
            }
        }
        // Determina si los botones de Editar/Eliminar deben estar habilitados.
        private bool CanExecuteEditDelete(object obj)
        {
            return CategoriaSeleccionada != null;
        }

        private void ExecuteNuevaCategoriaCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Categoria);

            LoadCategoriasAsync();
        }

        private void ExecuteEditarCategoriaCommand(object obj)
        {
            // Aquí irá la lógica para abrir el formulario de edición.
        }

        private async void ExecuteEliminarCategoriaCommand(object obj)
        {
            // Lógica para eliminar la categoría seleccionada.
            if (CategoriaSeleccionada != null)
            {
                await _categoriaRepository.RemoveAsync(CategoriaSeleccionada.Id);
                // Volvemos a cargar la lista para que se refleje el cambio.
                LoadCategoriasAsync();
            }
        }
    }
}
