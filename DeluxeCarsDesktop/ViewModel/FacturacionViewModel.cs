using DeluxeCarsDesktop.Interfaces;
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
    public class FacturacionViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private Factura _facturaEnProgreso;
        private bool _isInitialized = false;
        private bool _isBuscandoItems = false;

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

        public FacturacionViewModel(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            LineasDeFactura = new ObservableCollection<DetalleFactura>();
            ResultadosBusquedaCliente = new ObservableCollection<Cliente>();
            ResultadosBusquedaItem = new ObservableCollection<object>();
            MetodosDePago = new ObservableCollection<MetodoPago>();
            AgregarItemCommand = new ViewModelCommand(ExecuteAgregarItem, p => p != null && CantidadItem > 0);
            EliminarItemCommand = new ViewModelCommand(ExecuteEliminarItem);
            FinalizarVentaCommand = new ViewModelCommand(async p => await ExecuteFinalizarVenta(), p => LineasDeFactura.Any() && ClienteSeleccionado != null);
            CancelarVentaCommand = new ViewModelCommand(async p => await InicializarNuevaVenta());
        }
        public async Task OnNavigatedTo()
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
                MetodosDePago = new ObservableCollection<MetodoPago>(metodos.Where(m => m.Disponible));
                OnPropertyChanged(nameof(MetodosDePago));
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
            if (string.IsNullOrWhiteSpace(TextoBusquedaCliente) || TextoBusquedaCliente.Length < 2)
            {
                ResultadosBusquedaCliente?.Clear();
                IsClientPopupOpen = false;
                return;
            }

            if (ClienteSeleccionado != null && TextoBusquedaCliente == ClienteSeleccionado.Nombre)
            {
                return;
            }

            ClienteSeleccionado = null;

            // --- INICIO DE LA CORRECCIÓN ---
            var textoBusquedaUpper = TextoBusquedaCliente.ToUpper();
            var clientes = await _unitOfWork.Clientes.GetByConditionAsync(c =>
                c.Nombre.ToUpper().Contains(textoBusquedaUpper) ||
                c.Email.ToUpper().Contains(textoBusquedaUpper)
            );
            // --- FIN DE LA CORRECCIÓN ---

            ResultadosBusquedaCliente = new ObservableCollection<Cliente>(clientes);
            OnPropertyChanged(nameof(ResultadosBusquedaCliente));
            IsClientPopupOpen = ResultadosBusquedaCliente.Any();
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

        private void ExecuteAgregarItem(object item)
        {
            if (item == null || CantidadItem <= 0) return;

            var detalle = new DetalleFactura { Cantidad = CantidadItem };

            if (item is Producto p)
            {
                if (p.Stock < CantidadItem) { MessageBox.Show($"Stock insuficiente para '{p.Nombre}'. Disponible: {p.Stock}", "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning); return; }


                detalle.TipoDetalle = "Producto";

                detalle.IdItem = p.Id;

                detalle.Descripcion = p.Nombre;

                detalle.PrecioUnitario = p.Precio;

                detalle.IVA = 19; // Asumir 19% o tomarlo de configuración
            }

            else if (item is Servicio s)
            {
                detalle.TipoDetalle = "Servicio";

                detalle.IdItem = s.Id;

                detalle.Descripcion = s.Nombre;

                detalle.PrecioUnitario = s.Precio;

                detalle.IVA = 19;
            }

            LineasDeFactura.Add(detalle);

            RecalcularTotales();
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
            if (ClienteSeleccionado == null || !LineasDeFactura.Any() || MetodoPagoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente, un método de pago y añadir al menos un ítem.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Llenar la cabecera
            _facturaEnProgreso.IdCliente = ClienteSeleccionado.Id;
            _facturaEnProgreso.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _facturaEnProgreso.FechaEmision = DateTime.Now;
            _facturaEnProgreso.NumeroFactura = $"F-{DateTime.Now:yyyyMMddHHmmss}";
            _facturaEnProgreso.IdUsuario = _currentUserService.CurrentUser?.Id;
            _facturaEnProgreso.SubTotal = SubTotal;
            _facturaEnProgreso.TotalIVA = TotalIVA;
            _facturaEnProgreso.Total = Total;
            _facturaEnProgreso.DetallesFactura = LineasDeFactura;

            try
            {
                await _unitOfWork.Facturas.AddAsync(_facturaEnProgreso);

                // Lógica de Stock Optimizada
                var idsProductos = LineasDeFactura.Where(d => d.TipoDetalle == "Producto").Select(d => d.IdItem).ToList();
                var productosAfectados = await _unitOfWork.Productos.GetByConditionAsync(p => idsProductos.Contains(p.Id));

                foreach (var producto in productosAfectados)
                {
                    var cantidadVendida = LineasDeFactura.First(d => d.IdItem == producto.Id).Cantidad;
                    if (producto.Stock >= cantidadVendida)
                    {
                        producto.Stock -= cantidadVendida;
                        // Ya no hay llamada a UpdateAsync. EF lo guardará al final.
                    }
                    else
                    {
                        throw new InvalidOperationException($"Stock insuficiente para el producto '{producto.Nombre}'.");
                    }
                }

                await _unitOfWork.CompleteAsync(); // Confirma la transacción (guarda factura y actualiza stock)
                MessageBox.Show($"Venta #{_facturaEnProgreso.NumeroFactura} finalizada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await InicializarNuevaVenta(); // Limpia el formulario para la siguiente venta
            }
            catch (Exception ex)
            {
                // Construimos un mensaje de error detallado para mostrarlo
                string errorMessage = $"Error principal: {ex.Message}";

                // Verificamos si hay una excepción interna (el error real de la BD)
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n--- DETALLES (InnerException) ---\n{ex.InnerException.Message}";

                    // A veces, la excepción interna tiene OTRA excepción interna.
                    // La buscamos también para tener toda la información.
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

