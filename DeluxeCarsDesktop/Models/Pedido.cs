using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime PlazoEntrega { get; set; }
        public int IdProveedor { get; set; }
        public int IdMetodoPago { get; set; }
        public string Observaciones { get; set; }
        public int IdUsuario { get; set; }

        // Navigation Properties
        public virtual Proveedor Proveedor { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; }
    }
}
