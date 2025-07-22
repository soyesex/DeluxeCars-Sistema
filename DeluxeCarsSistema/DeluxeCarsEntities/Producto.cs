using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeluxeCarsEntities
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        public int IdCategoria { get; set; }

        public string OriginalEquipamentManufacture { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        public decimal Precio { get; set; }

        public string Descripcion { get; set; }

        public bool Estado { get; set; }

        public string? ImagenUrl { get; set; }
        public int? StockMinimo { get; set; }

        public int? StockMaximo { get; set; }

        public decimal? UltimoPrecioCompra { get; set; }

        [StringLength(20)]
        public string? UnidadMedida { get; set; }
        [NotMapped]
        public int StockCalculado { get; set; }

        // --- Propiedades de Navegación ---
        public virtual Categoria Categoria { get; set; }

        public virtual ICollection<DetalleNotaDeCredito> DetallesNotaDeCredito { get; set; } = new HashSet<DetalleNotaDeCredito>();
        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; } = new HashSet<DetallePedido>();
        public virtual ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new HashSet<ProductoProveedor>();
    }
}
