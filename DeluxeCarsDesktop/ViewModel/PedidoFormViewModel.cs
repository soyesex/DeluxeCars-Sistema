using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
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
        private bool _isBuscandoProductos = false;
        private Producto _productoSeleccionado;
        private bool _isProductPopupOpen;
        private bool _isUpdatingFromSelection = false;

        // --- Propiedades para Binding (Estandarizadas con SetProperty) ---
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                SetProperty(ref _productoSeleccionado, value);
                (AgregarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();

                if (value != null)
                {
                    // 1. Levantamos la bandera para indicar que estamos actualizando desde una selección.
                    _isUpdatingFromSelection = true;

                    // 2. Actualizamos el texto del buscador.
                    TextoBusquedaProducto = value.Nombre;

                    // 3. Bajamos la bandera inmediatamente después.
                    _isUpdatingFromSelection = false;

                    IsProductPopupOpen = false;
                }
            }
        }

        public bool IsProductPopupOpen
        {
            get => _isProductPopupOpen;
            set => SetProperty(ref _isProductPopupOpen, value);
        }
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
        public string TextoBusquedaProducto
        {
            get => _textoBusquedaProducto;
            set
            {
                SetProperty(ref _textoBusquedaProducto, value);

                // ¡LA CLAVE! Solo buscamos si el cambio NO vino de seleccionar un producto.
                if (!_isUpdatingFromSelection)
                {
                    BuscarProductos();
                }
            }
        }

        private int _cantidadItem = 1;
        public int CantidadItem
        {
            get => _cantidadItem;
            set
            {
                // 1. Llama a SetProperty para actualizar el valor.
                SetProperty(ref _cantidadItem, value);

                // 2. Notifica al comando que debe re-evaluar su estado.
                (AgregarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        private decimal _precioCompraItem;
        public decimal PrecioCompraItem
        {
            get => _precioCompraItem;
            set
            {
                // 1. Llama a SetProperty para actualizar el valor.
                SetProperty(ref _precioCompraItem, value);

                // 2. Notifica al comando que debe re-evaluar su estado.
                (AgregarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

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

            AgregarProductoCommand = new ViewModelCommand(ExecuteAgregarProductoCommand, CanExecuteAgregarProductoCommand);
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
            if (_isBuscandoProductos) return;

            try
            {
                _isBuscandoProductos = true;

                if (ProveedorSeleccionado == null || ProveedorSeleccionado.Id == 0)
                {
                    ResultadosBusquedaProducto.Clear();
                    IsProductPopupOpen = false;
                    return;
                }
                if (string.IsNullOrWhiteSpace(TextoBusquedaProducto) || TextoBusquedaProducto.Length < 2)
                {
                    ResultadosBusquedaProducto.Clear();
                    IsProductPopupOpen = false;
                    return;
                }

                // El bloque problemático ha sido eliminado.

                var productos = await _unitOfWork.Productos.SearchProductsBySupplierAsync(ProveedorSeleccionado.Id, TextoBusquedaProducto);

                ResultadosBusquedaProducto.Clear();
                foreach (var p in productos)
                {
                    ResultadosBusquedaProducto.Add(p);
                }

                IsProductPopupOpen = ResultadosBusquedaProducto.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando productos por proveedor: {ex.Message}");
                IsProductPopupOpen = false;
            }
            finally
            {
                _isBuscandoProductos = false;
            }
        }
        private bool CanExecuteAgregarProductoCommand(object obj)
        {
            // El botón solo estará activo si hay un producto seleccionado,
            // la cantidad es válida y el precio es válido.
            return ProductoSeleccionado != null && CantidadItem > 0 && PrecioCompraItem >= 0;
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

        // EN: PedidoFormViewModel.cs

        private async void ExecuteGuardarPedidoCommand(object obj)
        {
            // 1. La validación inicial se queda como está.
            if (ProveedorSeleccionado == null || MetodoPagoSeleccionado == null || !LineasDePedido.Any())
            {
                MessageBox.Show("Debe seleccionar un proveedor, un método de pago y añadir al menos un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- INICIO DE LA CORRECCIÓN ---

            // 2. Poblamos el objeto Pedido con todos los datos de la UI ANTES de intentar guardarlo.
            _pedidoActual.IdProveedor = ProveedorSeleccionado.Id;
            _pedidoActual.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _pedidoActual.FechaEmision = FechaEmision;
            _pedidoActual.PlazoEntrega = PlazoEntrega;
            _pedidoActual.Observaciones = Observaciones;
            _pedidoActual.DetallesPedidos = LineasDePedido;

            // 3. Asignamos el NumeroPedido y otros valores por defecto FUERA del try, si es un pedido nuevo.
            //    Esto garantiza que el objeto esté completo antes de la transacción.
            if (!_esModoEdicion && string.IsNullOrEmpty(_pedidoActual.NumeroPedido))
            {
                _pedidoActual.NumeroPedido = $"PED-{DateTime.Now:yyyyMMddHHmmss}";
                // TODO: Reemplazar con el ID del usuario logueado
                _pedidoActual.IdUsuario = 1;
            }

            try
            {
                if (!_esModoEdicion)
                {
                    // 4. Aplicamos el control de estado manual para evitar el error de tracking.
                    _unitOfWork.Context.Pedidos.Add(_pedidoActual);
                    foreach (var detalle in _pedidoActual.DetallesPedidos)
                    {
                        _unitOfWork.Context.Entry(detalle.Producto).State = EntityState.Unchanged;
                    }
                }
                else
                {
                    // La lógica de edición
                    _unitOfWork.Context.Pedidos.Update(_pedidoActual);
                }

                // 5. Guardamos todo en una sola transacción atómica.
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Pedido guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                // Usamos el MessageBox detallado para cualquier error.
                string errorMessage = $"Error principal: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n--- DETALLES ---\n{ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Error de Guardado Detallado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // --- FIN DE LA CORRECCIÓN ---
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
