using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [StringLength(3)] // CC, NIT, CE, etc.
        public string TipoIdentificacion { get; set; }

        [StringLength(20)]
        public string Identificacion { get; set; }

        [StringLength(100)]
        public string Direccion { get; set; }

        public int? IdCiudad { get; set; } // Opcional, clave foránea

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(80)]
        public string Email { get; set; }

        [StringLength(20)]
        public string TipoCliente { get; set; } // "Taller", "Persona Natural"

        public DateTime FechaCreacion { get; set; }

        [Required]
        public bool Estado { get; set; }

        public virtual ICollection<Factura> Facturas { get; set; }
    }
}
