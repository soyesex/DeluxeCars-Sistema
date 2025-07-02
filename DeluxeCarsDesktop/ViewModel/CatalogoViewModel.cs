using DeluxeCars.DataAccess.Repositories.Implementations.Search;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View;
using DeluxeCarsEntities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CatalogoViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly IStockAlertService _stockAlertService;
        private bool _isSearching = false;

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
        private string _cambiarEstadoButtonText = "Activar/Desactivar";
        public string CambiarEstadoButtonText
        {
            get => _cambiarEstadoButtonText;
            set => SetProperty(ref _cambiarEstadoButtonText, value);
        }
        private Categoria _categoriaFiltroSeleccionada;
        public Categoria CategoriaFiltroSeleccionada
        {
            get => _categoriaFiltroSeleccionada;
            set { SetProperty(ref _categoriaFiltroSeleccionada, value); FiltrarProductos(); }
        }

        public ObservableCollection<string> StockStatusOptions { get; private set; }
        private string _stockStatusFiltroSeleccionado;
        public string StockStatusFiltroSeleccionado
        {
            get => _stockStatusFiltroSeleccionado;
            set { SetProperty(ref _stockStatusFiltroSeleccionado, value); FiltrarProductos(); }
        }

        // --- Propiedades para el DataGrid y la Selección ---
        public ObservableCollection<ProductoDisplayViewModel> Productos { get; private set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; private set; }

        private ProductoDisplayViewModel _productoSeleccionado;
        public ProductoDisplayViewModel ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                SetProperty(ref _productoSeleccionado, value);
                (EditarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (CambiarEstadoProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();

                // --- LÍNEAS A AÑADIR ---
                if (value != null)
                {
                    // Si el producto está activo, el botón dirá "Desactivar", si no, "Activar"
                    CambiarEstadoButtonText = value.Producto.Estado ? "Desactivar" : "Activar";
                }
                else
                {
                    // Texto por defecto si no hay nada seleccionado
                    CambiarEstadoButtonText = "Activar/Desactivar";
                }
            }
        }

        // --- Comandos ---
        public ICommand NuevoProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand CambiarEstadoProductoCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand AjusteManualCommand { get; }

        // --- Constructor ---
        public CatalogoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService, IStockAlertService stockAlertService)
    {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _stockAlertService = stockAlertService;

        Productos = new ObservableCollection<ProductoDisplayViewModel>();
            CategoriasDisponibles = new ObservableCollection<Categoria>();

            // Opciones para el filtro de Stock
            StockStatusOptions = new ObservableCollection<string> { "Todos", "En Stock", "Agotado", "Bajo Stock" };

            NuevoProductoCommand = new ViewModelCommand(async p => await ExecuteNuevoProductoCommand());
            EditarProductoCommand = new ViewModelCommand(async p => await ExecuteEditarProductoCommand(), p => CanExecuteEditDelete());
            CambiarEstadoProductoCommand = new ViewModelCommand(async p => await ExecuteCambiarEstadoProductoCommand(), p => CanExecuteEditDelete());
            LimpiarFiltrosCommand = new ViewModelCommand(ExecuteLimpiarFiltros);
            AjusteManualCommand = new ViewModelCommand(ExecuteAjusteManual);
        }

        // --- Lógica Principal ---
        // Este método se llama desde el NavigationService al cargar la vista
        public async Task LoadAsync()
        {
            await LoadCategoriasAsync();
            ExecuteLimpiarFiltros(null); // Limpia los filtros y carga los productos iniciales
        }

        private async Task LoadCategoriasAsync()
        {
            try
            {
                var categoriasDesdeRepo = await _unitOfWork.Categorias.GetAllAsync();
                CategoriasDisponibles.Clear();
                CategoriasDisponibles.Add(new Categoria { Id = 0, Nombre = "Todas las Categorías" });
                foreach (var cat in categoriasDesdeRepo.OrderBy(c => c.Nombre))
                {
                    CategoriasDisponibles.Add(cat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR CATEGORÍAS: {ex.Message}");
                MessageBox.Show("Ocurrió un error al cargar las categorías.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void FiltrarProductos()
        {
            if (_isSearching) return;
            _isSearching = true;

            try
            {
                var criteria = new ProductSearchCriteria
                {
                    UniversalSearchText = this.SearchText,
                    CategoryId = this.CategoriaFiltroSeleccionada?.Id,
                    StockStatus = this.StockStatusFiltroSeleccionado
                };

                // 1. PRIMER VIAJE A LA BD: Obtenemos la lista de productos.
                var productosDesdeRepo = await _unitOfWork.Productos.SearchAsync(criteria);

                // Extraemos solo los IDs de los productos encontrados.
                var productIds = productosDesdeRepo.Select(p => p.Id).ToList();

                // Creamos un diccionario vacío para los stocks por si no hay productos.
                var stockDictionary = new Dictionary<int, int>();

                if (productIds.Any())
                {
                    // 2. SEGUNDO VIAJE A LA BD: Obtenemos el stock para TODOS esos productos a la vez.
                    stockDictionary = await _unitOfWork.Productos.GetCurrentStocksAsync(productIds);
                }

                Productos.Clear();
                foreach (var prod in productosDesdeRepo)
                {
                    var displayVM = new ProductoDisplayViewModel(prod);

                    // Buscamos el stock en nuestro diccionario (operación en memoria, instantánea).
                    // Si un producto no tiene movimientos, su stock es 0.
                    displayVM.StockCalculado = stockDictionary.GetValueOrDefault(prod.Id, 0);

                    Productos.Add(displayVM);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL FILTRAR PRODUCTOS: {ex.Message}");
            }
            finally
            {
                _isSearching = false;
            }
        }
        private void ExecuteLimpiarFiltros(object obj)
        {
            // Reseteamos todos los filtros a su estado inicial
            SearchText = string.Empty;
            CategoriaFiltroSeleccionada = CategoriasDisponibles.FirstOrDefault();
            StockStatusFiltroSeleccionado = StockStatusOptions.FirstOrDefault();

            // Notificamos a la UI para que los campos se limpien
            OnPropertyChanged(string.Empty);

            // La llamada a FiltrarProductos() se dispara automáticamente por los setters,
            // cargando la lista completa de productos.
        }

        private void ExecuteAjusteManual(object obj)
        {
        // Este código es conceptual. Necesitarás una forma de crear y mostrar la ventana.
        // Podrías expandir tu NavigationService para que maneje diálogos como este.
        var vm = new AjusteInventarioViewModel(_unitOfWork, _stockAlertService);
        var view = new AjusteInventarioView { DataContext = vm };
            vm.CloseAction = view.Close;
            view.ShowDialog();

            // Refrescamos la lista principal después de cerrar el diálogo
            FiltrarProductos();
        }

        private bool CanExecuteEditDelete() => ProductoSeleccionado != null;

        private async Task ExecuteNuevoProductoCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Producto, 0);
            FiltrarProductos();
        }

        private async Task ExecuteEditarProductoCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Producto, ProductoSeleccionado.Producto.Id);
            FiltrarProductos();
        }

        private async Task ExecuteCambiarEstadoProductoCommand()
        {
            var productoSeleccionado = ProductoSeleccionado;
            if (productoSeleccionado == null) return;

            // Lógica para determinar la acción y el mensaje
            var producto = productoSeleccionado.Producto;
            string accion = producto.Estado ? "desactivar" : "activar";
            string accionPasado = producto.Estado ? "desactivado" : "activado";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} el producto '{producto.Nombre}'?",
                                         $"Confirmar {char.ToUpper(accion[0]) + accion.Substring(1)}",
                                         MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No) return;

            try
            {
                // La nueva lógica: invierte el estado actual
                producto.Estado = !producto.Estado;

                await _unitOfWork.CompleteAsync();

                // Refrescar la vista para que el cambio se vea reflejado
                FiltrarProductos();

                MessageBox.Show($"Producto {accionPasado} exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al {accion} el producto.\n\nError: {ex.Message}",
                                $"Error de {char.ToUpper(accion[0]) + accion.Substring(1)}",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                // Revertir el cambio en el objeto si falla el guardado
                producto.Estado = !producto.Estado;
            }
        }
    }
}
