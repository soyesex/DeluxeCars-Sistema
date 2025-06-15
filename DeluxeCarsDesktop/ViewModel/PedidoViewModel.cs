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
    public class PedidoViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Propiedades para el Binding y Filtros ---
        private ObservableCollection<Pedido> _pedidos;
        public ObservableCollection<Pedido> Pedidos { get => _pedidos; private set => SetProperty(ref _pedidos, value); }

        private Pedido _pedidoSeleccionado;
        public Pedido PedidoSeleccionado { get => _pedidoSeleccionado; set { SetProperty(ref _pedidoSeleccionado, value); (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        public ObservableCollection<Proveedor> Proveedores { get; private set; }
        private Proveedor _proveedorFiltro;
        public Proveedor ProveedorFiltro { get => _proveedorFiltro; set => SetProperty(ref _proveedorFiltro, value); }

        private DateTime _fechaInicio;
        public DateTime FechaInicio { get => _fechaInicio; set => SetProperty(ref _fechaInicio, value); }

        private DateTime _fechaFin;
        public DateTime FechaFin { get => _fechaFin; set => SetProperty(ref _fechaFin, value); }

        // --- Comandos ---
        public ICommand NuevoPedidoCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand FiltrarCommand { get; }

        public PedidoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            Pedidos = new ObservableCollection<Pedido>();
            Proveedores = new ObservableCollection<Proveedor>();

            // Valores por defecto para los filtros de fecha
            FechaFin = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-1);

            NuevoPedidoCommand = new ViewModelCommand(ExecuteNuevoPedidoCommand);
            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetallesCommand, _ => PedidoSeleccionado != null);
            FiltrarCommand = new ViewModelCommand(async p => await AplicarFiltros());

            LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
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
                var pedidosFiltrados = await _unitOfWork.Pedidos.GetPedidosByCriteriaAsync(FechaInicio, FechaFin, proveedorId);
                Pedidos = new ObservableCollection<Pedido>(pedidosFiltrados);
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

        private void ExecuteVerDetallesCommand(object obj)
        {
            // Aquí abrirías una nueva ventana o UserControl para mostrar los detalles del PedidoSeleccionado
            MessageBox.Show($"Mostrando detalles del pedido N° {PedidoSeleccionado.NumeroPedido}", "Ver Detalles", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
