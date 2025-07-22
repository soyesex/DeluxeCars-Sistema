namespace Aplicacion.Application.ViewModels
{
    public class OrdenViewModel
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public DateTime FechaOrden { get; set; }
        public decimal TotalOrden { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public List<OrdenDetalleViewModel> Detalles { get; set; } = new List<OrdenDetalleViewModel>();
    }

    public class OrdenDetalleViewModel
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public string? ImagenUrl { get; set; }
    }
}
