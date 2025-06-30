using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aplicacion.Models
{
    [Table("Servicios")]
    public class Servicio
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("TipoServicio")]
        [Column("IdTipoServicio")]
        public int TipoServicioId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionEstimada { get; set; }
        public bool Estado { get; set; }
        public TipoServicio TipoServicio { get; set; }
    }
}
