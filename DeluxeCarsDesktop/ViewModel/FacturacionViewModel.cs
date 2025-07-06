using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsDesktop.Services;
using DeluxeCarsEntities;
using DeluxeCarsShared.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class FacturacionViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias (sin cambios) ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStockAlertService _stockAlertService;
        private readonly IMessengerService _messengerService;

        // --- Estado Interno ---
        private Factura _facturaEnProgreso;
        private bool _isBuscandoItems = false;

        // --- NUEVA PROPIEDAD PARA RECIBIR EL ID ---
        /// <summary>
        /// El NavigationService establecerá este valor ANTES de llamar a LoadAsync.
        /// Si es 0, es una factura nueva. Si es > 0, es para un cliente específico.
        /// </summary>
        public int ClienteIdToLoad { get; set; }

        // --- PROPIEDADES PARA BINDING (Simplificadas) ---
        public ObservableCollection<Cliente> ClientesDisponibles { get; private set; }
        public ObservableCollection<MetodoPago> MetodosDePago { get; private set; }
        public ObservableCollection<DetalleFactura> LineasDeFactura { get; private set; }
        public ObservableCollection<object> ResultadosBusquedaItem { get; private set; }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set => SetProperty(ref _clienteSeleccionado, value);
        }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado { get => _metodoPagoSeleccionado; set => SetProperty(ref _metodoPagoSeleccionado, value); }

        // ... el resto de propiedades para búsqueda de items y totales se quedan igual ...
        private string _textoBusquedaItem;
        public string TextoBusquedaItem { get => _textoBusquedaItem; set { SetProperty(ref _textoBusquedaItem, value); _ = BuscarItems(); } }
        private int _cantidadItem = 1;
        public int CantidadItem { get => _cantidadItem; set => SetProperty(ref _cantidadItem, value); }
        private decimal _subTotal;
        public decimal SubTotal { get => _subTotal; private set => SetProperty(ref _subTotal, value); }
        private decimal _totalIVA;
        public decimal TotalIVA { get => _totalIVA; private set => SetProperty(ref _totalIVA, value); }
        private decimal _total;
        public decimal Total { get => _total; private set => SetProperty(ref _total, value); }
        private bool _isProductPopupOpen;
        public bool IsProductPopupOpen { get => _isProductPopupOpen; set => SetProperty(ref _isProductPopupOpen, value); }
        private object _itemSeleccionado;
        public object ItemSeleccionado
        {
            get => _itemSeleccionado;
            set
            {
                // Asignamos el valor, pero sin ejecutar ningún comando.
                SetProperty(ref _itemSeleccionado, value);

                // Si el valor no es nulo (es decir, el usuario ha seleccionado algo)...
                if (value != null)
                {
                    // 1. Obtenemos el nombre para mostrarlo en el TextBox de búsqueda.
                    string nombreAMostrar = string.Empty;
                    if (value is Producto p)
                    {
                        nombreAMostrar = p.Nombre;
                    }
                    else if (value is Servicio s)
                    {
                        nombreAMostrar = s.Nombre;
                    }

                    // 2. Actualizamos el campo privado del texto y notificamos a la UI.
                    //    Esto evita disparar otra búsqueda innecesaria.
                    _textoBusquedaItem = nombreAMostrar;
                    OnPropertyChanged(nameof(TextoBusquedaItem));

                    // 3. Cerramos el popup de resultados.
                    IsProductPopupOpen = false;
                }
            }
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

            // Inicializamos las colecciones
            LineasDeFactura = new ObservableCollection<DetalleFactura>();
            ResultadosBusquedaItem = new ObservableCollection<object>();
            ClientesDisponibles = new ObservableCollection<Cliente>();
            MetodosDePago = new ObservableCollection<MetodoPago>();

            // Inicializamos los comandos
            AgregarItemCommand = new ViewModelCommand(async p => await ExecuteAgregarItem(p), p => p != null && CantidadItem > 0);
            EliminarItemCommand = new ViewModelCommand(ExecuteEliminarItem);
            FinalizarVentaCommand = new ViewModelCommand(async p => await ExecuteFinalizarVenta(), p => LineasDeFactura.Any() && ClienteSeleccionado != null);
            CancelarVentaCommand = new ViewModelCommand(async p => await LoadAsync()); // Cancelar simplemente reinicia la venta
        }
        public async Task LoadAsync()
        {
            // Reseteamos el estado de la factura
            _facturaEnProgreso = new Factura();
            LineasDeFactura.Clear();
            TextoBusquedaItem = string.Empty;
            CantidadItem = 1;
            RecalcularTotales();

            try
            {
                // Cargamos los clientes y métodos de pago
                var clientes = await _unitOfWork.Clientes.GetByConditionAsync(c => c.Estado);
                ClientesDisponibles.Clear();
                foreach (var cliente in clientes.OrderBy(c => c.Nombre))
                {
                    ClientesDisponibles.Add(cliente);
                }

                var metodos = await _unitOfWork.MetodosPago.GetByConditionAsync(m => m.Disponible && m.AplicaParaVentas);
                MetodosDePago.Clear();
                foreach (var metodo in metodos)
                {
                    MetodosDePago.Add(metodo);
                }
                MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();

                // --- LÓGICA CLAVE PARA SELECCIONAR EL CLIENTE ---
                if (ClienteIdToLoad > 0)
                {
                    // Si se nos pasó un ID, buscamos y seleccionamos ese cliente.
                    ClienteSeleccionado = ClientesDisponibles.FirstOrDefault(c => c.Id == ClienteIdToLoad);
                }
                else
                {
                    // Si no, seleccionamos el "Consumidor Final" por defecto (asumiendo que tiene ID=1).
                    ClienteSeleccionado = ClientesDisponibles.FirstOrDefault(c => c.Id == 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inicializando la vista de facturación: {ex.Message}", "Error de Carga");
            }
        }
        private async Task InicializarNuevaVenta()
        {
            _facturaEnProgreso = new Factura();
            try
            {// --- INICIO DE LA LÓGICA MODIFICADA ---

                // Carga de Clientes
                var clientes = await _unitOfWork.Clientes.GetAllAsync();
                ClientesDisponibles.Clear();
                // Añadimos una opción para "Cliente Genérico" o "Venta Rápida"
                ClientesDisponibles.Add(new Cliente { Id = 0, Nombre = "Cliente Ocasional" });
                foreach (var cliente in clientes.Where(c => c.Estado).OrderBy(c => c.Nombre))
                {
                    ClientesDisponibles.Add(cliente);
                }

                // Carga de Métodos de Pago (sin cambios)
                var metodos = await _unitOfWork.MetodosPago.GetAllAsync();
                MetodosDePago.Clear();
                foreach (var metodo in metodos.Where(m => m.Disponible))
                {
                    MetodosDePago.Add(metodo);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error cargando métodos de pago: {ex.Message}", "Error"); }

            LineasDeFactura.Clear();
            ResultadosBusquedaItem?.Clear();

            // Seleccionamos los valores por defecto
            ClienteSeleccionado = ClientesDisponibles.FirstOrDefault();
            MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();

            TextoBusquedaItem = string.Empty;
            CantidadItem = 1;
            RecalcularTotales();

            // Notificamos a la UI que estas propiedades han cambiado
            OnPropertyChanged(nameof(TextoBusquedaItem));
        }

        // Reemplaza tu método BuscarItems actual por este
        private async Task BuscarItems()
        {
            if (_isBuscandoItems) return;

            // La lógica para no buscar si el texto es corto o no ha cambiado se mantiene
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

            try
            {
                _isBuscandoItems = true;
                ItemSeleccionado = null;
                ResultadosBusquedaItem.Clear();

                // 1. Ejecutamos la búsqueda de productos y ESPERAMOS a que termine.
                var productos = await _unitOfWork.Productos.SearchActivosConStockAsync(TextoBusquedaItem);
                foreach (var p in productos) { ResultadosBusquedaItem.Add(p); }

                // 2. UNA VEZ TERMINADA la anterior, ejecutamos la búsqueda de servicios.
                var servicios = await _unitOfWork.Servicios.GetByConditionAsync(s => s.Nombre.Contains(TextoBusquedaItem) && s.Estado == true);
                foreach (var s in servicios) { ResultadosBusquedaItem.Add(s); }

                // --- FIN DE LA CORRECCIÓN ---

                IsProductPopupOpen = ResultadosBusquedaItem.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error buscando ítems: {ex.Message}");
                // Opcional: Mostrar un mensaje de error al usuario.
                MessageBox.Show("Ocurrió un error al buscar los ítems.", "Error");
            }
            finally
            {
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
                // Calculamos y asignamos los valores aquí, en el ViewModel.
                detalle.SubTotalLinea = (detalle.Cantidad * detalle.PrecioUnitario) - (detalle.Descuento ?? 0);
                detalle.Total = detalle.SubTotalLinea * (1 + ((detalle.IVA ?? 19) / 100));

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
                // --- LÍNEAS AÑADIDAS ---
                detalle.SubTotalLinea = (detalle.Cantidad * detalle.PrecioUnitario) - (detalle.Descuento ?? 0);
                detalle.Total = detalle.SubTotalLinea * (1 + ((detalle.IVA ?? 19) / 100));

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
            Total = LineasDeFactura.Sum(d => d.Total);
            SubTotal = LineasDeFactura.Sum(d => d.SubTotalLinea);
            TotalIVA = Total - SubTotal;
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

            // --- LÓGICA DE CÁLCULO Y ASIGNACIÓN ---
            // Calculamos los totales una última vez con los ítems de la lista.
            _facturaEnProgreso.SubTotal = LineasDeFactura.Sum(d => d.SubTotalLinea);
            _facturaEnProgreso.TotalIVA = LineasDeFactura.Sum(d => d.SubTotalLinea * ((d.IVA ?? 19) / 100));
            _facturaEnProgreso.Total = _facturaEnProgreso.SubTotal + _facturaEnProgreso.TotalIVA;

            // Asignamos el resto de los datos
            _facturaEnProgreso.IdCliente = ClienteSeleccionado.Id;
            _facturaEnProgreso.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _facturaEnProgreso.FechaEmision = DateTime.Now;
            _facturaEnProgreso.NumeroFactura = $"F-{DateTime.Now:yyyyMMddHHmmss}";
            _facturaEnProgreso.IdUsuario = _currentUserService.CurrentUserId;
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

