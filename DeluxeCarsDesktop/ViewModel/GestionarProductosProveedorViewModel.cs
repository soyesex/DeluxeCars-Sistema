using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class GestionarProductosProveedorViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private int _proveedorId;

        public Proveedor ProveedorActual { get; private set; }
        public ObservableCollection<ProductoProveedor> ProductosAsociados { get; private set; }
        public ObservableCollection<Producto> ProductosNoAsociados { get; private set; }

        public ProductoProveedor ProductoAsociadoSeleccionado { get; set; }
        public Producto ProductoNoAsociadoSeleccionado { get; set; }
        public decimal NuevoPrecioCompra { get; set; }

        private string _textoBusquedaInventario;
        public string TextoBusquedaInventario
        {
            get => _textoBusquedaInventario;
            set { SetProperty(ref _textoBusquedaInventario, value); RefreshLists(); } // Llama a RefreshLists para filtrar
        }

        public ICommand AsociarCommand { get; }
        public ICommand DesasociarCommand { get; }
        public Action CloseAction { get; set; }

        public GestionarProductosProveedorViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ProductosAsociados = new ObservableCollection<ProductoProveedor>();
            ProductosNoAsociados = new ObservableCollection<Producto>();
            AsociarCommand = new ViewModelCommand(ExecuteAsociar, p => ProductoNoAsociadoSeleccionado != null && NuevoPrecioCompra > 0);
            DesasociarCommand = new ViewModelCommand(ExecuteDesasociar, p => ProductoAsociadoSeleccionado != null);
        }

        public async Task LoadAsync(int entityId)
        {
            _proveedorId = entityId;
            ProveedorActual = await _unitOfWork.Proveedores.GetByIdAsync(_proveedorId);
            OnPropertyChanged(nameof(ProveedorActual));
            await RefreshLists();
        }

        // --- MÉTODO REFRESHLISTS ACTUALIZADO ---
        private async Task RefreshLists()
        {
            var asociados = await _unitOfWork.ProductoProveedores.GetByProveedorWithProductoAsync(_proveedorId);
            var noAsociados = await _unitOfWork.Productos.GetUnassociatedProductsAsync(_proveedorId);

            // Aplicamos el filtro de búsqueda si existe
            if (!string.IsNullOrWhiteSpace(TextoBusquedaInventario))
            {
                noAsociados = noAsociados.Where(p => p.Nombre.Contains(TextoBusquedaInventario, StringComparison.OrdinalIgnoreCase));
            }

            ProductosAsociados = new ObservableCollection<ProductoProveedor>(asociados.OrderBy(p => p.Producto.Nombre));
            ProductosNoAsociados = new ObservableCollection<Producto>(noAsociados.OrderBy(p => p.Nombre));

            OnPropertyChanged(nameof(ProductosAsociados));
            OnPropertyChanged(nameof(ProductosNoAsociados));
        }

        private async void ExecuteAsociar(object obj)
        {
            var newAssociation = new ProductoProveedor
            {
                IdProveedor = _proveedorId,
                IdProducto = ProductoNoAsociadoSeleccionado.Id,
                PrecioCompra = this.NuevoPrecioCompra
            };
            await _unitOfWork.ProductoProveedores.AddAsync(newAssociation);
            await _unitOfWork.CompleteAsync();
            await RefreshLists();
        }

        private async void ExecuteDesasociar(object obj)
        {
            if (ProductoAsociadoSeleccionado == null) return;
            try
            {
                // La lógica ahora es más simple porque el objeto seleccionado es del tipo correcto.
                await _unitOfWork.ProductoProveedores.RemoveAsync(ProductoAsociadoSeleccionado);
                await _unitOfWork.CompleteAsync();
                await RefreshLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desasociar el producto: {ex.Message}", "Error");
            }
        }
    }
}
