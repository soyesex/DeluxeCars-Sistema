using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class ProductoProveedor
    {
        public int Id { get; set; }

        public int IdProveedor { get; set; }
        public int IdProducto { get; set; }

        // Columnas adicionales de la tabla de unión
        public string? TipoSuministro { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecioCompra { get; set; }

        public DateTime? Fecha { get; set; }

        // Propiedades de Navegación para que EF entienda las relaciones
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }
    }
}
