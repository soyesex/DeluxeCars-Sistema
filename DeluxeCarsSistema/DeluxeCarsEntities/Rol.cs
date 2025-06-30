using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }

        // Propiedad de navegación: Un Rol puede estar asignado a muchos Usuarios.
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
