using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aplicacion.Core.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Categoria")]
        [Column("IdCategoria")]
        public int CategoriaId { get; set; }
        [Column("OriginalEquipmentManufacture")]
        public string OrigninalEquipmentManufacture { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
        public decimal? CostoUnitario { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Lote { get; set; }
        public Categoria Categoria { get; set; }
        public string ImagenUrl { get; set; }
    }
}
