using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using System.Configuration;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ProductoFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockAlertService _stockAlertService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        // --- Estado Interno ---
        private Producto _productoActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding ---
        private string _tituloVentana;
        public string TituloVentana { get => _tituloVentana; set => SetProperty(ref _tituloVentana, value); }

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set { SetProperty(ref _nombre, value); (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _oem;
        public string OEM { get => _oem; set => SetProperty(ref _oem, value); }

        private decimal _precio;
        public decimal Precio
        {
            get => _precio;
            set { SetProperty(ref _precio, value); (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _unidadMedida;
        public string UnidadMedida { get => _unidadMedida; set => SetProperty(ref _unidadMedida, value); }
        private int _stockCalculado;
        public int StockCalculado { get => _stockCalculado; private set => SetProperty(ref _stockCalculado, value); }
        private int? _stockMinimo;
        public int? StockMinimo { get => _stockMinimo; set => SetProperty(ref _stockMinimo, value); }

        private int? _stockMaximo;
        public int? StockMaximo { get => _stockMaximo; set => SetProperty(ref _stockMaximo, value); }


        private string _descripcion;
        public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

        private bool _estado;
        public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

        public ObservableCollection<Categoria> Categorias { get; private set; }
        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set { SetProperty(ref _categoriaSeleccionada, value); (GuardarCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _rutaImagenSeleccionada;
        public string RutaImagenSeleccionada
        {
            get => _rutaImagenSeleccionada;
            set => SetProperty(ref _rutaImagenSeleccionada, value);
        }

        private string _imagenUrl;
        public string ImagenUrl { get => _imagenUrl; set => SetProperty(ref _imagenUrl, value); }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand NuevaCategoriaCommand { get; }
        public Action CloseAction { get; set; }
        public ICommand SeleccionarImagenCommand { get; }
        // ...junto a tus otros ICommand...
        public ICommand EliminarImagenCommand { get; }

        public ProductoFormViewModel(IUnitOfWork unitOfWork, IStockAlertService stockAlertService,
                                  INotificationService notificationService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _stockAlertService = stockAlertService;
            _notificationService = notificationService;
            _navigationService = navigationService;

            Categorias = new ObservableCollection<Categoria>();
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand, CanExecuteGuardarCommand);
            NuevaCategoriaCommand = new ViewModelCommand(ExecuteShowCategoriaCommand);
            CancelarCommand = new ViewModelCommand(p => CloseAction?.Invoke());
            SeleccionarImagenCommand = new ViewModelCommand(_ => ExecuteSeleccionarImagen());
            EliminarImagenCommand = new ViewModelCommand(_ => ExecuteEliminarImagen());
        }

        public async Task LoadAsync(int productoId)
        {
            await LoadCategoriasAsync();

            if (productoId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _productoActual = new Producto();
                TituloVentana = "Nuevo Producto";
                Estado = true; // Por defecto
                UnidadMedida = "Unidad";
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _productoActual = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(productoId);
                if (_productoActual != null)
                {
                    TituloVentana = $"Editar Producto: {_productoActual.Nombre}";
                    Nombre = _productoActual.Nombre;
                    OEM = _productoActual.OriginalEquipamentManufacture;
                    Precio = _productoActual.Precio;
                    UnidadMedida = _productoActual.UnidadMedida; // Carga la unidad de medida existente
                    StockCalculado = await _unitOfWork.Productos.GetCurrentStockAsync(productoId);
                    Descripcion = _productoActual.Descripcion;
                    StockMinimo = _productoActual.StockMinimo;
                    StockMaximo = _productoActual.StockMaximo;
                    Estado = _productoActual.Estado;
                    CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == _productoActual.IdCategoria);
                    this.ImagenUrl = _productoActual.ImagenUrl;
                }
            }
        }
        private void ExecuteSeleccionarImagen()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Seleccionar Imagen del Producto"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // --- CAMBIO 1: Leer la ruta base desde App.config ---
                    string rutaBase = ConfigurationManager.AppSettings["RutaBaseImagenes"];
                    if (string.IsNullOrEmpty(rutaBase))
                    {
                        MessageBox.Show("La ruta base para imágenes no está configurada en App.config.", "Error de Configuración");
                        return;
                    }
                    string destinationFolder = Path.Combine(rutaBase, "images", "products");

                    Directory.CreateDirectory(destinationFolder);

                    string originalFilePath = openFileDialog.FileName;
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(originalFilePath);
                    string destinationPath = Path.Combine(destinationFolder, newFileName);

                    File.Copy(originalFilePath, destinationPath);

                    // --- CAMBIO 2: Guardar la ruta RELATIVA, no solo el nombre del archivo ---
                    // Esta es la ruta que la app web podrá usar directamente.
                    ImagenUrl = $"images/products/{newFileName}";

                    // Opcional: Para la previsualización en el formulario actual, 
                    // puedes seguir usando la ruta completa del archivo que el usuario seleccionó.
                    RutaImagenSeleccionada = originalFilePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al procesar la imagen: {ex.Message}", "Error de Imagen");
                }
            }
        }
        private async Task LoadCategoriasAsync()
        {
            if (Categorias.Any())
            {
                return;
            }

            var cats = await _unitOfWork.Categorias.GetAllAsync();
            Categorias.Clear(); // Limpiamos la colección existente
            foreach (var cat in cats.OrderBy(c => c.Nombre))
            {
                Categorias.Add(cat); // Añadimos a la colección existente
            }
        }

        private bool CanExecuteGuardarCommand(object obj)
        {
            // Validación simple para habilitar el botón
            return !string.IsNullOrWhiteSpace(Nombre) && Precio > 0 && CategoriaSeleccionada != null;
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            // Validación 1: Campos obligatorios y valores negativos
            if (string.IsNullOrWhiteSpace(Nombre) || CategoriaSeleccionada == null || Precio < 0 || (StockMinimo.HasValue && StockMinimo < 0) || (StockMaximo.HasValue && StockMaximo < 0))
            {
                MessageBox.Show("Los campos Nombre, Categoría y Precio son obligatorios. Los valores de Stock no pueden ser negativos.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación 2: Coherencia entre Stock Mínimo y Máximo
            if (StockMinimo.HasValue && StockMaximo.HasValue && StockMinimo >= StockMaximo)
            {
                MessageBox.Show("El Stock Mínimo no puede ser mayor o igual al Stock Máximo.", "Error de Lógica", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _productoActual.Nombre = Nombre;
            _productoActual.OriginalEquipamentManufacture = OEM;
            _productoActual.Precio = Precio;
            _productoActual.UnidadMedida = UnidadMedida;
            _productoActual.StockMinimo = StockMinimo;
            _productoActual.StockMaximo = StockMaximo;
            _productoActual.Descripcion = Descripcion;
            _productoActual.Estado = Estado;
            _productoActual.IdCategoria = CategoriaSeleccionada.Id;
            _productoActual.ImagenUrl = this.ImagenUrl;

            try
            {
                if (_esModoEdicion)
                {
                    await _unitOfWork.Productos.UpdateAsync(_productoActual);
                }
                else
                {
                    await _unitOfWork.Productos.AddAsync(_productoActual);
                }
                await _unitOfWork.CompleteAsync();

                // Después de guardar, llamamos al servicio de alertas para este producto.
                // Si el nuevo StockMinimo que acabamos de guardar lo pone en estado
                // crítico, se generará la alerta correspondiente.
                await _stockAlertService.CheckAndCreateStockAlertAsync(_productoActual.Id, _unitOfWork);
                // --- FIN DE LA NUEVA LÓGICA ---

                _notificationService.ShowSuccess("Producto guardado exitosamente.");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el producto: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteShowCategoriaCommand(object obj)
        {
            // Abrimos el formulario de Categoría en modo creación.
            _navigationService.OpenFormWindow(Utils.FormType.Categoria, 0);
            // No necesitamos esperar a que se cierre, ya que las categorías se recargarán al abrir el formulario de Producto.

        }

        private void ExecuteEliminarImagen()
        {
            // Simplemente limpiamos la URL. El binding se encargará de actualizar la vista.
            // Si quieres poner una imagen por defecto, aquí asignarías su ruta.
            ImagenUrl = null;
        }
    }
}
