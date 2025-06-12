using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CatalogoViewModel : ViewModelBase
    {
        // --- Dependencias Inyectadas ---
        private readonly INavigationService _navigationService;
        private readonly IProductoRepository _productoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        // --- Estado Interno ---
        private List<Producto> _todosLosProductos;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FiltrarProductos();
            }
        }

        private ObservableCollection<Producto> _productos;
        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            private set
            {
                _productos = value;
                OnPropertyChanged(nameof(Productos));
            }
        }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                _productoSeleccionado = value;
                OnPropertyChanged(nameof(ProductoSeleccionado));
            }
        }

        private ObservableCollection<Categoria> _categoriasDisponibles;
        public ObservableCollection<Categoria> CategoriasDisponibles
        {
            get => _categoriasDisponibles;
            set
            {
                _categoriasDisponibles = value;
                OnPropertyChanged(nameof(CategoriasDisponibles));
            }
        }

        private Categoria _categoriaFiltroSeleccionada;
        public Categoria CategoriaFiltroSeleccionada
        {
            get => _categoriaFiltroSeleccionada;
            set
            {
                _categoriaFiltroSeleccionada = value;
                OnPropertyChanged(nameof(CategoriaFiltroSeleccionada));
                // Cuando el usuario cambia la selección, volvemos a filtrar los productos.
                FiltrarProductos();
            }
        }

        // --- Comandos ---
        public ICommand NuevoProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand ShowCategoriaViewCommand { get; }

        // --- Constructor ---
        public CatalogoViewModel(ICategoriaRepository categoriaRepository, IProductoRepository productoRepository, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;

            Productos = new ObservableCollection<Producto>();
            CategoriasDisponibles = new ObservableCollection<Categoria>();
            _todosLosProductos = new List<Producto>();

            NuevoProductoCommand = new ViewModelCommand(ExecuteNuevoProductoCommand);
            EditarProductoCommand = new ViewModelCommand(ExecuteEditarProductoCommand, CanExecuteEditDelete);
            EliminarProductoCommand = new ViewModelCommand(ExecuteEliminarProductoCommand, CanExecuteEditDelete);
            ShowCategoriaViewCommand = new ViewModelCommand(ExecuteShowCategoriaView);

            // Cargar los datos al iniciar.
            LoadInitialDataAsync();
        }

        // --- Métodos de Lógica ---
        private async void LoadInitialDataAsync()
        {
            await LoadCategoriasAsync();
            await LoadProductosAsync();
        }
        private async Task LoadCategoriasAsync()
        {
            try
            {
                var categoriasDesdeRepo = await _categoriaRepository.GetAllAsync();
                CategoriasDisponibles.Clear();
                // Añadimos una opción "Todas" al principio para poder limpiar el filtro.
                CategoriasDisponibles.Add(new Categoria { Id = 0, Nombre = "Todas las Categorías" });
                foreach (var cat in categoriasDesdeRepo)
                {
                    CategoriasDisponibles.Add(cat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR CATEGORÍAS: {ex.Message}");
            }
        }
        private async Task LoadProductosAsync()
        {
            try
            {
                var productosDesdeRepo = await _productoRepository.GetAllAsync();
                _todosLosProductos = productosDesdeRepo.ToList();
                FiltrarProductos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR PRODUCTOS: {ex.Message}");
            }
        }

        private void FiltrarProductos()
        {
            // Empezamos con la lista completa de productos.
            IEnumerable<Producto> itemsFiltrados = _todosLosProductos;

            // 1. Aplicamos el filtro por categoría seleccionada.
            // Si hay una categoría seleccionada y su ID no es 0 (nuestra opción "Todas"), filtramos.
            if (CategoriaFiltroSeleccionada != null && CategoriaFiltroSeleccionada.Id != 0)
            {
                itemsFiltrados = itemsFiltrados.Where(p => p.IdCategoria == CategoriaFiltroSeleccionada.Id);
            }

            // 2. Sobre la lista ya filtrada, aplicamos el filtro por texto de búsqueda.
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(p => p.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Actualizamos la colección final que se muestra en la UI.
            Productos = new ObservableCollection<Producto>(itemsFiltrados);
        }

        private bool CanExecuteEditDelete(object obj)
        {
            return ProductoSeleccionado != null;
        }

        private void ExecuteNuevoProductoCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Producto);
            LoadProductosAsync();
        }

        private void ExecuteEditarProductoCommand(object obj)
        {
            // Lógica futura para editar el ProductoSeleccionado.
        }

        private async void ExecuteEliminarProductoCommand(object obj)
        {
            if (ProductoSeleccionado != null)
            {
                // Aquí podrías añadir una confirmación para el usuario.
                await _productoRepository.RemoveAsync(ProductoSeleccionado.Id);
                LoadProductosAsync();
            }
        }

        private void ExecuteShowCategoriaView(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Categoria);
        }
    }
}
