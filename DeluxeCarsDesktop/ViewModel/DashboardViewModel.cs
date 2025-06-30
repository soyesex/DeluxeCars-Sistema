using DeluxeCarsDesktop.Dtos;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using LiveCharts;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Windows.Media;

namespace DeluxeCarsDesktop.ViewModel
{
    public class DashboardViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades para las Tarjetas del Dashboard ---
        private int _numeroDeClientes;
        public int NumeroDeClientes { get => _numeroDeClientes; set => SetProperty(ref _numeroDeClientes, value); }

        private int _productosEnInventario;
        public int ProductosEnInventario { get => _productosEnInventario; set => SetProperty(ref _productosEnInventario, value); }

        private int _productosBajoStock;
        public int ProductosBajoStock { get => _productosBajoStock; set => SetProperty(ref _productosBajoStock, value); }

        private decimal _ventasDeHoy;
        public decimal VentasDeHoy { get => _ventasDeHoy; set => SetProperty(ref _ventasDeHoy, value); }

        // --- NUEVA PROPIEDAD ---
        private int _pedidosPendientes;
        public int PedidosPendientes { get => _pedidosPendientes; set => SetProperty(ref _pedidosPendientes, value); }

        public ISeries[] SeriesTopProductos { get; set; }
        public Axis[] XAxes { get; set; } // <-- AÑADE ESTA LÍNEA

        public DashboardViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            SeriesTopProductos = new ISeries[0];
        }

        public async Task LoadAsync()
        {
            try
            {
                NumeroDeClientes = await _unitOfWork.Clientes.CountAsync();
                ProductosEnInventario = await _unitOfWork.Productos.CountAsync();
                ProductosBajoStock = await _unitOfWork.Productos.CountLowStockProductsAsync();

                var facturasHoy = await _unitOfWork.Facturas.GetFacturasByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));
                VentasDeHoy = facturasHoy.Sum(f => f.Total);

                PedidosPendientes = await _unitOfWork.Pedidos.CountAsync(p => p.Estado == EstadoPedido.Aprobado);

                // --- LÓGICA DEFINITIVA PARA CARGAR DATOS DEL GRÁFICO ---
                var topProductosData = await _unitOfWork.Facturas.GetTopProductosVendidosAsync(DateTime.Now.AddDays(-30), DateTime.Now);

                SeriesTopProductos = new ISeries[]
                {
                    // Usaremos ColumnSeries para un gráfico de barras vertical, es más común
                    new ColumnSeries<TopProductoDto>
                    {
                        // Le pasamos la colección completa de nuestros objetos
                        Values = topProductosData.ToList(), 
                        
                        // Le decimos cómo "mapear" cada objeto al gráfico
                        Mapping = (producto, index) => new(index, producto.UnidadesVendidas),

                        Name = "Unidades Vendidas"
                    }
                };

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        // Le decimos al eje que use los nombres de los productos como etiquetas
                        Labels = topProductosData.Select(p => p.NombreProducto).ToArray(),
                        // Giramos un poco las etiquetas por si son largas para que no se solapen
                    }
                };

                OnPropertyChanged(nameof(SeriesTopProductos));
                OnPropertyChanged(nameof(XAxes));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos del dashboard: {ex.Message}");
            }
        }
    }
}
