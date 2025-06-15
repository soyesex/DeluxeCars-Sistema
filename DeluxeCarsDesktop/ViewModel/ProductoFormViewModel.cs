using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ProductoFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Estado Interno ---
        private Producto _productoActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding ---
        private string _tituloVentana;
        public string TituloVentana { get => _tituloVentana; set => SetProperty(ref _tituloVentana, value); }

        private string _nombre;
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

        private string _oem;
        public string OEM { get => _oem; set => SetProperty(ref _oem, value); }

        private decimal _precio;
        public decimal Precio { get => _precio; set => SetProperty(ref _precio, value); }

        private int _stock;
        public int Stock { get => _stock; set => SetProperty(ref _stock, value); }

        private string _descripcion;
        public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

        private bool _estado;
        public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

        public ObservableCollection<Categoria> Categorias { get; private set; }
        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set => SetProperty(ref _categoriaSeleccionada, value); }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }

        public ProductoFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Categorias = new ObservableCollection<Categoria>();
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(p => CloseAction?.Invoke());
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
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _productoActual = await _unitOfWork.Productos.GetByIdAsync(productoId);
                if (_productoActual != null)
                {
                    TituloVentana = "Editar Producto";
                    Nombre = _productoActual.Nombre;
                    OEM = _productoActual.OriginalEquipamentManufacture;
                    Precio = _productoActual.Precio;
                    Stock = _productoActual.Stock;
                    Descripcion = _productoActual.Descripcion;
                    Estado = _productoActual.Estado;
                    CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == _productoActual.IdCategoria);
                }
            }
        }

        private async Task LoadCategoriasAsync()
        {
            var cats = await _unitOfWork.Categorias.GetAllAsync();
            Categorias = new ObservableCollection<Categoria>(cats.OrderBy(c => c.Id));
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Nombre) || CategoriaSeleccionada == null || Precio < 0 || Stock < 0)
            {
                MessageBox.Show("Nombre, Categoría, Precio y Stock son obligatorios. Precio y Stock no pueden ser negativos.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _productoActual.Nombre = Nombre;
            _productoActual.OriginalEquipamentManufacture = OEM;
            _productoActual.Precio = Precio;
            _productoActual.Stock = Stock;
            _productoActual.Descripcion = Descripcion;
            _productoActual.Estado = Estado;
            _productoActual.IdCategoria = CategoriaSeleccionada.Id;

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

                MessageBox.Show("Producto guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el producto: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
