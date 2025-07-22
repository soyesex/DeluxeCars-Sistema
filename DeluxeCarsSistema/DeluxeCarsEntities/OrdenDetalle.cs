using DeluxeCarsEntities;

namespace DeluxeCarsEntities
{
    public class OrdenDetalle
    {
        public int Id { get; set; }
        public int OrdenId { get; set; } // FK a Ordenes
        public int ProductoId { get; set; } // FK a Productos
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedades de navegación
        public Orden Orden { get; set; }
        public Producto Producto { get; set; }
    }
}
