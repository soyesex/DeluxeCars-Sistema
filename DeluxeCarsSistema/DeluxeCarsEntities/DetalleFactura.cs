using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class DetalleFactura
    {
        public int Id { get; set; }
        public int IdFactura { get; set; }
        public string TipoDetalle { get; set; }
        public int IdItem { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? Descuento { get; set; }
        public decimal? IVA { get; set; }

        // --- PROPIEDADES CORREGIDAS ---
        // Ahora son propiedades simples. El ViewModel se encargará de calcular y asignar sus valores.
        public decimal SubTotalLinea { get; set; }
        public decimal Total { get; set; }

        // Navigation Property
        public virtual Factura Factura { get; set; }
    }
}
