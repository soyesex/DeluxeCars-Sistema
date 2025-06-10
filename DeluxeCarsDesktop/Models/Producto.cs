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
        [Key]
        public int Id { get; set; }

        // Clave foránea a la tabla Categorias
        public int IdCategoria { get; set; }

        [StringLength(50)]
        public string OriginalEquipamentManufacture { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Es buena práctica especificar el tipo de SQL para decimales
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required]
        public bool Estado { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CostoUnitario { get; set; } // Se usa '?' porque la columna permite nulos

        public DateTime? FechaIngreso { get; set; } // Se usa '?' porque la columna permite nulos

        [StringLength(50)]
        public string Lote { get; set; }

        [StringLength(255)]
        public string ImagenUrl { get; set; }

        // Propiedad de navegación: Un Producto pertenece a una Categoria.
        [ForeignKey("IdCategoria")]
        public virtual Categoria Categoria { get; set; }
    }
}
