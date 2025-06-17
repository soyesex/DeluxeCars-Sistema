using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public int IdCliente { get; set; }
        public int? IdUsuario { get; set; } // Nullable
        public int IdMetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalIVA { get; set; }
        public decimal? Total { get; set; }

        // Navigation Properties
        public virtual Cliente Cliente { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual ICollection<DetalleFactura> DetallesFactura { get; set; }
        public virtual ICollection<FacturaElectronica> FacturasElectronicas { get; set; }
    }
}
