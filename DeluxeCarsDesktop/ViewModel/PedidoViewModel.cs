using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Services.PdfDocuments;
using DeluxeCarsDesktop.Utils;
using Microsoft.Win32;
using QuestPDF.Fluent;
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

namespace DeluxeCarsDesktop.ViewModel
{
    public class PedidoViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

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
                (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (RecepcionarPedidoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (GenerarPdfCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (RegistrarPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); // --- NUEVO --- Notifica al nuevo comando
            }
        }

        public ObservableCollection<Proveedor> Proveedores { get; private set; }
        private Proveedor _proveedorFiltro;
        public Proveedor ProveedorFiltro { get => _proveedorFiltro; set => SetProperty(ref _proveedorFiltro, value); }

        private DateTime _fechaInicio;
        public DateTime FechaInicio { get => _fechaInicio; set => SetProperty(ref _fechaInicio, value); }

        private DateTime _fechaFin;
        public DateTime FechaFin { get => _fechaFin; set => SetProperty(ref _fechaFin, value); }

        public ObservableCollection<EstadoPedido> EstadosFiltro { get; }
        private EstadoPedido? _estadoFiltroSeleccionado;
        public EstadoPedido? EstadoFiltroSeleccionado { get => _estadoFiltroSeleccionado; set => SetProperty(ref _estadoFiltroSeleccionado, value); }

        // --- Comandos ---
        public ICommand NuevoPedidoCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand FiltrarCommand { get; }
        public ICommand RecepcionarPedidoCommand { get; }
        public ICommand GenerarPdfCommand { get; }
        public ICommand RegistrarPagoCommand { get; }
        public PedidoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Pedidos = new ObservableCollection<Pedido>();
            Proveedores = new ObservableCollection<Proveedor>();

            // Valores por defecto para los filtros de fecha
            FechaFin = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-1);
            EstadosFiltro = new ObservableCollection<EstadoPedido>(Enum.GetValues(typeof(EstadoPedido)).Cast<EstadoPedido>());

            NuevoPedidoCommand = new ViewModelCommand(ExecuteNuevoPedidoCommand);
            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetallesCommand, _ => PedidoSeleccionado != null);
            FiltrarCommand = new ViewModelCommand(async p => await AplicarFiltros());
            RecepcionarPedidoCommand = new ViewModelCommand(ExecuteRecepcionarPedido, CanExecuteRecepcionarPedido);
            RegistrarPagoCommand = new ViewModelCommand(ExecuteRegistrarPago, CanExecuteRegistrarPago);
            GenerarPdfCommand = new ViewModelCommand(ExecuteGenerarPdf, CanExecuteGenerarPdf);
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

        private async Task AplicarFiltros()
        {
            try
            {
                int? proveedorId = (ProveedorFiltro != null && ProveedorFiltro.Id > 0) ? ProveedorFiltro.Id : (int?)null;
                // --- LÍNEA MODIFICADA ---
                // Ahora le pasamos el 'EstadoFiltroSeleccionado' al método del repositorio.
                var pedidosFiltrados = await _unitOfWork.Pedidos.GetPedidosByCriteriaAsync(FechaInicio, FechaFin, proveedorId, EstadoFiltroSeleccionado);

                // Lógica de actualización mejorada (ya la tienes)
                Pedidos.Clear();
                foreach (var pedido in pedidosFiltrados)
                {
                    Pedidos.Add(pedido);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al filtrar los pedidos: {ex.Message}", "Error de Filtrado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
