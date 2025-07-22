using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class FacturasHistorialViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private bool _isSearching = false;

        // --- Propiedades para Filtros ---
        private string _searchText;
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); } // Lo dejamos así por ahora

        private DateTime? _fechaInicio;
        public DateTime? FechaInicio
        {
            get => _fechaInicio;
            set
            {
                if (SetPropertyAndCheck(ref _fechaInicio, value))
                {
                    // Si el valor cambió, aplicamos el filtro
                    AplicarFiltrosCommand.Execute(null);
                }
            }
        }

        private DateTime? _fechaFin;
        public DateTime? FechaFin
        {
            get => _fechaFin;
            set
            {
                if (SetPropertyAndCheck(ref _fechaFin, value))
                {
                    AplicarFiltrosCommand.Execute(null);
                }
            }
        }
        public ObservableCollection<Cliente> ClientesDisponibles { get; private set; }
        public ObservableCollection<DisplayItem<EstadoPagoFactura?>> EstadosPago { get; }

        private Cliente _clienteFiltro;
        public Cliente ClienteFiltro
        {
            get => _clienteFiltro;
            set
            {
                if (SetPropertyAndCheck(ref _clienteFiltro, value))
                {
                    AplicarFiltrosCommand.Execute(null);
                }
            }
        }
        private DisplayItem<EstadoPagoFactura?> _selectedEstadoPago;
        public DisplayItem<EstadoPagoFactura?> SelectedEstadoPago
        {
            get => _selectedEstadoPago;
            set
            {
                if (SetPropertyAndCheck(ref _selectedEstadoPago, value))
                {
                    // Cuando el item seleccionado cambia, actualizamos el valor del filtro real
                    EstadoPagoFiltro = _selectedEstadoPago?.Value;
                }
            }
        }
        private EstadoPagoFactura? _estadoPagoFiltro;
        public EstadoPagoFactura? EstadoPagoFiltro
        {
            get => _estadoPagoFiltro;
            set
            {
                if (SetPropertyAndCheck(ref _estadoPagoFiltro, value))
                {
                    AplicarFiltrosCommand.Execute(null);
                }
            }
        }

        // --- Colección de Datos y Selección ---
        private ObservableCollection<Factura> _facturas;
        public ObservableCollection<Factura> Facturas { get => _facturas; private set => SetProperty(ref _facturas, value); }

        private Factura _facturaSeleccionada;
        public Factura FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set
            {
                SetProperty(ref _facturaSeleccionada, value);
                ActualizarEstadoComandos();
            }
        }

        // --- Paginación ---
        public int NumeroDePagina { get; set; } = 1;
        public int TamañoDePagina { get; set; } = 15;
        private int _totalItems;
        public int TotalItems { get => _totalItems; private set => SetProperty(ref _totalItems, value); }
        public int TotalPaginas => (TotalItems == 0) ? 1 : (int)Math.Ceiling((double)TotalItems / TamañoDePagina);

        // --- Comandos ---
        public ICommand AplicarFiltrosCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand AnularFacturaCommand { get; }
        public ICommand RegistrarPagoCommand { get; }
        public ICommand LiquidarCreditoCommand { get; }
        public ICommand IrAPaginaSiguienteCommand { get; }
        public ICommand IrAPaginaAnteriorCommand { get; }
        public ICommand NuevaFacturaCommand { get; }
        public ICommand ExportarExcelCommand { get; }

        public event Action OnRequestNuevaFactura;

        public FacturasHistorialViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Facturas = new ObservableCollection<Factura>();
            ClientesDisponibles = new ObservableCollection<Cliente>();
            EstadosPago = new ObservableCollection<DisplayItem<EstadoPagoFactura?>>
            {
                // Aquí puedes poner el texto que prefieras para la opción 'null'
                new DisplayItem<EstadoPagoFactura?>  { Value = null, DisplayText = "Todos los Estados" },
                new DisplayItem<EstadoPagoFactura?> { Value = EstadoPagoFactura.Pendiente, DisplayText = "Pendiente" },
                new DisplayItem<EstadoPagoFactura?> { Value = EstadoPagoFactura.Abonada, DisplayText = "Abonada" },
                new DisplayItem<EstadoPagoFactura?> { Value = EstadoPagoFactura.Pagada, DisplayText = "Pagada" },
                new DisplayItem<EstadoPagoFactura?> { Value = EstadoPagoFactura.Anulada, DisplayText = "Anulada" }
            };

            SelectedEstadoPago = EstadosPago.FirstOrDefault();

            // --- Instanciación de Comandos Corregida ---
            AplicarFiltrosCommand = new ViewModelCommand(async _ => await AplicarFiltros());
            LimpiarFiltrosCommand = new ViewModelCommand(async _ => await ExecuteLimpiarFiltros());
            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetalles, CanExecuteActions);
            AnularFacturaCommand = new ViewModelCommand(async _ => await ExecuteAnularFacturaCommand(), CanExecuteActions);
            RegistrarPagoCommand = new ViewModelCommand(async _ => await ExecuteRegistrarPago(), CanExecuteRegistrarPago);
            LiquidarCreditoCommand = new ViewModelCommand(async _ => await ExecuteLiquidarCredito(), CanExecuteLiquidarCredito);
            NuevaFacturaCommand = new ViewModelCommand(_ => OnRequestNuevaFactura?.Invoke());

            IrAPaginaSiguienteCommand = new ViewModelCommand(async _ => { if (NumeroDePagina < TotalPaginas) { NumeroDePagina++; await AplicarFiltros(); } }, _ => NumeroDePagina < TotalPaginas);
            IrAPaginaAnteriorCommand = new ViewModelCommand(async _ => { if (NumeroDePagina > 1) { NumeroDePagina--; await AplicarFiltros(); } }, _ => NumeroDePagina > 1);
            ExportarExcelCommand = new ViewModelCommand(async _ => await ExecuteExportarFacturasExcelCommand());
        }

        public async Task LoadAsync()
        {
            await CargarFiltrosDisponibles();
            await AplicarFiltros();
        }

        private async Task CargarFiltrosDisponibles()
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            ClientesDisponibles.Clear();
            ClientesDisponibles.Add(new Cliente { Id = 0, Nombre = "Todos los Clientes" });
            foreach (var cliente in clientes.OrderBy(c => c.Nombre))
            {
                ClientesDisponibles.Add(cliente);
            }
            ClienteFiltro = ClientesDisponibles.FirstOrDefault();
            OnPropertyChanged(nameof(ClienteFiltro));
        }

        private void ExecuteVerDetalles(object obj)
        {
            if (FacturaSeleccionada != null)
            {
                // Ya no mostramos un MessageBox.
                // Usamos tu servicio para abrir el nuevo UserControl en la ventana genérica.
                // Asumo que tu servicio puede tomar un ID como parámetro.
                _navigationService.OpenFormWindow(FormType.FacturaDetalles, FacturaSeleccionada.Id);
            }
        }
        private async Task AplicarFiltros()
        {
            if (_isSearching) return;
            _isSearching = true;
            try
            {
                var criteria = new FacturaSearchCriteria
                {
                    SearchText = this.SearchText,
                    FechaInicio = this.FechaInicio,
                    FechaFin = this.FechaFin,
                    ClienteId = (this.ClienteFiltro?.Id > 0) ? this.ClienteFiltro.Id : null,
                    EstadoPago = this.EstadoPagoFiltro,
                    PageNumber = this.NumeroDePagina,
                    PageSize = this.TamañoDePagina
                };

                var pagedResult = await _unitOfWork.Facturas.SearchAsync(criteria);
                Facturas = new ObservableCollection<Factura>(pagedResult.Items);
                TotalItems = pagedResult.TotalCount;

                OnPropertyChanged(nameof(TotalPaginas));
                OnPropertyChanged(nameof(NumeroDePagina));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar facturas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isSearching = false;
                ActualizarEstadoComandos();
            }
        }

        private async Task ExecuteLimpiarFiltros()
        {
            SearchText = string.Empty;
            FechaInicio = null;
            FechaFin = null;
            ClienteFiltro = ClientesDisponibles.FirstOrDefault();
            EstadoPagoFiltro = null;
            NumeroDePagina = 1;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(FechaInicio));
            OnPropertyChanged(nameof(FechaFin));
            OnPropertyChanged(nameof(ClienteFiltro));
            OnPropertyChanged(nameof(EstadoPagoFiltro));

            await AplicarFiltros();
        }
        private async Task ExecuteExportarFacturasExcelCommand()
        {
            // 1. Obtenemos TODAS las facturas que coinciden con los filtros actuales, ignorando la paginación.
            var exportCriteria = new FacturaSearchCriteria
            {
                SearchText = this.SearchText,
                FechaInicio = this.FechaInicio,
                FechaFin = this.FechaFin,
                ClienteId = (this.ClienteFiltro?.Id > 0) ? this.ClienteFiltro.Id : null,
                EstadoPago = this.EstadoPagoFiltro,
                PageNumber = 1,
                PageSize = int.MaxValue // Truco para obtener todos los registros
            };

            var pagedResult = await _unitOfWork.Facturas.SearchAsync(exportCriteria);
            var facturasAExportar = pagedResult.Items;

            if (!facturasAExportar.Any())
            {
                MessageBox.Show("No hay facturas para exportar con los filtros actuales.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2. Usar EPPlus para crear el archivo Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Historial de Ventas");

                // Encabezados
                worksheet.Cells[1, 1].Value = "N° Factura";
                worksheet.Cells[1, 2].Value = "Fecha Emisión";
                worksheet.Cells[1, 3].Value = "Cliente";
                worksheet.Cells[1, 4].Value = "Total Factura";
                worksheet.Cells[1, 5].Value = "Monto Abonado";
                worksheet.Cells[1, 6].Value = "Saldo Pendiente";
                worksheet.Cells[1, 7].Value = "Estado";
                worksheet.Cells[1, 8].Value = "Observaciones";

                // Datos
                int row = 2;
                foreach (var factura in facturasAExportar)
                {
                    worksheet.Cells[row, 1].Value = factura.NumeroFactura;
                    worksheet.Cells[row, 2].Value = factura.FechaEmision;
                    worksheet.Cells[row, 3].Value = factura.Cliente.Nombre;
                    worksheet.Cells[row, 4].Value = factura.Total;
                    worksheet.Cells[row, 5].Value = factura.MontoAbonado;
                    worksheet.Cells[row, 6].Value = factura.SaldoPendiente;
                    worksheet.Cells[row, 7].Value = factura.EstadoPago.ToString();
                    worksheet.Cells[row, 8].Value = factura.Observaciones;
                    row++;
                }

                // Formato y auto-ajuste de columnas
                worksheet.Cells["A1:H1"].Style.Font.Bold = true;
                worksheet.Cells["B:B"].Style.Numberformat.Format = "dd/mm/yyyy"; // Formato de fecha
                worksheet.Cells["D:F"].Style.Numberformat.Format = "$ #,##0.00"; // Formato de moneda para varias columnas
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 3. Guardar el archivo
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"HistorialVentas_DeluxeCars_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                    Filter = "Archivos de Excel (*.xlsx)|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await package.SaveAsAsync(new FileInfo(saveFileDialog.FileName));
                        MessageBox.Show("Exportación a Excel completada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ocurrió un error al guardar el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void ActualizarEstadoComandos()
        {
            (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (AnularFacturaCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (RegistrarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (LiquidarCreditoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
        }

        // --- Lógica de los Comandos ---

        private bool CanExecuteActions(object obj) => FacturaSeleccionada != null;
        private bool CanExecuteRegistrarPago(object obj) => FacturaSeleccionada != null && FacturaSeleccionada.EstadoPago != EstadoPagoFactura.Pagada;
        private bool CanExecuteLiquidarCredito(object obj) => FacturaSeleccionada != null && FacturaSeleccionada.SaldoPendiente < 0;

        private void ExecuteVerDetallesCommand(object obj)
        {
            MessageBox.Show($"Mostrando detalles para la factura: {FacturaSeleccionada.NumeroFactura}", "Información");
        }

        private async Task ExecuteAnularFacturaCommand()
        {
            if (!CanExecuteActions(null)) return;
            await _navigationService.OpenFormWindow(FormType.NotaDeCredito, FacturaSeleccionada.Id);
            await AplicarFiltros();
        }

        private async Task ExecuteRegistrarPago()
        {
            if (!CanExecuteRegistrarPago(null)) return;
            await _navigationService.OpenFormWindow(FormType.RegistrarPagoCliente, FacturaSeleccionada.Id);
            await AplicarFiltros();
        }

        private async Task ExecuteLiquidarCredito()
        {
            if (!CanExecuteLiquidarCredito(null)) return;

            // Pedimos confirmación al usuario
            var confirmacion = MessageBox.Show(
                $"Esto registrará un reembolso por el crédito de {FacturaSeleccionada.SaldoPendiente:C} y el saldo de la factura volverá a ser $0.00.\n\n¿Desea continuar?",
                "Confirmar Liquidación de Saldo a Favor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.No) return;

            try
            {
                // Creamos una transacción POSITIVA que cancela exactamente el crédito negativo.
                var pagoReembolso = new PagoCliente
                {
                    IdCliente = FacturaSeleccionada.IdCliente,
                    IdMetodoPago = FacturaSeleccionada.IdMetodoPago, // Asumimos el mismo método
                    IdUsuario = _unitOfWork.Usuarios.GetAdminUserAsync().Result.Id, // Asignamos al admin por defecto

                    // ¡LA CLAVE! El monto es el saldo pendiente negativo, multiplicado por -1 para hacerlo positivo.
                    MontoRecibido = FacturaSeleccionada.SaldoPendiente * -1,

                    FechaPago = DateTime.Now,
                    Referencia = $"Liquidación de crédito para Factura {FacturaSeleccionada.NumeroFactura}",
                    FacturasCubiertas = new List<PagoClienteFactura>()
                };

                // Creamos la "grapa" para vincularlo a la factura original
                var enlace = new PagoClienteFactura { IdFactura = FacturaSeleccionada.Id, PagoCliente = pagoReembolso };
                pagoReembolso.FacturasCubiertas.Add(enlace);

                await _unitOfWork.PagosClientes.AddAsync(pagoReembolso);
                await _unitOfWork.CompleteAsync();

                // Refrescamos la lista para ver el resultado
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al liquidar el crédito: {ex.Message}", "Error");
            }
            finally
            {
                await AplicarFiltros();
            }
        }
    }
}