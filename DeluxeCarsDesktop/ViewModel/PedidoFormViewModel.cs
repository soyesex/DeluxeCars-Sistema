using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsShared.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class PedidoFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        private Pedido _pedidoActual;
        private bool _esModoEdicion;
        private bool _isBuscandoProductos = false;
        private Producto _productoSeleccionado;
        private bool _isProductPopupOpen;
        private bool _isUpdatingFromSelection = false;

        // --- Propiedades para Binding (Estandarizadas con SetProperty) ---
        // En PedidoFormViewModel.cs, junto a las otras propiedades
        public ObservableCollection<Producto> ProductosDisponibles { get; private set; }
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                SetProperty(ref _productoSeleccionado, value);

                if (value != null)
                {
                    _isUpdatingFromSelection = true;
                    TextoBusquedaProducto = value.Nombre;
                    _isUpdatingFromSelection = false;

                    IsProductPopupOpen = false;

                    // --- LÓGICA MODIFICADA ---
                    // Disparamos el método asíncrono para cargar el precio
                    _ = CargarPrecioDeCompraAsync(value);
                }

                (AgregarProductoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
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
        public Proveedor ProveedorSeleccionado
        {
            get => _proveedorSeleccionado;
            set
            {
                SetProperty(ref _proveedorSeleccionado, value);
                // Si cambia el proveedor, limpiamos la búsqueda y los items del pedido actual.
                TextoBusquedaProducto = string.Empty;
                ResultadosBusquedaProducto.Clear();
                IsProductPopupOpen = false;
                _ = CargarProductosPorProveedorAsync();
            }
        }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado { get => _metodoPagoSeleccionado; set => SetProperty(ref _metodoPagoSeleccionado, value); }

        private DateTime _fechaEmision;
        public DateTime FechaEmision { get => _fechaEmision; private set => SetProperty(ref _fechaEmision, value); }

        private DateTime _fechaEstimadaEntrega;
        public DateTime FechaEstimadaEntrega { get => _fechaEstimadaEntrega; set => SetProperty(ref _fechaEstimadaEntrega, value); }

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

        private string _botonGuardarTexto;
        public string BotonGuardarTexto { get => _botonGuardarTexto; private set => SetProperty(ref _botonGuardarTexto, value); }

        private bool _puedeEditar;
        public bool PuedeEditar { get => _puedeEditar; private set => SetProperty(ref _puedeEditar, value); }
        public EstadoPedido EstadoActual => _pedidoActual?.Estado ?? EstadoPedido.Borrador; // MODIFICADO: Usa el enum

        // --- Comandos y Acción de Cierre ---
        public ICommand AgregarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand GuardarPedidoCommand { get; }
        public ICommand CancelarPedidoCommand { get; }
        public Action CloseAction { get; set; }

        public PedidoFormViewModel(IUnitOfWork unitOfWork, INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _currentUserService = currentUserService;

            LineasDePedido = new ObservableCollection<DetallePedido>();
            ResultadosBusquedaProducto = new ObservableCollection<Producto>();
            Proveedores = new ObservableCollection<Proveedor>();
            MetodosDePago = new ObservableCollection<MetodoPago>();
            ProductosDisponibles = new ObservableCollection<Producto>();

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
                _pedidoActual = new Pedido { Estado = EstadoPedido.Borrador };
                FechaEmision = DateTime.Now;
                FechaEstimadaEntrega = DateTime.Now.AddDays(15);
                ProveedorSeleccionado = Proveedores.FirstOrDefault();
                MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();
                BotonGuardarTexto = "Crear Pedido";
                PuedeEditar = true; // Un nuevo pedido siempre es editable
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
                    FechaEstimadaEntrega = _pedidoActual.FechaEstimadaEntrega;
                    Observaciones = _pedidoActual.Observaciones;
                    LineasDePedido = new ObservableCollection<DetallePedido>(_pedidoActual.DetallesPedidos);
                    OnPropertyChanged(nameof(EstadoActual));

                    // --- LÓGICA DE ESTADOS CON ENUM ---
                    switch (_pedidoActual.Estado)
                    {
                        case EstadoPedido.Borrador:
                            BotonGuardarTexto = "Aprobar Pedido";
                            PuedeEditar = true;
                            break;
                        case EstadoPedido.Aprobado:
                            BotonGuardarTexto = "Guardar Cambios";
                            PuedeEditar = true; // Permitimos editar un pedido aprobado pero no recibido
                            break;
                        default: // Recibido, Cancelado
                            BotonGuardarTexto = "Pedido Cerrado";
                            PuedeEditar = false;
                            break;
                    }
                }
            }
            OnPropertyChanged(nameof(EstadoActual));
            RecalcularTotal();
        }

        // En PedidoFormViewModel.cs

        private async Task LoadProveedoresYMetodosDePago()
        {
            try
            {
                var provs = await _unitOfWork.Proveedores.GetAllAsync();
                var metodos = await _unitOfWork.MetodosPago.GetAllAsync();

                Proveedores.Clear();
                MetodosDePago.Clear();

                // Llenamos las colecciones existentes una por una.
                // Esto mantiene el enlace con la UI y la notifica de cada nuevo item.
                foreach (var p in provs.OrderBy(p => p.RazonSocial))
                {
                    Proveedores.Add(p);
                }
                foreach (var m in metodos.Where(m => m.Disponible).OrderBy(m => m.Descripcion))
                {
                    MetodosDePago.Add(m);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task CargarProductosPorProveedorAsync()
        {
            // Limpiamos la lista de productos cada vez que se llama
            ProductosDisponibles.Clear();

            if (ProveedorSeleccionado != null && ProveedorSeleccionado.Id != 0)
            {
                try
                {
                    // Usamos el método que ya existe para traer los productos asociados
                    var productos = await _unitOfWork.Productos.GetAssociatedProductsAsync(ProveedorSeleccionado.Id);
                    foreach (var producto in productos.OrderBy(p => p.Nombre))
                    {
                        ProductosDisponibles.Add(producto);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cargando los productos del proveedor: {ex.Message}", "Error");
                }
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
                MessageBox.Show($"La búsqueda falló: {ex.Message}", "Error en Búsqueda", MessageBoxButton.OK, MessageBoxImage.Error);
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
            return ProductoSeleccionado != null && CantidadItem > 0 && PrecioCompraItem > 0; // Cambiado a > 0
        }

        private void ExecuteAgregarProductoCommand(object item)
        {
            if (item is not Producto producto || CantidadItem <= 0 || PrecioCompraItem < 0) return;
            var detalle = new DetallePedido
            {
                IdProducto = producto.Id,
                Cantidad = CantidadItem,
                PrecioUnitario = PrecioCompraItem,
                Descripcion = producto.Nombre,
                UnidadMedida = "Unidad"
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
            // 1. La validación inicial se queda como está.
            if (ProveedorSeleccionado == null || MetodoPagoSeleccionado == null || !LineasDePedido.Any())
            {
                MessageBox.Show("Debe seleccionar un proveedor, un método de pago y añadir al menos un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Poblamos el objeto Pedido con todos los datos de la UI ANTES de intentar guardarlo.
            _pedidoActual.IdProveedor = ProveedorSeleccionado.Id;
            _pedidoActual.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _pedidoActual.FechaEmision = FechaEmision;
            _pedidoActual.Observaciones = Observaciones;
            _pedidoActual.DetallesPedidos = LineasDePedido;
            _pedidoActual.FechaEstimadaEntrega = FechaEstimadaEntrega;

            // 3. Asignamos el NumeroPedido y otros valores por defecto FUERA del try, si es un pedido nuevo.
            //    Esto garantiza que el objeto esté completo antes de la transacción.
            if (!_esModoEdicion)
            {
                _pedidoActual.NumeroPedido = $"PED-{DateTime.Now:yyyyMMddHHmmss}";
                _pedidoActual.IdUsuario = _currentUserService.CurrentUserId.Value;
                _pedidoActual.Estado = EstadoPedido.Aprobado; // Se aprueba al crear
            }
            else if (_pedidoActual.Estado == EstadoPedido.Borrador)
            {
                _pedidoActual.Estado = EstadoPedido.Aprobado;
            }

            try
            {
                // 1. Obtenemos los IDs de todos los productos del pedido.
                var idsDeProductos = _pedidoActual.DetallesPedidos.Select(d => d.IdProducto).ToList();

                // 2. Hacemos UNA SOLA CONSULTA a la BD para traer todos esos productos a la vez.
                //    Esto es importante para que Entity Framework los "vigile" (track).
                var productosDelPedido = await _unitOfWork.Productos.GetByConditionAsync(p => idsDeProductos.Contains(p.Id));

                // 3. Los convertimos a un diccionario para una búsqueda instantánea en memoria.
                var productosDict = productosDelPedido.ToDictionary(p => p.Id);

                // 4. Ahora, actualizamos la propiedad en cada objeto de producto en memoria.
                foreach (var detalle in _pedidoActual.DetallesPedidos)
                {
                    if (productosDict.TryGetValue(detalle.IdProducto, out var productoAActualizar))
                    {
                        productoAActualizar.UltimoPrecioCompra = detalle.PrecioUnitario;
                    }
                }

                // Lógica de persistencia en la BD
                if (!_esModoEdicion)
                {
                    // Sugerencia: Usar el repositorio es más consistente con el patrón
                    await _unitOfWork.Pedidos.AddAsync(_pedidoActual);
                }
                // Al llamar a CompleteAsync, EF guardará tanto el nuevo Pedido
                // como las actualizaciones en los Productos modificados.
                await _unitOfWork.CompleteAsync();

                _notificationService.ShowSuccess("Pedido guardado exitosamente.");
                CloseAction?.Invoke();
            }
            catch (DbUpdateException dbEx) // El 'catch' específico para errores de BD
            {
                // Sacamos el mensaje de la excepción más interna, que es la que viene de SQL Server.
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                Debug.WriteLine(innerExceptionMessage);
                MessageBox.Show($"Error de base de datos al guardar el pedido:\n\n{innerExceptionMessage}",
                                "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error");
            }
        }
        // --- NUEVO MÉTODO ASÍNCRONO ---
        private async Task CargarPrecioDeCompraAsync(Producto producto)
        {
            if (producto == null || ProveedorSeleccionado == null)
            {
                PrecioCompraItem = 0;
                return;
            }

            try
            {
                var prodProv = await _unitOfWork.ProductoProveedores.GetByProductAndSupplierAsync(producto.Id, ProveedorSeleccionado.Id);
                // Si se encuentra la asociación, se usa su precio. Si no, se deja en 0.
                PrecioCompraItem = prodProv?.PrecioCompra ?? 0;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"No se pudo cargar el precio de compra: {ex.Message}");
                PrecioCompraItem = 0;
            }
        }
        // <<< CAMBIO 4: NUEVO MÉTODO PARA RESETEAR LA BÚSQUEDA.
        private void LimpiarBusquedaProducto()
        {
            ResultadosBusquedaProducto.Clear();
            IsProductPopupOpen = false;
            TextoBusquedaProducto = string.Empty;
            PrecioCompraItem = 0;
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
