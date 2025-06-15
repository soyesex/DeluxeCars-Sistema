using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
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
        private Factura _facturaEnProgreso;

        // --- Propiedades para la Cabecera de la Factura ---
        public ObservableCollection<Cliente> ResultadosBusquedaCliente { get; private set; }
        public ObservableCollection<MetodoPago> MetodosDePago { get; private set; }

        private string _textoBusquedaCliente;
        public string TextoBusquedaCliente { get => _textoBusquedaCliente; set { SetProperty(ref _textoBusquedaCliente, value); BuscarClientes(); } }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado { get => _clienteSeleccionado; set => SetProperty(ref _clienteSeleccionado, value); }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado { get => _metodoPagoSeleccionado; set => SetProperty(ref _metodoPagoSeleccionado, value); }

        // --- Propiedades para las Líneas de Detalle ---
        public ObservableCollection<DetalleFactura> LineasDeFactura { get; private set; }
        public ObservableCollection<object> ResultadosBusquedaItem { get; private set; }

        private string _textoBusquedaItem;
        public string TextoBusquedaItem { get => _textoBusquedaItem; set { SetProperty(ref _textoBusquedaItem, value); BuscarItems(); } }

        private int _cantidadItem = 1;
        public int CantidadItem { get => _cantidadItem; set => SetProperty(ref _cantidadItem, value); }

        // --- Propiedades para los Totales ---
        private decimal _subTotal;
        public decimal SubTotal { get => _subTotal; private set => SetProperty(ref _subTotal, value); }

        private decimal _totalIVA;
        public decimal TotalIVA { get => _totalIVA; private set => SetProperty(ref _totalIVA, value); }

        private decimal _total;
        public decimal Total { get => _total; private set => SetProperty(ref _total, value); }

        // --- Comandos ---
        public ICommand AgregarItemCommand { get; }
        public ICommand EliminarItemCommand { get; }
        public ICommand FinalizarVentaCommand { get; }
        public ICommand CancelarVentaCommand { get; }

        public FacturacionViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            InicializarNuevaVenta();

            AgregarItemCommand = new ViewModelCommand(ExecuteAgregarItem);
            EliminarItemCommand = new ViewModelCommand(ExecuteEliminarItem);
            FinalizarVentaCommand = new ViewModelCommand(ExecuteFinalizarVenta);
            CancelarVentaCommand = new ViewModelCommand(p => InicializarNuevaVenta());
        }

        private async void InicializarNuevaVenta()
        {
            _facturaEnProgreso = new Factura();
            LineasDeFactura = new ObservableCollection<DetalleFactura>();
            ResultadosBusquedaCliente = new ObservableCollection<Cliente>();
            ResultadosBusquedaItem = new ObservableCollection<object>();

            // Cargar datos iniciales
            MetodosDePago = new ObservableCollection<MetodoPago>(await _unitOfWork.MetodosPago.GetAllAsync());
            MetodoPagoSeleccionado = MetodosDePago.FirstOrDefault();

            ClienteSeleccionado = null;
            TextoBusquedaCliente = string.Empty;
            TextoBusquedaItem = string.Empty;
            CantidadItem = 1;
            RecalcularTotales();
            OnPropertyChanged(nameof(LineasDeFactura)); // Notificar a la UI
        }

        private async void BuscarClientes()
        {
            if (string.IsNullOrWhiteSpace(TextoBusquedaCliente) || TextoBusquedaCliente.Length < 3)
            {
                ResultadosBusquedaCliente.Clear();
                return;
            }
            var clientes = await _unitOfWork.Clientes.GetAllAsync(); // Se puede optimizar con un método de búsqueda
            ResultadosBusquedaCliente.Clear();
            foreach (var c in clientes.Where(c => c.Nombre.Contains(TextoBusquedaCliente, StringComparison.OrdinalIgnoreCase)))
            {
                ResultadosBusquedaCliente.Add(c);
            }
        }

        private async void BuscarItems()
        {
            if (string.IsNullOrWhiteSpace(TextoBusquedaItem) || TextoBusquedaItem.Length < 3)
            {
                ResultadosBusquedaItem.Clear();
                return;
            }
            var productos = await _unitOfWork.Productos.GetAllAsync();
            var servicios = await _unitOfWork.Servicios.GetAllAsync();

            ResultadosBusquedaItem.Clear();
            foreach (var p in productos.Where(p => p.Nombre.Contains(TextoBusquedaItem, StringComparison.OrdinalIgnoreCase))) { ResultadosBusquedaItem.Add(p); }
            foreach (var s in servicios.Where(s => s.Nombre.Contains(TextoBusquedaItem, StringComparison.OrdinalIgnoreCase))) { ResultadosBusquedaItem.Add(s); }
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
            SubTotal = LineasDeFactura.Sum(d => d.SubTotalLinea);
            TotalIVA = LineasDeFactura.Sum(d => d.SubTotalLinea * (d.IVA ?? 0) / 100);
            Total = SubTotal + TotalIVA;
        }

        private async void ExecuteFinalizarVenta(object obj)
        {
            if (ClienteSeleccionado == null) { MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!LineasDeFactura.Any()) { MessageBox.Show("La factura no tiene productos o servicios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (MetodoPagoSeleccionado == null) { MessageBox.Show("Debe seleccionar un método de pago.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            // Llenar la cabecera de la factura
            _facturaEnProgreso.IdCliente = ClienteSeleccionado.Id;
            _facturaEnProgreso.IdMetodoPago = MetodoPagoSeleccionado.Id;
            _facturaEnProgreso.FechaEmision = DateTime.Now;
            _facturaEnProgreso.NumeroFactura = $"F-{DateTime.Now:yyyyMMddHHmmss}"; // Lógica de numeración
            _facturaEnProgreso.SubTotal = SubTotal;
            _facturaEnProgreso.TotalIVA = TotalIVA;
            _facturaEnProgreso.Total = Total;
            _facturaEnProgreso.DetallesFactura = LineasDeFactura;

            try
            {
                // Inicia la transacción
                await _unitOfWork.Facturas.AddAsync(_facturaEnProgreso);

                // Descontar stock
                foreach (var detalle in _facturaEnProgreso.DetallesFactura.Where(d => d.TipoDetalle == "Producto"))
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.IdItem);
                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad;
                        await _unitOfWork.Productos.UpdateAsync(producto);
                    }
                }

                await _unitOfWork.CompleteAsync(); // Confirma la transacción
                MessageBox.Show($"Venta #{_facturaEnProgreso.NumeroFactura} finalizada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                InicializarNuevaVenta(); // Limpia el formulario para la siguiente venta
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error crítico al finalizar la venta: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
