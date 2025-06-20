using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? Descuento { get; set; } // Nullable
        public decimal? IVA { get; set; } // Nullable
        public decimal Total { get; set; } // Columna calculada
        [NotMapped]
        public decimal SubtotalPreview => Cantidad * PrecioUnitario;

        // Navigation Properties
        public virtual Pedido Pedido { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
