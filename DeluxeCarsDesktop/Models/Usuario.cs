using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(80)]
        public string Email { get; set; }

        [Required]
        [MaxLength(64)] // Corresponde a VARBINARY(64)
        public byte[] PasswordHash { get; set; }

        [Required]
        [MaxLength(16)] // Corresponde a VARBINARY(16)
        public byte[] PasswordSalt { get; set; }

        [Required]
        public int IdRol { get; set; }

        [Required]
        public bool Activo { get; set; }

        // Propiedad de navegación: Un Usuario tiene un Rol.
        [ForeignKey("IdRol")]
        public virtual Roles Rol { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
        public virtual ICollection<Factura> Facturas { get; set; }
        public virtual ICollection<PasswordReset> PasswordResets { get; set; }

    }
}
