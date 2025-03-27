using System.ComponentModel.DataAnnotations.Schema;

namespace Aplicacion.Core.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}
