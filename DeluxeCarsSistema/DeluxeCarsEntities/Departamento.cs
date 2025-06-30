using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class Departamento
    {
        [Key] // Atributo que define la clave primaria
        public int Id { get; set; }

        [Required] // Corresponde a NOT NULL
        [StringLength(60)] // Corresponde a NVARCHAR(60)
        public string Nombre { get; set; }

        // Propiedad de navegación: Un Departamento puede tener muchos Municipios.
        // La palabra clave 'virtual' permite la carga diferida (lazy loading) si está habilitada.
        public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
    }
}
