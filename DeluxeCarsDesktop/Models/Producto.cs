using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public int IdCategoria { get; set; }
        public string OriginalEquipamentManufacture { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
        public decimal? UltimoPrecioCompra { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? Lote { get; set; }
        public string? ImagenUrl { get; set; }

        // Propiedades de navegación
        public virtual Categoria Categoria { get; set; }
        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; } = new HashSet<DetallePedido>();
        public virtual ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new HashSet<ProductoProveedor>();
    }
}
