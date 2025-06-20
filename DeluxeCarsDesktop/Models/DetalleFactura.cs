using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class DetalleFactura
    {
        public int Id { get; set; }
        public int IdFactura { get; set; }
        public string TipoDetalle { get; set; } // "Producto" o "Servicio"
        public int IdItem { get; set; } // Corresponde a Producto.Id o Servicio.Id
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? Descuento { get; set; } // Nullable
        public decimal? IVA { get; set; } // Nullable
        [NotMapped]
        public decimal SubTotalLinea { get; set; } // Columna calculada
        public decimal Total { get; set; } // Columna calculada

        // Navigation Property
        public virtual Factura Factura { get; set; }
    }
}
