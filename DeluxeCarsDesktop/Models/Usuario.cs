using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Usuario : ViewModelBase
    {
        private int _id;
        [Key]
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private string _nombre;
        [Required]
        [StringLength(60)]
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

        private string _telefono;
        [StringLength(20)]
        public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }

        private string _email;
        [Required]
        [StringLength(80)]
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private byte[] _passwordHash;
        [Required]
        [MaxLength(64)]
        public byte[] PasswordHash { get => _passwordHash; set => SetProperty(ref _passwordHash, value); }

        private byte[] _passwordSalt;
        [Required]
        [MaxLength(128)] // <-- CORRECCIÓN: Tu script original usa VARBINARY(128) para el Salt
        public byte[] PasswordSalt { get => _passwordSalt; set => SetProperty(ref _passwordSalt, value); }

        private int _idRol;
        [Required]
        public int IdRol { get => _idRol; set => SetProperty(ref _idRol, value); }

        private bool _activo;
        [Required]
        public bool Activo { get => _activo; set => SetProperty(ref _activo, value); }

        private byte[]? _profilePicture;
        public byte[]? ProfilePicture { get => _profilePicture; set => SetProperty(ref _profilePicture, value); }


        // --- Propiedades de Navegación ---
        // Estas generalmente pueden quedarse como auto-propiedades, ya que es raro
        // que reemplaces el objeto de rol completo, sino más bien una propiedad dentro de él.
        // Pero para consistencia, también las podemos envolver.
        private Rol _rol;
        [ForeignKey("IdRol")]
        public virtual Rol Rol { get => _rol; set => SetProperty(ref _rol, value); }

        public virtual ICollection<Pedido> Pedidos { get; set; } = new HashSet<Pedido>();
        public virtual ICollection<Factura> Facturas { get; set; } = new HashSet<Factura>();
        public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new HashSet<PasswordReset>();
    }
}
