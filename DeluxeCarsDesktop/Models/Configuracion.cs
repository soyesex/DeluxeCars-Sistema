using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Configuracion
    {
        [Key]
        public int Id { get; set; } // Solo habrá una fila, con Id = 1

        // --- Información de la Empresa ---
        [StringLength(100)]
        public string NombreTienda { get; set; }

        [StringLength(255)]
        public string Direccion { get; set; }

        [StringLength(50)]
        public string Telefono { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(150)]
        public string HorarioAtencion { get; set; }

        // --- Configuración Financiera ---
        [Column(TypeName = "decimal(5, 2)")] // Permite valores como 19.00
        public decimal PorcentajeIVA { get; set; }

        // --- Seguridad ---
        // Guardamos el PIN hasheado, nunca el PIN en texto plano.
        public byte[]? AdminPINHash { get; set; }
        public byte[]? AdminPINSalt { get; set; }

        // --- Personalización ---
        public byte[]? Logo { get; set; }
        public byte[]? Banner { get; set; }
    }
}
