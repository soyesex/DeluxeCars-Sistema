using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsEntities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ChartModel
    {
        public string Title { get; set; }
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
    }
    public class DashboardViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Propiedades para las Tarjetas (KPIs) ---
        private decimal _ventasDeHoy;
        public decimal VentasDeHoy { get => _ventasDeHoy; set => SetProperty(ref _ventasDeHoy, value); }
        // ... (resto de tus propiedades de KPIs)
        private int _pedidosPendientes;
        public int PedidosPendientes { get => _pedidosPendientes; set => SetProperty(ref _pedidosPendientes, value); }
        private int _productosEnInventario;
        public int ProductosEnInventario { get => _productosEnInventario; set => SetProperty(ref _productosEnInventario, value); }
        private int _productosBajoStock;
        public int ProductosBajoStock { get => _productosBajoStock; set => SetProperty(ref _productosBajoStock, value); }


        // --- Propiedades para el Carrusel de Gráficos ---
        public ObservableCollection<ChartModel> Charts { get; private set; }
        private ChartModel _currentChart;
        public ChartModel CurrentChart { get => _currentChart; private set => SetProperty(ref _currentChart, value); }
        private int _currentChartIndex = 0;

        // --- Comandos para Navegación ---
        public ICommand NextChartCommand { get; }
        public ICommand PreviousChartCommand { get; }
        // Nuevos comandos para la navegación
        public ICommand NavigateToVentasCommand { get; }
        public ICommand NavigateToPedidosCommand { get; }
        public ICommand NavigateToInventarioCommand { get; }
        public ICommand NavigateToReportesCommand { get; }
        public DashboardViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            Charts = new ObservableCollection<ChartModel>();

            NextChartCommand = new ViewModelCommand(p => NavigateChart(1), p => Charts.Any());
            PreviousChartCommand = new ViewModelCommand(p => NavigateChart(-1), p => Charts.Any());
            NavigateToVentasCommand = new ViewModelCommand(
                async _ => await _navigationService.NavigateTo<FacturasHistorialViewModel>());

            NavigateToPedidosCommand = new ViewModelCommand(
                async _ => await _navigationService.NavigateTo<PedidoViewModel>());

            NavigateToInventarioCommand = new ViewModelCommand(
                async _ => await _navigationService.NavigateTo<CatalogoViewModel>());

            NavigateToReportesCommand = new ViewModelCommand(
                async _ => await _navigationService.NavigateTo<ReportesViewModel>());
        }

        private void NavigateChart(int direction)
        {
            if (!Charts.Any()) return;
            _currentChartIndex = (_currentChartIndex + direction + Charts.Count) % Charts.Count;
            CurrentChart = Charts[_currentChartIndex];
        }

        public async Task LoadAsync()
        {
            try
            {
                // Carga de datos para las tarjetas
                await LoadKpisAsync();

                // Carga y preparación de datos para los Gráficos
                Charts.Clear();
                Charts.Add(await LoadTopProductsChartAsync());
                Charts.Add(await LoadSalesTrendChartAsync());

                // --- AÑADE ESTA LÍNEA ---
                Charts.Add(await LoadProfitabilityChartAsync());

                // Establece el primer gráfico como el actual
                CurrentChart = Charts.FirstOrDefault();
                (NextChartCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (PreviousChartCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos del dashboard: {ex.Message}");
            }
        }

        private async Task<ChartModel> LoadProfitabilityChartAsync()
        {
            // Obtenemos los datos de las últimas 20 facturas para mantener el gráfico limpio
            var data = (await _unitOfWork.Facturas.GetReporteRentabilidadAsync(DateTime.Now.AddDays(-30), DateTime.Now))
                       .OrderByDescending(r => r.Fecha)
                       .Take(15)
                       .Reverse() // Lo invertimos para que en el gráfico se lea de más antiguo a más nuevo
                       .ToList();

            return new ChartModel
            {
                Title = "Rentabilidad por Venta (Últimas 15 Facturas)",
                Series = new ISeries[]
                {
            // Serie para el Total de la Venta
            new ColumnSeries<decimal>
            {
                Name = "Venta",
                Values = data.Select(r => r.TotalVenta),
                Fill = new SolidColorPaint(SKColor.Parse("#6699CC")), // Azul Cornflower
                DataLabelsPaint = new SolidColorPaint(SKColors.White)
            },
            // Serie para el Costo Total
            new ColumnSeries<decimal>
            {
                Name = "Costo",
                Values = data.Select(r => r.TotalCosto),
                Fill = new SolidColorPaint(SKColor.Parse("#FF9966")), // Naranja Pastel
                DataLabelsPaint = new SolidColorPaint(SKColors.Black)
            }
                },
                XAxes = new Axis[]
                {
            new Axis
            {
                Labels = data.Select(r => r.NumeroFactura).ToArray(),
                LabelsRotation = 45
            }
                },
                YAxes = new Axis[]
                {
            new Axis
            {
                Labeler = value => value.ToString("C0"),
                MinLimit = 0
            }
                }
            };
        }
        private async Task LoadKpisAsync()
        {
            var today = DateTime.Today;
            var facturasHoy = await _unitOfWork.Facturas.GetFacturasByDateRangeAsync(today, today.AddDays(1).AddTicks(-1));
            VentasDeHoy = facturasHoy.Sum(f => f.Total);
            PedidosPendientes = await _unitOfWork.Pedidos.CountAsync(p => p.Estado == EstadoPedido.Aprobado);
            ProductosEnInventario = await _unitOfWork.Productos.CountAllAsync();
            ProductosBajoStock = await _unitOfWork.Productos.CountLowStockProductsAsync();
        }

        // --- Métodos para generar cada gráfico ---

        private async Task<ChartModel> LoadTopProductsChartAsync()
        {
            var data = await _unitOfWork.Facturas.GetTopProductosVendidosAsync(DateTime.Now.AddDays(-30), DateTime.Now, 5);
            return new ChartModel
            {
                Title = "Top 5 Productos Vendidos (Últimos 30 días)",
                Series = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Values = data.Select(p => p.UnidadesVendidas).ToArray(),
                        Name = "Unidades",
                        Fill = new SolidColorPaint(SKColor.Parse("#6699CC")), // Azul Cornflower
                        DataLabelsPaint = new SolidColorPaint(SKColors.White)
                    }
                },
                XAxes = new Axis[] { new Axis { Labels = data.Select(p => p.NombreProducto).ToArray(), LabelsRotation = 15 } },
                YAxes = new Axis[] { new Axis { MinLimit = 0 } }
            };
        }

        private async Task<ChartModel> LoadSalesTrendChartAsync()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-29);
            var salesData = await _unitOfWork.Facturas.GetFacturasByDateRangeAsync(startDate, endDate);

            var dailySales = salesData
                .GroupBy(f => f.FechaEmision.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(f => f.Total) })
                .OrderBy(d => d.Date)
                .ToDictionary(d => d.Date, d => d.Total);

            var completeSalesData = Enumerable.Range(0, 30)
                .Select(offset => startDate.AddDays(offset).Date)
                .Select(date => new { Date = date, Total = dailySales.ContainsKey(date) ? dailySales[date] : 0m });

            return new ChartModel
            {
                Title = "Ventas (Últimos 30 días)",
                Series = new ISeries[]
                {
                    new LineSeries<decimal>
                    {
                        Values = completeSalesData.Select(d => d.Total),
                        Name = "Ventas Diarias",
                        Fill = new SolidColorPaint(SKColor.Parse("#2E4D3E55")), // Verde Oscuro con transparencia
                        Stroke = new SolidColorPaint(SKColor.Parse("#669966")) { StrokeThickness = 3 }, // Verde Forest
                        GeometryFill = new SolidColorPaint(SKColor.Parse("#669966")),
                        GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 3 }
                    }
                },
                XAxes = new Axis[] { new Axis { Labels = completeSalesData.Select(d => d.Date.ToString("dd MMM")).ToArray(), LabelsRotation = 45 } },
                YAxes = new Axis[] { new Axis { Labeler = value => value.ToString("C0"), MinLimit = 0 } }
            };
        }
    }
}
