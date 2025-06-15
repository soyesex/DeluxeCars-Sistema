using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CatalogoViewModel : ViewModelBase
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Estado Interno ---
        private List<Producto> _todosLosProductos;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FiltrarProductos();
            }
        }

        private ObservableCollection<Producto> _productos;
        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            private set => SetProperty(ref _productos, value);
        }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                SetProperty(ref _productoSeleccionado, value);
                (EditarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (EliminarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<Categoria> _categoriasDisponibles;
        public ObservableCollection<Categoria> CategoriasDisponibles
        {
            get => _categoriasDisponibles;
            private set => SetProperty(ref _categoriasDisponibles, value);
        }

        private Categoria _categoriaFiltroSeleccionada;
        public Categoria CategoriaFiltroSeleccionada
        {
            get => _categoriaFiltroSeleccionada;
            set
            {
                SetProperty(ref _categoriaFiltroSeleccionada, value);
                FiltrarProductos();
            }
        }

        // --- Comandos ---
        public ICommand NuevoProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }

        // --- Constructor ---
        public CatalogoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Productos = new ObservableCollection<Producto>();
            CategoriasDisponibles = new ObservableCollection<Categoria>();
            _todosLosProductos = new List<Producto>();

            NuevoProductoCommand = new ViewModelCommand(async p => await ExecuteNuevoProductoCommand());
            EditarProductoCommand = new ViewModelCommand(async p => await ExecuteEditarProductoCommand(), p => CanExecuteEditDelete());
            EliminarProductoCommand = new ViewModelCommand(async p => await ExecuteEliminarProductoCommand(), p => CanExecuteEditDelete());

            LoadInitialDataAsync();
        }

        // --- Métodos de Lógica ---
        private async Task LoadInitialDataAsync()
        {
            await LoadCategoriasAsync();
            await LoadProductosAsync();
        }

        private async Task LoadCategoriasAsync()
        {
            try
            {
                var categoriasDesdeRepo = await _unitOfWork.Categorias.GetAllAsync();
                var categoriasOrdenadas = categoriasDesdeRepo.OrderBy(c => c.Nombre).ToList();

                CategoriasDisponibles.Clear();
                CategoriasDisponibles.Add(new Categoria { Id = 0, Nombre = "Todas las Categorías" });
                categoriasOrdenadas.ForEach(cat => CategoriasDisponibles.Add(cat));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR CATEGORÍAS: {ex.Message}");
                MessageBox.Show("Ocurrió un error al cargar las categorías.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadProductosAsync()
        {
            try
            {
                // --- CÓDIGO RESTAURADO A SU VERSIÓN FINAL ---
                // Usamos el método que incluye las categorías para mostrar el nombre en el DataGrid.
                var productosDesdeRepo = await _unitOfWork.Productos.GetAllWithCategoriaAsync();

                _todosLosProductos = productosDesdeRepo.ToList();
                FiltrarProductos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR PRODUCTOS: {ex.Message}");
                MessageBox.Show($"Ocurrió un error crítico al cargar los productos: {ex.Message}\n\nAsegúrate de que el modelo Producto.cs coincide con la base de datos.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarProductos()
        {
            IEnumerable<Producto> itemsFiltrados = _todosLosProductos;

            if (CategoriaFiltroSeleccionada != null && CategoriaFiltroSeleccionada.Id != 0)
            {
                itemsFiltrados = itemsFiltrados.Where(p => p.IdCategoria == CategoriaFiltroSeleccionada.Id);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(p => p.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Productos = new ObservableCollection<Producto>(itemsFiltrados.OrderBy(p => p.Id));
        }

        private bool CanExecuteEditDelete() => ProductoSeleccionado != null;

        private async Task ExecuteNuevoProductoCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Producto, 0);
            await LoadProductosAsync();
        }

        private async Task ExecuteEditarProductoCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Producto, ProductoSeleccionado.Id);
            await LoadProductosAsync();
        }

        private async Task ExecuteEliminarProductoCommand()
        {
            var productoAEliminar = ProductoSeleccionado;
            var result = MessageBox.Show($"¿Estás seguro de que deseas eliminar el producto '{productoAEliminar.Nombre}'?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            try
            {
                await _unitOfWork.Productos.RemoveAsync(productoAEliminar);
                await _unitOfWork.CompleteAsync();

                _todosLosProductos.Remove(productoAEliminar);
                FiltrarProductos();

                MessageBox.Show("Producto eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al eliminar el producto. Es posible que esté asociado a una factura o pedido.\n\nError: {ex.Message}", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
