using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    // Pequeña clase para representar los datos de cada tarjeta de reporte
    public class ReportCard
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public FontAwesome.Sharp.IconChar Icon { get; set; }
    }
    public class ReportesViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades para la Vista ---
        public ObservableCollection<ReportCard> AvailableReports { get; }

        private ReportCard _selectedReport;
        public ReportCard SelectedReport
        {
            get => _selectedReport;
            set
            {
                SetProperty(ref _selectedReport, value);
                // --- APLICAMOS LA CORRECIÓN AQUÍ ---
                (GenerarReporteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public DateTime FechaInicio { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime FechaFin { get; set; } = DateTime.Now;

        private IEnumerable<object> _reportResults;
        public IEnumerable<object> ReportResults { get => _reportResults; set => SetProperty(ref _reportResults, value); }

        public ICommand GenerarReporteCommand { get; }


        public ReportesViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            // Llenamos la lista de tarjetas que se mostrarán en la UI
            AvailableReports = new ObservableCollection<ReportCard>
            {
                new ReportCard { Title = "Ventas por Periodo", Description = "Consulte el historial de facturas emitidas entre dos fechas.", Icon = FontAwesome.Sharp.IconChar.FileInvoiceDollar },
                new ReportCard { Title = "Productos Más Vendidos", Description = "Ranking de productos según la cantidad vendida en un período.", Icon = FontAwesome.Sharp.IconChar.ChartSimple },
                new ReportCard { Title = "Inventario Crítico", Description = "Muestra los productos con stock por debajo de un umbral.", Icon = FontAwesome.Sharp.IconChar.ExclamationTriangle },
                new ReportCard { Title = "Compras a Proveedores", Description = "Historial de pedidos de compra realizados a los proveedores.", Icon = FontAwesome.Sharp.IconChar.ShoppingCart }
            };

            SelectedReport = AvailableReports.FirstOrDefault();

            GenerarReporteCommand = new ViewModelCommand(async p => await ExecuteGenerarReporte(), p => CanExecuteGenerateReport());
        }
        // Añadimos un método CanExecute explícito para mayor claridad
        private bool CanExecuteGenerateReport()
        {
            return SelectedReport != null;
        }

        private async Task ExecuteGenerarReporte()
        {
            if (SelectedReport == null) return;

            // Limpiamos los resultados anteriores
            ReportResults = null;

            switch (SelectedReport.Title)
            {
                case "Ventas por Periodo":
                    ReportResults = await _unitOfWork.Facturas.GetFacturasByDateRangeAsync(FechaInicio, FechaFin);
                    break;
                case "Inventario Crítico":
                    ReportResults = await _unitOfWork.Productos.GetLowStockProductsAsync(); 
                    break;
                // ... Aquí irían las llamadas a los otros métodos de reporte cuando los implementes
                default:
                    System.Windows.MessageBox.Show("Este reporte no está implementado todavía.", "Información");
                    break;
            }
        }
    }
}
