namespace Aplicacion.Application.ViewModels
{
    public class ProductoViewModel
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string? NombreCategoria { get; set; }
        public string? OrigininalEquipmentManufacture { get; set; }
        public string? Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string ImagenUrl { get; set; } // Mapear la columna ImagenUrl
    }
}
