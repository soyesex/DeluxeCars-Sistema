using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }

        // Esta es la propiedad que contendrá el valor de la clave foránea.
        public int IdUsuario { get; set; }

        // --- INICIO DE LA CORRECCIÓN ---
        // Con esta anotación, le decimos a EF que la propiedad de navegación 'Usuario'
        // debe usar la columna especificada en 'IdUsuario' como su clave foránea.
        // Ya no creará una columna extra 'UsuarioId'.
        [ForeignKey("IdUsuario")]
        // --- FIN DE LA CORRECCIÓN ---
        public virtual Usuario Usuario { get; set; }
    }
}
