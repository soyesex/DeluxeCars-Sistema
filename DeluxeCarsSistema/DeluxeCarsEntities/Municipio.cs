using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeluxeCarsEntities
{
    public class Municipio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        // Propiedad de clave foránea
        public int IdDepartamento { get; set; }

        [Required]
        public bool Estado { get; set; } // Corresponde a BIT

        // Propiedad de navegación: Un Municipio pertenece a un Departamento.
        // El atributo [ForeignKey] le dice a EF a qué propiedad de clave foránea está ligada esta navegación.
        [ForeignKey("IdDepartamento")]
        public virtual Departamento Departamento { get; set; }
        public virtual ICollection<Proveedor> Proveedores { get; set; }
    }
}