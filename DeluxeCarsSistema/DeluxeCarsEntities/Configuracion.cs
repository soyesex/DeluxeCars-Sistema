using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
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

        // Guarda la fecha y hora (en UTC) hasta la cual el PIN está bloqueado.
        // Es nullable (DateTime?) porque puede que no haya ningún bloqueo activo.
        public DateTime? LockoutEndTimeUtc { get; set; }

        // Guarda el número de intentos fallidos.
        public int PinFailedAttempts { get; set; }
        [StringLength(100)]
        public string? SmtpHost { get; set; } // ej: "smtp.gmail.com"

        public int SmtpPort { get; set; } // ej: 587

        [StringLength(100)]
        public string? EmailEmisor { get; set; } // El correo que enviará los emails

        // La contraseña se guarda como un array de bytes encriptado
        public byte[]? PasswordEmailEmisor { get; set; }

        public bool EnableSsl { get; set; }
        public bool NotificacionesActivas { get; set; }
    }
}
