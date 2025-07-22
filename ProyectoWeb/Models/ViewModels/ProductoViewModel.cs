namespace Aplicacion.Models.ViewModels
{
    public class ProductoViewModel
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; }

        // Corregido el typo
        public string OriginalEquipmentManufacture { get; set; }

        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }

        // Se elimina FechaIngreso para evitar errores de mapeo,
        // ya que no existe en la entidad Producto.
        // public DateTime FechaIngreso { get; set; } 

        public string ImagenUrl { get; set; }
    }
}
