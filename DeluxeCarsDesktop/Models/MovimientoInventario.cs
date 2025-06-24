using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int Id { get; set; }

        // Este atributo le dice a EF que esta propiedad es la clave foránea
        // para la propiedad de navegación 'Producto' de abajo.
        [ForeignKey("Producto")]
        public int IdProducto { get; set; }

        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMovimiento { get; set; }

        public int Cantidad { get; set; }

        public decimal CostoUnitario { get; set; }

        public int? IdReferencia { get; set; }
        public string? MotivoAjuste { get; set; }

        // Propiedad de navegación
        public virtual Producto Producto { get; set; }
    }
}
