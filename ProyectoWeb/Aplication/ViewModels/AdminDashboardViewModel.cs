namespace Aplicacion.Application.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProductos { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal VentasTotales { get; set; }
        public int ProductosBajoStock { get; set; }

        // Listas para mostrar actividad reciente
        public List<OrdenViewModel> UltimasOrdenes { get; set; } = new();
        public List<ProductoViewModel> ProductosConBajoStock { get; set; } = new();
    }
}
