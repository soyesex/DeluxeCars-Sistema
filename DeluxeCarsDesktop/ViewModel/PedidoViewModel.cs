using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Services.PdfDocuments;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
using Microsoft.Win32;
using OfficeOpenXml;
using QuestPDF.Fluent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class PedidoViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly IEmailService _emailService;

        private bool _isSearching = false;

        // --- Propiedades para el Binding y Filtros ---
        private ObservableCollection<Pedido> _pedidos;
        public ObservableCollection<Pedido> Pedidos { get => _pedidos; private set => SetProperty(ref _pedidos, value); }

        private Pedido _pedidoSeleccionado;
        public Pedido PedidoSeleccionado
        {
            get => _pedidoSeleccionado;
            set
            {
                SetProperty(ref _pedidoSeleccionado, value);
                ActualizarEstadoComandos();
                (AprobarPedidoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); // <-- Añade esta línea
            }
        }

        public ObservableCollection<Proveedor> Proveedores { get; private set; }

        private DateTime _fechaInicio;
        public DateTime FechaInicio { get => _fechaInicio; set => SetProperty(ref _fechaInicio, value); }

        private DateTime _fechaFin;
        public DateTime FechaFin { get => _fechaFin; set => SetProperty(ref _fechaFin, value); }

        public ObservableCollection<DisplayableEnum<EstadoPedido?>> EstadosFiltro { get; }
        
        private DisplayableEnum<EstadoPedido?> _estadoFiltroSeleccionado;
        public DisplayableEnum<EstadoPedido?> EstadoFiltroSeleccionado
        {
            get => _estadoFiltroSeleccionado;
            set
            {
                // Usamos SetPropertyAndCheck para evitar filtros innecesarios
                if (SetPropertyAndCheck(ref _estadoFiltroSeleccionado, value))
                {
                    // Aplicamos el filtro inmediatamente al cambiar la selección
                    _ = AplicarFiltros();
                }
            }
        }
        private Proveedor _proveedorFiltro;
        public Proveedor ProveedorFiltro
        {
            get => _proveedorFiltro;
            set
            {
                // Usamos el mismo patrón que en EstadoFiltroSeleccionado
                if (SetPropertyAndCheck(ref _proveedorFiltro, value))
                {
                    // Aplicamos el filtro inmediatamente
                    _ = AplicarFiltros();
                }
            }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        // --- Paginación ---
        private int _numeroDePagina = 1;
        public int NumeroDePagina { get => _numeroDePagina; set { if (SetPropertyAndCheck(ref _numeroDePagina, value)) _ = AplicarFiltros(); } }
        private int _tamañoDePagina = 15;
        public int TamañoDePagina { get => _tamañoDePagina; set { if (SetPropertyAndCheck(ref _tamañoDePagina, value)) _ = AplicarFiltros(); } }
        private int _totalItems;
        public int TotalItems { get => _totalItems; private set => SetProperty(ref _totalItems, value); }
        public int TotalPaginas => (TotalItems == 0) ? 1 : (int)Math.Ceiling((double)TotalItems / TamañoDePagina);

        // --- Comandos ---
        public ICommand NuevoPedidoCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand FiltrarCommand { get; }
        public ICommand RecepcionarPedidoCommand { get; }
        public ICommand GenerarPdfCommand { get; }
        public ICommand RegistrarPagoCommand { get; }
        public ICommand AplicarFiltrosCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand IrAPaginaSiguienteCommand { get; }
        public ICommand IrAPaginaAnteriorCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand AprobarPedidoCommand { get; }
        public PedidoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService, IEmailService emailService,
                           IMessengerService messengerService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _emailService = emailService;


            Pedidos = new ObservableCollection<Pedido>();
            Proveedores = new ObservableCollection<Proveedor>();

            // Valores por defecto para los filtros de fecha
            FechaFin = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-1);
            EstadosFiltro = new ObservableCollection<DisplayableEnum<EstadoPedido?>>
            {
                // El primer item "dummy" que no hace nada
                new DisplayableEnum<EstadoPedido?> { Value = null, DisplayName = "Todos los Estados" },
        
                // El resto de los valores del enum
                new DisplayableEnum<EstadoPedido?> { Value = EstadoPedido.Borrador, DisplayName = "Borrador" },
                new DisplayableEnum<EstadoPedido?> { Value = EstadoPedido.Aprobado, DisplayName = "Aprobado" },
                new DisplayableEnum<EstadoPedido?> { Value = EstadoPedido.RecibidoParcialmente, DisplayName = "Recibido Parcialmente" },
                new DisplayableEnum<EstadoPedido?> { Value = EstadoPedido.Recibido, DisplayName = "Recibido" },
                new DisplayableEnum<EstadoPedido?> { Value = EstadoPedido.Cancelado, DisplayName = "Cancelado" }
            };

            messengerService.Subscribe<PedidoGuardadoMessage>(async (message) =>
            {
                // Cuando recibamos el mensaje, simplemente recargamos los filtros.
                await AplicarFiltros();
            });

            _estadoFiltroSeleccionado = EstadosFiltro.FirstOrDefault();
            NuevoPedidoCommand = new ViewModelCommand(ExecuteNuevoPedidoCommand);
            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetallesCommand, _ => PedidoSeleccionado != null);
            FiltrarCommand = new ViewModelCommand(async p => await AplicarFiltros());
            RecepcionarPedidoCommand = new ViewModelCommand(ExecuteRecepcionarPedido, CanExecuteRecepcionarPedido);
            RegistrarPagoCommand = new ViewModelCommand(ExecuteRegistrarPago, CanExecuteRegistrarPago);
            GenerarPdfCommand = new ViewModelCommand(ExecuteGenerarPdf, CanExecuteGenerarPdf); 
            AplicarFiltrosCommand = new ViewModelCommand(async _ => await AplicarFiltros());
            LimpiarFiltrosCommand = new ViewModelCommand(async _ => await ExecuteLimpiarFiltros());
            IrAPaginaSiguienteCommand = new ViewModelCommand(_ => NumeroDePagina++, _ => NumeroDePagina < TotalPaginas);
            IrAPaginaAnteriorCommand = new ViewModelCommand(_ => NumeroDePagina--, _ => NumeroDePagina > 1);
            ExportarExcelCommand = new ViewModelCommand(async _ => await ExecuteExportarExcelCommand());
            AprobarPedidoCommand = new ViewModelCommand(async _ => await ExecuteAprobarPedidoCommand(), _ => CanAprobarPedido());
        }

        public async Task LoadAsync()
        {
            await LoadProveedoresAsync();
            await AplicarFiltros();
        }

        private async Task LoadProveedoresAsync()
        {
            try
            {
                var provs = await _unitOfWork.Proveedores.GetAllAsync();
                Proveedores.Clear();
                Proveedores.Add(new Proveedor { Id = 0, RazonSocial = "Todos los Proveedores" });
                foreach (var p in provs.OrderBy(p => p.RazonSocial))
                {
                    Proveedores.Add(p);
                }
                ProveedorFiltro = Proveedores.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los proveedores: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanAprobarPedido()
        {
            // Solo se puede aprobar si hay un pedido seleccionado y su estado es Borrador
            return PedidoSeleccionado != null && PedidoSeleccionado.Estado == EstadoPedido.Borrador;
        }

        private async Task ExecuteAprobarPedidoCommand()
        {
            if (!CanAprobarPedido()) return;

            var result = MessageBox.Show($"¿Está seguro de que desea aprobar el pedido {PedidoSeleccionado.NumeroPedido}? Se enviará una notificación al proveedor.", "Confirmar Aprobación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                // Cambiamos el estado
                PedidoSeleccionado.Estado = EstadoPedido.Aprobado;
                // Guardamos el cambio en la BD
                await _unitOfWork.CompleteAsync();

                // Ahora que está aprobado y guardado, enviamos el correo
                await _emailService.EnviarEmailPedidoCreado(PedidoSeleccionado);

                // Refrescamos la lista para que se actualice el estado visualmente
                await AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aprobar el pedido: {ex.Message}");
            }
        }
        private async Task AplicarFiltros()
        {
            if (_isSearching) return;
            _isSearching = true;
            try
            {
                var pagedResult = await _unitOfWork.Pedidos.SearchAsync(
                    SearchText,
                    FechaInicio,
                    FechaFin,
                    (ProveedorFiltro?.Id > 0) ? ProveedorFiltro.Id : null,
                    EstadoFiltroSeleccionado?.Value,
                    NumeroDePagina,
                    TamañoDePagina);

                Pedidos.Clear();
                foreach (var pedido in pagedResult.Items) { Pedidos.Add(pedido); }
                TotalItems = pagedResult.TotalCount;
            }
            catch (Exception ex) { MessageBox.Show($"Ocurrió un error al filtrar los pedidos: {ex.Message}", "Error"); }
            finally { _isSearching = false; ActualizarEstadoComandos(); }
        }
        private void ActualizarEstadoComandos()
        {
            (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (RecepcionarPedidoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (RegistrarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
        }
        private async Task ExecuteExportarExcelCommand()
        {
            // 1. Obtenemos TODOS los pedidos que coinciden con los filtros actuales
            var pagedResult = await _unitOfWork.Pedidos.SearchAsync(
                SearchText, FechaInicio, FechaFin,
                (ProveedorFiltro?.Id > 0) ? ProveedorFiltro.Id : null,
                EstadoFiltroSeleccionado?.Value,
                1, int.MaxValue); // Truco para obtener todos los registros

            var pedidosAExportar = pagedResult.Items;

            if (!pedidosAExportar.Any())
            {
                MessageBox.Show("No hay pedidos para exportar con los filtros actuales.", "Información");
                return;
            }

            // 2. Usar EPPlus para crear el archivo Excel
            ExcelPackage.License.SetNonCommercialPersonal("Deluxe Cars Desktop App");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Pedidos");

                // Encabezados
                worksheet.Cells[1, 1].Value = "N° Pedido";
                worksheet.Cells[1, 2].Value = "Fecha Emisión";
                worksheet.Cells[1, 3].Value = "Proveedor";
                worksheet.Cells[1, 4].Value = "Monto Total";
                worksheet.Cells[1, 5].Value = "Saldo Pendiente";
                worksheet.Cells[1, 6].Value = "Estado Pedido";
                worksheet.Cells[1, 7].Value = "Estado Pago";

                // Datos
                int row = 2;
                foreach (var pedido in pedidosAExportar)
                {
                    worksheet.Cells[row, 1].Value = pedido.NumeroPedido;
                    worksheet.Cells[row, 2].Value = pedido.FechaEmision;
                    worksheet.Cells[row, 3].Value = pedido.Proveedor?.RazonSocial;
                    worksheet.Cells[row, 4].Value = pedido.MontoTotal;
                    worksheet.Cells[row, 5].Value = pedido.SaldoPendiente;
                    worksheet.Cells[row, 6].Value = pedido.Estado.ToString();
                    worksheet.Cells[row, 7].Value = pedido.EstadoPago.ToString();
                    row++;
                }

                // Formato y auto-ajuste
                worksheet.Cells["A1:G1"].Style.Font.Bold = true;
                worksheet.Cells["B:B"].Style.Numberformat.Format = "dd/mm/yyyy";
                worksheet.Cells["D:E"].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 3. Guardar el archivo
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Reporte_Pedidos_{DateTime.Now:yyyyMMdd}.xlsx",
                    Filter = "Archivos de Excel (*.xlsx)|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await package.SaveAsAsync(new FileInfo(saveFileDialog.FileName));
                        MessageBox.Show("Exportación a Excel completada exitosamente.", "Éxito");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ocurrió un error al guardar el archivo: {ex.Message}", "Error");
                    }
                }
            }
        }
        private async Task ExecuteLimpiarFiltros()
        {
            _searchText = string.Empty;
            _proveedorFiltro = Proveedores.FirstOrDefault();
            _estadoFiltroSeleccionado = null;
            _fechaFin = DateTime.Now;
            _fechaInicio = DateTime.Now.AddMonths(-1);
            _numeroDePagina = 1;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(ProveedorFiltro));
            OnPropertyChanged(nameof(EstadoFiltroSeleccionado));
            OnPropertyChanged(nameof(FechaFin));
            OnPropertyChanged(nameof(FechaInicio));
            OnPropertyChanged(nameof(NumeroDePagina));

            await AplicarFiltros();
        }

        private async void ExecuteNuevoPedidoCommand(object obj)
        {
            await _navigationService.OpenFormWindow(Utils.FormType.Pedido, 0);
            // Después de cerrar el formulario, refrescamos la lista
            await AplicarFiltros();
        }

        private async void ExecuteVerDetallesCommand(object obj)
        {
            if (PedidoSeleccionado == null) return;

            await _navigationService.OpenFormWindow(FormType.Pedido, PedidoSeleccionado.Id);

            // Después de cerrar el formulario, refrescamos la lista para ver cualquier cambio
            await AplicarFiltros();
        }

        private bool CanExecuteRecepcionarPedido(object obj)
        {
            // Primero, nos aseguramos de que haya un pedido seleccionado.
            if (PedidoSeleccionado == null) return false;

            // CAMBIO: Ahora permitimos recepcionar si el estado es Aprobado O RecibidoParcialmente.
            // El botón se desactivará para los estados Recibido, Borrador o Cancelado.
            return PedidoSeleccionado.Estado == EstadoPedido.Aprobado ||
                   PedidoSeleccionado.Estado == EstadoPedido.RecibidoParcialmente;
        }

        private async void ExecuteRecepcionarPedido(object obj)
        {
            // Aquí llamaremos a un nuevo tipo de formulario que vamos a crear.
            // Necesitaremos añadir 'RecepcionPedido' a nuestro FormType enum.
            await _navigationService.OpenFormWindow(Utils.FormType.RecepcionPedido, PedidoSeleccionado.Id);

            // Al volver, refrescamos para ver el cambio de estado de "Aprobado" a "Recibido".
            await AplicarFiltros();
        }
        private bool CanExecuteRegistrarPago(object obj)
        {
            // Se puede pagar si el pedido está Aprobado o Recibido, pero no si ya está totalmente pagado.
            return PedidoSeleccionado != null &&
                   (PedidoSeleccionado.Estado == EstadoPedido.Aprobado || PedidoSeleccionado.Estado == EstadoPedido.Recibido) &&
                   PedidoSeleccionado.EstadoPago != EstadoPagoPedido.Pagado;
        }

        private async void ExecuteRegistrarPago(object obj)
        {
            // Asegúrate de añadir 'RegistrarPagoProveedor' a tu enum FormType.
            await _navigationService.OpenFormWindow(FormType.RegistrarPagoProveedor, PedidoSeleccionado.Id);
            await AplicarFiltros();
        }
        private bool CanExecuteGenerarPdf(object obj)
        {
            // Solo se puede generar PDF de un pedido que esté seleccionado Y APROBADO o RECIBIDO.
            return obj is Pedido pedido && (pedido.Estado == EstadoPedido.Aprobado || pedido.Estado == EstadoPedido.Recibido);
        }

        private async void ExecuteGenerarPdf(object obj)
        {
            if (obj is not Pedido pedidoSeleccionado) return;

            try
            {
                // 1. Volvemos a cargar el pedido con TODOS sus detalles para el PDF.
                var pedidoCompleto = await _unitOfWork.Pedidos.GetPedidoWithDetailsAsync(pedidoSeleccionado.Id);

                if (pedidoCompleto == null)
                {
                    MessageBox.Show("No se pudo encontrar el pedido para generar el PDF.", "Error");
                    return;
                }

                // 2. Creamos una instancia de nuestro documento, pasándole el pedido completo.
                var document = new PedidoCompraDocument(pedidoCompleto);

                // 3. Le preguntamos al usuario dónde guardar el archivo.
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Orden de Compra - {pedidoCompleto.NumeroPedido}.pdf", // Nombre por defecto
                    Filter = "Documento PDF (*.pdf)|*.pdf" // Filtro de archivos
                };

                // Si el usuario selecciona una ubicación y hace clic en "Guardar"
                if (saveFileDialog.ShowDialog() == true)
                {
                    // 4. Generamos el PDF y lo guardamos en la ruta seleccionada.
                    document.GeneratePdf(saveFileDialog.FileName);

                    // 5. (Opcional pero muy recomendado) Abrimos el PDF recién creado.
                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName)
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al generar el PDF: {ex.Message}", "Error");
            }
        }
    }
}
