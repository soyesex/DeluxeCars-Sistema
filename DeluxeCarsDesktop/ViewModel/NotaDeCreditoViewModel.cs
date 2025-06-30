using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class NotaDeCreditoViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService; // Necesitamos saber quién hace la devolución
        private readonly INotificationService _notificationService;

        private Factura _facturaOriginal;

        public string Titulo => $"Nota de Crédito para Factura: {_facturaOriginal?.NumeroFactura}";
        public ObservableCollection<DevolucionItemViewModel> ItemsParaDevolver { get; }

        private string _motivo;
        public string Motivo
        {
            get => _motivo;
            set { SetProperty(ref _motivo, value); (ConfirmarDevolucionCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); }
        }

        public Action CloseAction { get; set; }
        public ICommand ConfirmarDevolucionCommand { get; }

        // El constructor ahora pide más servicios
        public NotaDeCreditoViewModel(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;

            ItemsParaDevolver = new ObservableCollection<DevolucionItemViewModel>();
            ConfirmarDevolucionCommand = new ViewModelCommand(async _ => await ExecuteConfirmarDevolucion(), _ => CanExecuteConfirmarDevolucion());
        }

        public async Task LoadAsync(int facturaId)
        {
            _facturaOriginal = await _unitOfWork.Facturas.GetFacturaWithDetailsAsync(facturaId);
            if (_facturaOriginal == null)
            {
                _notificationService.ShowError("Factura no encontrada.");
                CloseAction?.Invoke();
                return;
            }

            ItemsParaDevolver.Clear();
            foreach (var detalle in _facturaOriginal.DetallesFactura.Where(d => d.TipoDetalle == "Producto"))
            {
                ItemsParaDevolver.Add(new DevolucionItemViewModel(detalle));
            }

            OnPropertyChanged(nameof(Titulo));
        }

        private bool CanExecuteConfirmarDevolucion()
        {
            // Se puede confirmar si se ha devuelto al menos 1 producto y se ha escrito un motivo.
            return ItemsParaDevolver.Any(i => i.CantidadADevolver > 0) && !string.IsNullOrWhiteSpace(Motivo);
        }

        private async Task ExecuteConfirmarDevolucion()
        {
            if (!CanExecuteConfirmarDevolucion()) return;

            try
            {
                // --- PASO 1: Preparar la Nota de Crédito ---
                var notaCredito = new NotaDeCredito
                {
                    NumeroNota = $"NC-{_facturaOriginal.NumeroFactura}", // Un número simple por ahora
                    Fecha = DateTime.Now,
                    Motivo = this.Motivo,
                    IdFacturaOriginal = _facturaOriginal.Id,
                    IdCliente = _facturaOriginal.IdCliente,
                    IdUsuario = _currentUserService.CurrentUserId.Value,
                    Detalles = new List<DetalleNotaDeCredito>()
                };

                decimal montoTotalDevolucion = 0;

                // --- PASO 2: Procesar cada item devuelto ---
                foreach (var item in ItemsParaDevolver.Where(i => i.CantidadADevolver > 0))
                {
                    // 1. Obtenemos el total de la línea original de la factura (este ya incluye IVA y descuentos).
                    decimal totalOriginalDeLaLinea = item.DetalleFacturaOriginal.Total;

                    // 2. Obtenemos la cantidad original vendida en esa línea.
                    int cantidadOriginalDeLaLinea = item.DetalleFacturaOriginal.Cantidad;

                    // 3. Calculamos el valor REAL y preciso de UNA unidad (con IVA incluido).
                    decimal valorPorUnidadConIva = totalOriginalDeLaLinea / cantidadOriginalDeLaLinea;

                    // 4. Calculamos el valor total de la devolución para esta línea.
                    decimal totalDevolucionDeLaLinea = item.CantidadADevolver * valorPorUnidadConIva;


                    // Añadimos el detalle a la nota de crédito
                    var detalleNC = new DetalleNotaDeCredito
                    {
                        IdProducto = item.DetalleFacturaOriginal.IdItem,
                        Descripcion = item.DescripcionProducto,
                        Cantidad = item.CantidadADevolver,
                        PrecioUnitario = item.PrecioVenta,
                        Total = totalDevolucionDeLaLinea,
                        ReingresaAInventario = item.ReingresaAInventario
                    };
                    notaCredito.Detalles.Add(detalleNC);
                    montoTotalDevolucion += detalleNC.Total;

                    // Si el producto debe reingresar al stock, creamos el movimiento de inventario
                    if (item.ReingresaAInventario)
                    {
                        var movimiento = new MovimientoInventario
                        {
                            IdProducto = item.DetalleFacturaOriginal.IdItem,
                            Cantidad = item.CantidadADevolver, // Positivo, es una entrada
                            TipoMovimiento = "Entrada por Devolución",
                            Fecha = DateTime.Now,
                            MotivoAjuste = $"Devolución de Factura N° {_facturaOriginal.NumeroFactura}",
                            CostoUnitario = 0 // El costo de una devolución es 0, no afecta el costo promedio.
                        };
                        await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
                    }
                }
                notaCredito.MontoTotal = montoTotalDevolucion;
                await _unitOfWork.NotasDeCredito.AddAsync(notaCredito);

                // --- PASO 3: Ajustar las Cuentas por Cobrar ---
                // Creamos un "pago negativo" para reflejar el crédito al cliente.
                var pagoNegativo = new PagoCliente
                {
                    IdCliente = _facturaOriginal.IdCliente,
                    IdMetodoPago = _facturaOriginal.IdMetodoPago, // Usamos el mismo método de pago original
                    IdUsuario = _currentUserService.CurrentUserId.Value,
                    MontoRecibido = -montoTotalDevolucion, // ¡MONTO NEGATIVO!
                    FechaPago = DateTime.Now,
                    Referencia = $"Crédito por NC {notaCredito.NumeroNota}",
                    FacturasCubiertas = new List<PagoClienteFactura>()
                };
                var enlace = new PagoClienteFactura { IdFactura = _facturaOriginal.Id, PagoCliente = pagoNegativo };
                pagoNegativo.FacturasCubiertas.Add(enlace);
                await _unitOfWork.PagosClientes.AddAsync(pagoNegativo);

                // --- PASO 4: Guardar todo en una transacción ---
                await _unitOfWork.CompleteAsync();

                _notificationService.ShowSuccess("Nota de Crédito generada e inventario actualizado.");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al procesar la devolución: {ex.Message}");
            }
        }
    }
}

