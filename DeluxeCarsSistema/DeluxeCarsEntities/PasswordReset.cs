using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class PasswordReset
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Guid Token { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }

        // Navigation Property
        public virtual Usuario Usuario { get; set; }
    }
}
