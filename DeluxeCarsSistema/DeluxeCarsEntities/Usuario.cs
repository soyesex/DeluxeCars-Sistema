using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeluxeCarsEntities
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
        [MaxLength(64)]
        public byte[] PasswordHash { get; set; }

        [Required]
        [MaxLength(128)]
        public byte[] PasswordSalt { get; set; }

        [Required]
        public int IdRol { get; set; }

        [Required]
        public bool Activo { get; set; }

        public byte[]? ProfilePicture { get; set; }


        // --- Propiedades de Navegación ---
        [ForeignKey("IdRol")]
        public virtual Rol Rol { get; set; }

        public virtual ICollection<Pedido> Pedidos { get; set; } = new HashSet<Pedido>();
        public virtual ICollection<Factura> Facturas { get; set; } = new HashSet<Factura>();
        public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new HashSet<PasswordReset>();
    }
}
