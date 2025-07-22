using DeluxeCars.DataAccess.Repositories.Implementations.Search;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View;
using DeluxeCarsDesktop.ViewModel.ColumnView;
using DeluxeCarsEntities;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CatalogoViewModel : PaginatedViewModel<ProductoDisplayViewModel>,       IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly IStockAlertService _stockAlertService;
        private readonly INotificationService _notificationService; 
        private bool _isSearching = false;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                // CAMBIO: Usar SetPropertyAndCheck y llamar al método correcto de la clase base
                if (SetPropertyAndCheck(ref _searchText, value))
                {
                    ApplyFilterAndResetPage();
                }
            }
        }
        private string _cambiarEstadoButtonText = "Desactivar";
        public string CambiarEstadoButtonText
        {
            get => _cambiarEstadoButtonText;
            set => SetProperty(ref _cambiarEstadoButtonText, value);
        }
        private Categoria _categoriaFiltroSeleccionada;
        public Categoria CategoriaFiltroSeleccionada
        {
            get => _categoriaFiltroSeleccionada;
            set
            {
                // CAMBIO: Usar SetPropertyAndCheck y llamar al método correcto
                if (SetPropertyAndCheck(ref _categoriaFiltroSeleccionada, value))
                {
                    ApplyFilterAndResetPage();
                }
            }
        }
        private ProductoDisplayViewModel _productoSeleccionado;
        public ProductoDisplayViewModel ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                if (SetPropertyAndCheck(ref _productoSeleccionado, value))
                {
                    (EditarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                    (CambiarEstadoProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                    CambiarEstadoButtonText = value?.Estado == true ? "Desactivar" : "Activar";
                }
            }
        }

        private string _stockStatusFiltroSeleccionado;
        public string StockStatusFiltroSeleccionado
        {
            get => _stockStatusFiltroSeleccionado;
            set
            {
                // CAMBIO: Usar SetPropertyAndCheck y llamar al método correcto
                if (SetPropertyAndCheck(ref _stockStatusFiltroSeleccionado, value))
                {
                    ApplyFilterAndResetPage();
                }
            }
        }
        public ObservableCollection<ProductoDisplayViewModel> Productos => Items;

        // --- Propiedades para el DataGrid y la Selección ---
        public ObservableCollection<Categoria> CategoriasDisponibles { get; private set; }
        public ObservableCollection<ColumnViewModel> Columns { get; }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            private set { SetProperty(ref _totalItems, value); OnPropertyChanged(nameof(TotalPaginas)); }
        }

        public ObservableCollection<string> StockStatusOptions { get; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / TamañoDePagina);

        private decimal _valorTotalInventario;
        public decimal ValorTotalInventario
        {
            get => _valorTotalInventario;
            set => SetProperty(ref _valorTotalInventario, value);
        }

        private int _productosAgotados;
        public int ProductosAgotados
        {
            get => _productosAgotados;
            set => SetProperty(ref _productosAgotados, value);
        }

        private int _productosBajoStock;
        public int ProductosBajoStock
        {
            get => _productosBajoStock;
            set => SetProperty(ref _productosBajoStock, value);
        }


        public ICommand ExportarExcelCommand { get; }

        // --- Comandos ---
        public ICommand NuevoProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand CambiarEstadoProductoCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand AjusteManualCommand { get; }

        // --- Constructor ---
        public CatalogoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService, IStockAlertService stockAlertService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _stockAlertService = stockAlertService;
            _notificationService = notificationService;

            // CAMBIO: Inicializar TODAS las colecciones
            CategoriasDisponibles = new ObservableCollection<Categoria>();
            StockStatusOptions = new ObservableCollection<string> { "Todos", "En Stock", "Agotado", "Bajo Stock" };
            Columns = new ObservableCollection<ColumnViewModel> { /* ... tus columnas ... */ };

            // Opciones para el filtro de Stock
            StockStatusOptions = new ObservableCollection<string> { "Todos", "En Stock", "Agotado", "Bajo Stock" };

            NuevoProductoCommand = new ViewModelCommand(async p => await ExecuteNuevoProductoCommand());
            EditarProductoCommand = new ViewModelCommand(async p => await ExecuteEditarProductoCommand(), p => CanExecuteEditDelete());
            CambiarEstadoProductoCommand = new ViewModelCommand(async p => await ExecuteCambiarEstadoProductoCommand(), p => CanExecuteEditDelete());
            LimpiarFiltrosCommand = new ViewModelCommand(ExecuteLimpiarFiltros);
            AjusteManualCommand = new ViewModelCommand(async p => await ExecuteAjusteManualCommand());
            ExportarExcelCommand = new ViewModelCommand(async p => await ExecuteExportarExcelCommand()); // ✅ INICIALIZACIÓN
            _notificationService = notificationService;
        }

        // --- Lógica Principal ---
        // Este método se llama desde el NavigationService al cargar la vista
        public async Task LoadAsync()
        {
            await LoadCategoriasAsync();
            await LoadDashboardDataAsync();
            // CAMBIO: Solo llamamos a LimpiarFiltros para la carga inicial.
            ExecuteLimpiarFiltros(null);
        }
        protected override async Task LoadItemsAsync()
        {
            var criteria = new ProductSearchCriteria
            {
                UniversalSearchText = this.SearchText,
                CategoryId = this.CategoriaFiltroSeleccionada?.Id == 0 ? null : this.CategoriaFiltroSeleccionada?.Id,
                StockStatus = this.StockStatusFiltroSeleccionado,
                PageNumber = this.NumeroDePagina,
                PageSize = this.TamañoDePagina
            };

            var pagedResult = await _unitOfWork.Productos.SearchAsync(criteria);

            TotalItems = pagedResult.TotalCount;
            foreach (var dto in pagedResult.Items)
            {
                Items.Add(new ProductoDisplayViewModel(dto));
            }
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
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Usamos los métodos que acabamos de crear en el repositorio.
                ValorTotalInventario = await _unitOfWork.Productos.GetTotalInventoryValueAsync();
                ProductosAgotados = await _unitOfWork.Productos.CountOutOfStockProductsAsync();
                // ¡Este método ya lo tenías! Lo reutilizamos.
                ProductosBajoStock = await _unitOfWork.Productos.CountLowStockProductsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL CARGAR DATOS DEL DASHBOARD: {ex.Message}");
                // No mostramos un MessageBox aquí para no ser intrusivos.
            }
        }

        private async Task ExecuteExportarExcelCommand()
        {
            // 1. Obtenemos TODOS los productos que coinciden con los filtros actuales, ignorando la paginación.
            var exportCriteria = new ProductSearchCriteria
            {
                UniversalSearchText = this.SearchText,
                CategoryId = this.CategoriaFiltroSeleccionada?.Id,
                StockStatus = this.StockStatusFiltroSeleccionado,
                PageNumber = 1,
                PageSize = int.MaxValue // Truco para obtener todos los registros
            };

            var pagedResult = await _unitOfWork.Productos.SearchAsync(exportCriteria);
            var productosAExportar = pagedResult.Items;

            if (!productosAExportar.Any())
            {
                MessageBox.Show("No hay productos para exportar con los filtros actuales.", "Información");
                return;
            }

            // 2. Usar EPPlus para crear el archivo Excel
            ExcelPackage.License.SetNonCommercialPersonal("Deluxe Cars Desktop App"); 

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Inventario");

                // Encabezados
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "OEM";
                worksheet.Cells[1, 4].Value = "Categoría";
                worksheet.Cells[1, 5].Value = "Proveedor";
                worksheet.Cells[1, 6].Value = "Precio";
                worksheet.Cells[1, 7].Value = "Stock";
                worksheet.Cells[1, 8].Value = "Activo";

                // Datos
                int row = 2;
                foreach (var producto in productosAExportar)
                {
                    worksheet.Cells[row, 1].Value = producto.Id;
                    worksheet.Cells[row, 2].Value = producto.Nombre;
                    worksheet.Cells[row, 3].Value = producto.OEM;
                    worksheet.Cells[row, 4].Value = producto.NombreCategoria;
                    worksheet.Cells[row, 5].Value = producto.NombreProveedorPrincipal;
                    worksheet.Cells[row, 6].Value = producto.Precio;
                    worksheet.Cells[row, 7].Value = producto.StockActual;
                    worksheet.Cells[row, 8].Value = producto.Estado ? "Sí" : "No";
                    row++;
                }

                // Formato y auto-ajuste de columnas
                worksheet.Cells["A1:H1"].Style.Font.Bold = true;
                worksheet.Cells["F:F"].Style.Numberformat.Format = "$#,##0.00"; // Formato de moneda
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 3. Guardar el archivo
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Inventario_DeluxeCars_{DateTime.Now:yyyyMMdd}.xlsx",
                    Filter = "Archivos de Excel (*.xlsx)|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await package.SaveAsAsync(new FileInfo(saveFileDialog.FileName));
                        MessageBox.Show("Exportación a Excel completada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ocurrió un error al guardar el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
                    StockStatus = this.StockStatusFiltroSeleccionado,
                    PageNumber = this.NumeroDePagina,
                    PageSize = this.TamañoDePagina
                };

                // 1. Llamamos al nuevo y potente SearchAsync. Devuelve todo lo que necesitamos.
                var pagedResult = await _unitOfWork.Productos.SearchAsync(criteria);

                // 2. Actualizamos el conteo total para que los controles de paginación funcionen.
                TotalItems = pagedResult.TotalCount;

                // 3. Limpiamos la lista de la UI y la llenamos con los items de la página actual.
                //    Los DTOs ya vienen con el stock calculado, ¡no hay que hacer nada más!
                Productos.Clear();
                foreach (var dto in pagedResult.Items)
                {
                    // Creamos el ProductoDisplayViewModel directamente desde el DTO
                    Productos.Add(new ProductoDisplayViewModel(dto));
                }

                // 4. Actualizamos el estado de los botones de paginación.
                (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR AL FILTRAR PRODUCTOS: {ex.Message}");
                MessageBox.Show("Ocurrió un error al cargar los productos.", "Error");
            }
            finally
            {
                _isSearching = false;
            }
        }
        private void ExecuteLimpiarFiltros(object obj)
        {
            // CAMBIO: No necesitamos el flag _isSearching
            _searchText = string.Empty;
            _categoriaFiltroSeleccionada = CategoriasDisponibles.FirstOrDefault();
            _stockStatusFiltroSeleccionado = StockStatusOptions.FirstOrDefault();

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(CategoriaFiltroSeleccionada));
            OnPropertyChanged(nameof(StockStatusFiltroSeleccionado));

            ApplyFilterAndResetPage();
        }

        private async Task ExecuteAjusteManualCommand()
        {
            await _navigationService.OpenFormWindow(FormType.AjusteInventario, 0);
            // Refrescamos la lista principal después de cerrar el diálogo
            ApplyFilterAndResetPage();
        }

        private bool CanExecuteEditDelete() => ProductoSeleccionado != null;

        private async Task ExecuteNuevoProductoCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Producto, 0);
            ApplyFilterAndResetPage();
        }

        private async Task ExecuteEditarProductoCommand()
        {
            // CORRECCIÓN: Usar el Id directamente del ViewModel seleccionado
            await _navigationService.OpenFormWindow(FormType.Producto, ProductoSeleccionado.Id);
            ApplyFilterAndResetPage();
        }

        private async Task ExecuteCambiarEstadoProductoCommand()
        {
            var productoSeleccionado = ProductoSeleccionado;
            if (productoSeleccionado == null) return;

            // Lógica para determinar la acción y el mensaje
            var productoId = productoSeleccionado.Id;
            var productoEnDB = await _unitOfWork.Productos.GetByIdAsync(productoId);

            if (productoEnDB == null)
            {
                MessageBox.Show("El producto no se encontró en la base de datos. Puede que haya sido eliminado.", "Error");
                return;
            }

            string accion = productoEnDB.Estado ? "desactivar" : "activar";
            string accionPasado = productoEnDB.Estado ? "desactivado" : "activado";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} el producto '{productoEnDB.Nombre}'?",
                                         $"Confirmar {char.ToUpper(accion[0]) + accion.Substring(1)}",
                                         MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No) return;

            try
            {
                // La nueva lógica: invierte el estado actual
                productoEnDB.Estado = !productoEnDB.Estado;
                await _unitOfWork.CompleteAsync();

                // Refrescar la vista para que el cambio se vea reflejado
                FiltrarProductos();

                _notificationService.ShowSuccess($"Producto {accionPasado} exitosamente.");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ocurrió un error al {accion} el producto.");
                MessageBox.Show($"Ocurrió un error al {accion} el producto.\n\nError: {ex.Message}",
                                $"Error de {char.ToUpper(accion[0]) + accion.Substring(1)}",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                // Revertir el cambio en el objeto si falla el guardado
                productoEnDB.Estado = !productoEnDB.Estado;
            }

            ApplyFilterAndResetPage();
        }
    }
}
