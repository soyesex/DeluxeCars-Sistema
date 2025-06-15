using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class GestionarProductosProveedorViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private int _proveedorId;

        public Proveedor ProveedorActual { get; private set; }
        public ObservableCollection<Producto> ProductosAsociados { get; private set; }
        public ObservableCollection<Producto> ProductosNoAsociados { get; private set; }

        public Producto ProductoAsociadoSeleccionado { get; set; }
        public Producto ProductoNoAsociadoSeleccionado { get; set; }
        public decimal NuevoPrecioCompra { get; set; }

        public ICommand AsociarCommand { get; }
        public ICommand DesasociarCommand { get; }
        public Action CloseAction { get; set; }

        public GestionarProductosProveedorViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ProductosAsociados = new ObservableCollection<Producto>();
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

        private async Task RefreshLists()
        {
            var asociados = await _unitOfWork.Productos.GetAssociatedProductsAsync(_proveedorId);
            var noAsociados = await _unitOfWork.Productos.GetUnassociatedProductsAsync(_proveedorId);
            ProductosAsociados = new ObservableCollection<Producto>(asociados.OrderBy(p => p.Nombre));
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
            var associationToRemove = await _unitOfWork.ProductoProveedores
                .GetByConditionAsync(pp => pp.IdProveedor == _proveedorId && pp.IdProducto == ProductoAsociadoSeleccionado.Id);

            if (associationToRemove.Any())
            {
                await _unitOfWork.ProductoProveedores.RemoveAsync(associationToRemove.First());
                await _unitOfWork.CompleteAsync();
                await RefreshLists();
            }
        }
    }
}
