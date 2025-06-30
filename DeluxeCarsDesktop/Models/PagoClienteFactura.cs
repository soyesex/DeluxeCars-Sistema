using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class PagoClienteFactura
    {
        public int IdPagoCliente { get; set; }
        public int IdFactura { get; set; }

        public virtual PagoCliente PagoCliente { get; set; }
        public virtual Factura Factura { get; set; }
    }
}
