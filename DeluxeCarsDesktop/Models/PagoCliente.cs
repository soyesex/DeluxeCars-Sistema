using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class PagoCliente
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int IdMetodoPago { get; set; }
        public int IdUsuario { get; set; }
        public decimal MontoRecibido { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Referencia { get; set; }
        public string? Notas { get; set; }

        public virtual Cliente Cliente { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<PagoClienteFactura> FacturasCubiertas { get; set; }
    }
}
