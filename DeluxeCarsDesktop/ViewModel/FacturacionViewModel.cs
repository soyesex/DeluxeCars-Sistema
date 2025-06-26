using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using Microsoft.Data.SqlClient;
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
    public class FacturacionViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStockAlertService _stockAlertService;
        private readonly IMessengerService _messengerService;

        private Factura _facturaEnProgreso;
        private bool _isInitialized = false;
        private bool _isBuscandoItems = false;
        private bool _isBuscandoClientes = false;

        // --- Propiedades para Binding (Estandarizadas con SetProperty) ---
        public ObservableCollection<Cliente> ResultadosBusquedaCliente { get; private set; }
        public ObservableCollection<MetodoPago> MetodosDePago { get; private set; }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                SetProperty(ref _clienteSeleccionado, value);
                if (value != null)
                {
                    // Actualiza el texto de búsqueda para que el usuario vea su selección
                    _textoBusquedaCliente = value.Nombre;
                    OnPropertyChanged(nameof(TextoBusquedaCliente));

                    // Cierra el popup de resultados
                    IsClientPopupOpen = false;
                }
            }
        }

        // Coloca estas propiedades junto a las otras en tu ViewModel

        private bool _isProductPopupOpen;
        public bool IsProductPopupOpen
        {
            get => _isProductPopupOpen;
            set => SetProperty(ref _isProductPopupOpen, value);
        }

        private object _itemSeleccionado;
        public object ItemSeleccionado
        {
            get => _itemSeleccionado;
            set
            {
                SetProperty(ref _itemSeleccionado, value);
                if (value != null)
                {
                    // Para obtener el nombre, necesitamos saber si es Producto o Servicio
                    string nombre = string.Empty;
                    if (value is Producto p) nombre = p.Nombre;
                    else if (value is Servicio s) nombre = s.Nombre;

                    _textoBusquedaItem = nombre;
                    OnPropertyChanged(nameof(TextoBusquedaItem));

                    IsProductPopupOpen = false;
                }
            }
        }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado { get => _metodoPagoSeleccionado; set => SetProperty(ref _metodoPagoSeleccionado, value); }

        private string _textoBusquedaCliente;
        public string TextoBusquedaCliente { get => _textoBusquedaCliente; set { SetProperty(ref _textoBusquedaCliente, value); BuscarClientes(); } }

        public ObservableCollection<DetalleFactura> LineasDeFactura { get; private set; }
        public ObservableCollection<object> ResultadosBusquedaItem { get; private set; }

        private string _textoBusquedaItem;
        public string TextoBusquedaItem { get => _textoBusquedaItem; set { SetProperty(ref _textoBusquedaItem, value); BuscarItems(); } }

        private int _cantidadItem = 1;
        public int CantidadItem { get => _cantidadItem; set => SetProperty(ref _cantidadItem, value); }

        private decimal _subTotal;
        public decimal SubTotal { get => _subTotal; private set => SetProperty(ref _subTotal, value); }

        private decimal _totalIVA;
        public decimal TotalIVA { get => _totalIVA; private set => SetProperty(ref _totalIVA, value); }

        private decimal _total;
        public decimal Total { get => _total; private set => SetProperty(ref _total, value); }

        private bool _isClientPopupOpen;
        public bool IsClientPopupOpen
        {
            get => _isClientPopupOpen;
            set => SetProperty(ref _isClientPopupOpen, value);
        }

        // --- Comandos ---
        public ICommand AgregarItemCommand { get; }
        public ICommand EliminarItemCommand { get; }
        public ICommand FinalizarVentaCommand { get; }
        public ICommand CancelarVentaCommand { get; }

        public FacturacionViewModel(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IStockAlertService stockAlertService, IMessengerService messengerService)
        {
            _unitOfWork = unitOfWork;
            _messengerService = messengerService;
            _currentUserService = currentUserService;
            _stockAlertService = stockAlertService;

            LineasDeFactura = new ObservableCollection<DetalleFactura>();
            ResultadosBusquedaCliente = new ObservableCollection<Cliente>();
            ResultadosBusquedaItem = new ObservableCollection<object>();
            MetodosDePago = new ObservableCollection<MetodoPago>();
            AgregarItemCommand = new ViewModelCommand( async p => await ExecuteAgregarItem(p), p => p != null && CantidadItem > 0);
            EliminarItemCommand = new ViewModelCommand(ExecuteEliminarItem);
            FinalizarVentaCommand = new ViewModelCommand(async p => await ExecuteFinalizarVenta(), p => LineasDeFactura.Any() && ClienteSeleccionado != null);
            CancelarVentaCommand = new ViewModelCommand(async p => await InicializarNuevaVenta());
        }
        public async Task LoadAsync()
        {
            if (_isInitialized)
            {
                // Si ya está inicializado, solo reinicia la venta si es necesario
                if (LineasDeFactura.Any() || ClienteSeleccionado != null)
                {
                    await InicializarNuevaVenta();
                }
                return;
            }
            await InicializarNuevaVenta();
            _isInitialized = true;
        }
        private async Task InicializarNuevaVenta()
        {
            _facturaEnProgreso = new Factura();
            try
            {
                var metodos = await _unitOfWork.MetodosPago.GetAllAsync();

                // --- INICIO DE LA CORRECCIÓN ---

                // 1. Limpiamos la colección existente.
                MetodosDePago.Clear();

                // 2. Añadimos los nuevos métodos a la colección que la UI ya conoce.
                foreach (var metodo in metodos.Where(m => m.Disponible))
                {
                    MetodosDePago.Add(metodo);
                }

                // Ya no necesitas la línea OnPropertyChanged(nameof(MetodosDePago));

                // --- FIN DE LA CORRECCIÓN ---

                MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Error cargando métodos de pago: {ex.Message}", "Error"); }

            LineasDeFactura.Clear();
            ResultadosBusquedaCliente?.Clear(); // Usamos el '?' por si acaso
            ResultadosBusquedaItem?.Clear();
            ClienteSeleccionado = null; // Esto ya lo tenías, pero es clave

            TextoBusquedaCliente = string.Empty; OnPropertyChanged(nameof(TextoBusquedaCliente));
            TextoBusquedaItem = string.Empty; OnPropertyChanged(nameof(TextoBusquedaItem));
            CantidadItem = 1;
            RecalcularTotales();
        }

        // --- MÉTODOS DE BÚSQUEDA CORREGIDOS ---
        // Reemplaza tu método BuscarClientes actual por este
        private async Task BuscarClientes()
        {
            // 1. Añadimos la protección de concurrencia
            if (_isBuscandoClientes) return;

            try
            {
                _isBuscandoClientes = true;

                if (string.IsNullOrWhiteSpace(TextoBusquedaCliente) || TextoBusquedaCliente.Length < 2)
                {
                    ResultadosBusquedaCliente?.Clear();
                    IsClientPopupOpen = false;
                    return;
                }

                if (ClienteSeleccionado != null && TextoBusquedaCliente == ClienteSeleccionado.Nombre)
                {
                    // Si el texto no ha cambiado desde la selección, cerramos el popup y no buscamos
                    IsClientPopupOpen = false;
                    return;
                }

                // Si el usuario sigue escribiendo, limpiamos la selección anterior
                ClienteSeleccionado = null;

                // La búsqueda ya es case-insensitive gracias a la configuración de la BD
                var clientes = await _unitOfWork.Clientes.GetByConditionAsync(c =>
                    c.Nombre.Contains(TextoBusquedaCliente) ||
                    c.Email.Contains(TextoBusquedaCliente)
                );

                // --- CORRECCIÓN CLAVE ---
                // Limpiamos y añadimos a la colección existente en lugar de crear una nueva
                ResultadosBusquedaCliente.Clear();
                foreach (var cliente in clientes)
                {
                    ResultadosBusquedaCliente.Add(cliente);
                }

                IsClientPopupOpen = ResultadosBusquedaCliente.Any();
            }
            finally
            {
                // Liberamos la bandera para la siguiente búsqueda
                _isBuscandoClientes = false;
            }
        }

        // Reemplaza tu método BuscarItems actual por este
        private async Task BuscarItems()
        {
            // 1. Chequeo de la bandera de "ocupado" al inicio.
            if (_isBuscandoItems)
                return;

            try
            {
                // 2. Se levanta la bandera para indicar que la búsqueda comienza.
                _isBuscandoItems = true;

                // =============================================================
                // AHORA VIENE TODA LA LÓGICA QUE YA TENÍAS, PERO LIMPIA
                // =============================================================

                if (string.IsNullOrWhiteSpace(TextoBusquedaItem) || TextoBusquedaItem.Length < 2)
                {
                    ResultadosBusquedaItem?.Clear();
                    IsProductPopupOpen = false;
                    return;
                }

                if (ItemSeleccionado != null)
                {
                    string nombreSeleccionado = (ItemSeleccionado as Producto)?.Nombre ?? (ItemSeleccionado as Servicio)?.Nombre;
                    if (TextoBusquedaItem == nombreSeleccionado) return;
                }

                ItemSeleccionado = null;
                ResultadosBusquedaItem.Clear();

                // Ya no usamos .ToUpper() porque la base de datos ahora ignora mayúsculas/minúsculas
                var productos = await _unitOfWork.Productos.GetByConditionAsync(p => p.Nombre.Contains(TextoBusquedaItem));
                foreach (var p in productos) { ResultadosBusquedaItem.Add(p); }

                var servicios = await _unitOfWork.Servicios.GetByConditionAsync(s => s.Nombre.Contains(TextoBusquedaItem));
                foreach (var s in servicios) { ResultadosBusquedaItem.Add(s); }

                OnPropertyChanged(nameof(ResultadosBusquedaItem));
                IsProductPopupOpen = ResultadosBusquedaItem.Any();
            }
            finally
            {
                // 3. En el 'finally', nos aseguramos de que la bandera SIEMPRE se baje,
                // permitiendo que la próxima búsqueda pueda ejecutarse.
                _isBuscandoItems = false;
            }
        }

        // La firma del método ahora devuelve un Task
        private async Task ExecuteAgregarItem(object item)
        {
            // Validación inicial (se queda igual)
            if (item == null || CantidadItem <= 0) return;

            // --- Lógica para PRODUCTOS ---
            if (item is Producto p)
            {
                // =======================================================
                // VALIDACIÓN 1: ¿HAY STOCK SUFICIENTE?
                // =======================================================
                int stockReal = await _unitOfWork.Productos.GetCurrentStockAsync(p.Id);
                if (stockReal < CantidadItem)
                {
                    MessageBox.Show($"Stock insuficiente para '{p.Nombre}'. Disponible: {stockReal}", "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Detenemos la operación.
                }

                // =======================================================
                // VALIDACIÓN 2: ¿SE ESTÁ VENDIENDO POR DEBAJO DEL COSTO?
                // =======================================================
                // Usamos UltimoPrecioCompra como el costo. Si es nulo, asumimos costo 0.
                decimal costoDelProducto = p.UltimoPrecioCompra ?? 0m;

                // La propiedad 'p.Precio' es el precio de venta que está en el catálogo.
                if (p.Precio < costoDelProducto)
                {
                    // Si el precio de venta es menor que el costo, verificamos el rol.
                    if (!_currentUserService.IsAdmin)
                    {
                        MessageBox.Show($"No tienes permiso para vender '{p.Nombre}' por debajo de su costo de ${costoDelProducto:N2}.", "Venta no Permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; // Detenemos la operación.
                    }
                }

                // --- Si todas las validaciones pasan, procedemos ---
                var detalle = new DetalleFactura
                {
                    Cantidad = CantidadItem,
                    TipoDetalle = "Producto",
                    IdItem = p.Id,
                    Descripcion = p.Nombre,
                    PrecioUnitario = p.Precio,
                    // Usamos el ?? para poner "Unidad" por defecto si un producto antiguo no la tuviera.
                    UnidadMedida = p.UnidadMedida ?? "Unidad",
                    IVA = 19 // O tomarlo de una configuración
                };
                LineasDeFactura.Add(detalle);
            }
            // --- Lógica para SERVICIOS (no cambia) ---
            else if (item is Servicio s)
            {
                var detalle = new DetalleFactura
                {
                    Cantidad = CantidadItem,
                    TipoDetalle = "Servicio",
                    IdItem = s.Id,
                    Descripcion = s.Nombre,
                    PrecioUnitario = s.Precio,
                    UnidadMedida = "Servicio",
                    IVA = 19
                };
                LineasDeFactura.Add(detalle);
            }

            // Finalmente, recalculamos los totales de la factura.
            RecalcularTotales();
            TextoBusquedaItem = string.Empty;
            CantidadItem = 1;
        }

        private void ExecuteEliminarItem(object item)
        {
            if (item is DetalleFactura detalle)
            {
                LineasDeFactura.Remove(detalle);

                RecalcularTotales();

            }
        }
        private void RecalcularTotales()
        {
            SubTotal = LineasDeFactura.Sum(d => d.Cantidad * d.PrecioUnitario);
            TotalIVA = LineasDeFactura.Sum(d => d.Cantidad * d.PrecioUnitario * (d.IVA ?? 19) / 100); // Asumimos 19% si es nulo
            Total = SubTotal + TotalIVA;
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(TotalIVA));
            OnPropertyChanged(nameof(Total));
        }
        private async Task ExecuteFinalizarVenta()
        {
            // 1. La validación inicial se queda como está. Es perfecta.
            if (ClienteSeleccionado == null || !LineasDeFactura.Any() || MetodoPagoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente, un método de pago y añadir al menos un ítem.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Llenar la cabecera de la factura también se queda igual.
            _facturaEnProgreso.IdCliente = ClienteSeleccionado.Id;
            _facturaEnProgreso.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _facturaEnProgreso.FechaEmision = DateTime.Now;
            _facturaEnProgreso.NumeroFactura = $"F-{DateTime.Now:yyyyMMddHHmmss}";
            _facturaEnProgreso.IdUsuario = _currentUserService.CurrentUserId;
            _facturaEnProgreso.SubTotal = SubTotal;
            _facturaEnProgreso.TotalIVA = TotalIVA;
            _facturaEnProgreso.Total = Total;
            _facturaEnProgreso.DetallesFactura = LineasDeFactura;

            try
            {
                // 3. Añadimos la Factura y sus Detalles al contexto.
                await _unitOfWork.Facturas.AddAsync(_facturaEnProgreso);

                // 4. Guardamos la factura PRIMERO. Esto es crucial para obtener el ID de la factura
                //    que usaremos como referencia en los movimientos de inventario.
                await _unitOfWork.CompleteAsync();

                // --- TAREA 3.1: Registrar Costo de Mercancía Vendida (CMV) ---
                // 1. Obtenemos los IDs de todos los productos que se vendieron.
                var idsDeProductosVendidos = _facturaEnProgreso.DetallesFactura
                                               .Where(d => d.TipoDetalle == "Producto")
                                               .Select(d => d.IdItem)
                                               .ToList();

                // 2. Hacemos UNA SOLA CONSULTA para traer los datos de esos productos.
                var productosVendidos = await _unitOfWork.Productos.GetByConditionAsync(p => idsDeProductosVendidos.Contains(p.Id));

                // 3. Los convertimos a un diccionario para una búsqueda instantánea y eficiente.
                var productosDict = productosVendidos.ToDictionary(p => p.Id);

                // 5. AHORA, creamos los registros de Movimiento de Inventario para cada producto vendido.
                //    Hemos eliminado por completo el bloque de "Lógica de Stock Optimizada".
                foreach (var detalle in _facturaEnProgreso.DetallesFactura)
                {
                    // Solo creamos movimientos para los detalles que son de tipo 'Producto'.
                    if (detalle.TipoDetalle == "Producto")
                    {
                        // Buscamos el producto completo en nuestro diccionario.
                        productosDict.TryGetValue(detalle.IdItem, out var productoVendido);

                        var movimiento = new MovimientoInventario
                        {
                            IdProducto = detalle.IdItem,
                            Fecha = DateTime.UtcNow,
                            TipoMovimiento = "Salida por Venta", // Es una salida por venta.
                            Cantidad = -detalle.Cantidad, // ¡LA CANTIDAD ES NEGATIVA!
                            IdReferencia = _facturaEnProgreso.Id, // El ID de la Factura que originó el movimiento.
                            // Asignamos el último costo de compra conocido. Usamos '?? 0' por si es un
                            // producto que nunca se ha comprado y su costo es nulo.
                            CostoUnitario = productoVendido?.UltimoPrecioCompra ?? 0m,
                        };
                        await _unitOfWork.Context.MovimientosInventario.AddAsync(movimiento);
                    }
                }

                // 6. Guardamos los nuevos registros de MovimientoInventario.
                await _unitOfWork.CompleteAsync();
                _messengerService.Publish(new InventarioCambiadoMessage());

                try
                {
                    // Ahora que la venta y los movimientos están confirmados,
                    // verificamos el stock de cada producto vendido.
                    foreach (var detalle in _facturaEnProgreso.DetallesFactura.Where(d => d.TipoDetalle == "Producto"))
                    {
                        await _stockAlertService.CheckAndCreateStockAlertAsync(detalle.IdItem, _unitOfWork);
                    }
                }
                catch (Exception ex)
                {
                    // Si la generación de la alerta falla, no debe detener el flujo principal de la venta.
                    // Solo lo registramos para futura depuración.
                    System.Diagnostics.Debug.WriteLine($"Error al verificar alertas de stock post-venta: {ex.Message}");
                }

                MessageBox.Show($"Venta #{_facturaEnProgreso.NumeroFactura} finalizada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await InicializarNuevaVenta(); // Limpia el formulario.
            }
            catch (Exception ex)
            {
                // Tu excelente manejador de errores detallado se queda igual.
                string errorMessage = $"Error principal: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n--- DETALLES (InnerException) ---\n{ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\n\n--- MÁS DETALLES ---\n{ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(errorMessage, "Error de Guardado Detallado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }    
}

