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
    public class PedidoFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private Pedido _pedidoActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding (Estandarizadas con SetProperty) ---
        public ObservableCollection<Proveedor> Proveedores { get; private set; }
        public ObservableCollection<MetodoPago> MetodosDePago { get; private set; }

        private Proveedor _proveedorSeleccionado;
        public Proveedor ProveedorSeleccionado { get => _proveedorSeleccionado; set => SetProperty(ref _proveedorSeleccionado, value); }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado { get => _metodoPagoSeleccionado; set => SetProperty(ref _metodoPagoSeleccionado, value); }

        private DateTime _fechaEmision;
        public DateTime FechaEmision { get => _fechaEmision; set => SetProperty(ref _fechaEmision, value); }

        private DateTime _plazoEntrega;
        public DateTime PlazoEntrega { get => _plazoEntrega; set => SetProperty(ref _plazoEntrega, value); }

        private string _observaciones;
        public string Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }

        public ObservableCollection<DetallePedido> LineasDePedido { get; private set; }
        public ObservableCollection<Producto> ResultadosBusquedaProducto { get; private set; }

        private string _textoBusquedaProducto;
        public string TextoBusquedaProducto { get => _textoBusquedaProducto; set { SetProperty(ref _textoBusquedaProducto, value); BuscarProductos(); } }

        private int _cantidadItem = 1;
        public int CantidadItem { get => _cantidadItem; set => SetProperty(ref _cantidadItem, value); }

        private decimal _precioCompraItem;
        public decimal PrecioCompraItem { get => _precioCompraItem; set => SetProperty(ref _precioCompraItem, value); }

        private decimal _totalPedido;
        public decimal TotalPedido { get => _totalPedido; private set => SetProperty(ref _totalPedido, value); }

        // --- Comandos y Acción de Cierre ---
        public ICommand AgregarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand GuardarPedidoCommand { get; }
        public ICommand CancelarPedidoCommand { get; }
        public Action CloseAction { get; set; }

        public PedidoFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            LineasDePedido = new ObservableCollection<DetallePedido>();
            ResultadosBusquedaProducto = new ObservableCollection<Producto>();
            Proveedores = new ObservableCollection<Proveedor>();
            MetodosDePago = new ObservableCollection<MetodoPago>();

            AgregarProductoCommand = new ViewModelCommand(ExecuteAgregarProductoCommand);
            EliminarProductoCommand = new ViewModelCommand(ExecuteEliminarProductoCommand);
            GuardarPedidoCommand = new ViewModelCommand(ExecuteGuardarPedidoCommand);
            CancelarPedidoCommand = new ViewModelCommand(ExecuteCancelarPedidoCommand);

        }

        public async Task LoadAsync(int entityId = 0)
        {
            await LoadProveedoresYMetodosDePago();

            if (entityId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _pedidoActual = new Pedido();
                FechaEmision = DateTime.Now;
                PlazoEntrega = DateTime.Now.AddDays(15);
                ProveedorSeleccionado = Proveedores.FirstOrDefault();
                MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();
                LineasDePedido.Clear();
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _pedidoActual = await _unitOfWork.Pedidos.GetPedidoWithDetailsAsync(entityId);
                if (_pedidoActual != null)
                {
                    ProveedorSeleccionado = Proveedores.FirstOrDefault(p => p.Id == _pedidoActual.IdProveedor);
                    MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault(m => m.Id == _pedidoActual.IdMetodoPago);
                    FechaEmision = _pedidoActual.FechaEmision;
                    PlazoEntrega = _pedidoActual.PlazoEntrega;
                    Observaciones = _pedidoActual.Observaciones;
                    LineasDePedido = new ObservableCollection<DetallePedido>(_pedidoActual.DetallesPedidos);
                }
            }
            RecalcularTotal();
        }

        private async Task LoadProveedoresYMetodosDePago()
        {
            try
            {
                var provs = await _unitOfWork.Proveedores.GetAllAsync();
                var metodos = await _unitOfWork.MetodosPago.GetAllAsync();

                Proveedores = new ObservableCollection<Proveedor>(provs.OrderBy(p => p.RazonSocial));
                MetodosDePago = new ObservableCollection<MetodoPago>(metodos.Where(m => m.Disponible).OrderBy(m => m.Descripcion));

                // Notificamos a la UI que estas colecciones han cambiado
                OnPropertyChanged(nameof(Proveedores));
                OnPropertyChanged(nameof(MetodosDePago));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BuscarProductos()
        {
            // Validación: No buscar si no hay proveedor o el texto es muy corto.
            if (ProveedorSeleccionado == null || ProveedorSeleccionado.Id == 0)
            {
                ResultadosBusquedaProducto.Clear();
                return;
            }
            if (string.IsNullOrWhiteSpace(TextoBusquedaProducto) || TextoBusquedaProducto.Length < 3)
            {
                ResultadosBusquedaProducto.Clear();
                return;
            }

            try
            {
                // ¡Ahora llamamos a nuestro nuevo método del repositorio!
                var productos = await _unitOfWork.Productos.SearchProductsBySupplierAsync(ProveedorSeleccionado.Id, TextoBusquedaProducto);

                ResultadosBusquedaProducto.Clear();
                foreach (var p in productos)
                {
                    ResultadosBusquedaProducto.Add(p);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando productos por proveedor: {ex.Message}");
            }
        }

        private void ExecuteAgregarProductoCommand(object item)
        {
            if (item is not Producto producto || CantidadItem <= 0 || PrecioCompraItem < 0) return;
            var detalle = new DetallePedido
            {
                IdProducto = producto.Id,
                Producto = producto,
                Cantidad = CantidadItem,
                PrecioUnitario = PrecioCompraItem,
                Descripcion = producto.Nombre,
            };
            LineasDePedido.Add(detalle);
            RecalcularTotal();
            // Limpiar campos
             TextoBusquedaProducto = string.Empty; 
            OnPropertyChanged(nameof(TextoBusquedaProducto));
            CantidadItem = 1; 
            OnPropertyChanged(nameof(CantidadItem));
            PrecioCompraItem = 0; 
            OnPropertyChanged(nameof(PrecioCompraItem));
        }

        private void ExecuteEliminarProductoCommand(object item)
        {
            if (item is DetallePedido detalle)
            {
                LineasDePedido.Remove(detalle);
                RecalcularTotal();
            }
        }

        private void RecalcularTotal()
        {
            TotalPedido = LineasDePedido.Sum(d => d.Cantidad * d.PrecioUnitario);
            OnPropertyChanged(nameof(TotalPedido));
        }

        private async void ExecuteGuardarPedidoCommand(object obj)
        {
            if (ProveedorSeleccionado == null || MetodoPagoSeleccionado == null || !LineasDePedido.Any())
            {
                MessageBox.Show("Debe seleccionar un proveedor, un método de pago y añadir al menos un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Actualizamos el objeto que estamos rastreando
            _pedidoActual.IdProveedor = ProveedorSeleccionado.Id;
            _pedidoActual.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _pedidoActual.FechaEmision = FechaEmision;
            _pedidoActual.PlazoEntrega = PlazoEntrega;
            _pedidoActual.Observaciones = Observaciones;
            _pedidoActual.DetallesPedidos = LineasDePedido;

            try
            {
                // CORRECCIÓN: La lógica de Añadir/Actualizar va DENTRO del try
                if (!_esModoEdicion)
                {
                    _pedidoActual.NumeroPedido = $"PED-{DateTime.Now:yyyyMMddHHmmss}";
                    // TODO: Reemplazar con el ID del usuario logueado
                    _pedidoActual.IdUsuario = 1;
                    await _unitOfWork.Pedidos.AddAsync(_pedidoActual);
                }
                else
                {
                    // Al ser modo edición, la entidad _pedidoActual ya está siendo rastreada por el DbContext.
                    // No es estrictamente necesario llamar a Update, pero hacerlo es una buena práctica explícita.
                    await _unitOfWork.Pedidos.UpdateAsync(_pedidoActual);
                }

                await _unitOfWork.CompleteAsync(); // Guardamos todos los cambios
                MessageBox.Show("Pedido guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el pedido: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelarPedidoCommand(object obj)
        {
            if (LineasDePedido.Any())
            {
                var result = MessageBox.Show("¿Estás seguro de que deseas cancelar este pedido? Se perderán los datos ingresados.", "Confirmar Cancelación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            CloseAction?.Invoke();
        }
    }
}
