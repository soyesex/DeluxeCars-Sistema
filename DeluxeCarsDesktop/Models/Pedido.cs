using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }

        // La fecha y hora en que se creó y guardó la orden de compra en nuestro sistema.
        public DateTime FechaEmision { get; set; }
        public int IdProveedor { get; set; }
        public int IdMetodoPago { get; set; }
        public string? Observaciones { get; set; }

        /* La fecha en la que el PROVEEDOR se comprometió a entregar la mercancía.
         Este campo lo rellenamos nosotros basándonos en la información del proveedor.
         Es la fecha clave para nuestros recordatorios y alertas de demora.*/
        public DateTime FechaEstimadaEntrega { get; set; }

        /* La fecha en la que el PROVEEDOR se comprometió a entregar la mercancía.
         Este campo lo rellenamos nosotros basándonos en la información del proveedor.
         Es la fecha clave para nuestros recordatorios y alertas de demora.*/
        public DateTime? FechaRecepcionReal { get; set; }
        
        /* El estado actual del pedido dentro de nuestro flujo de trabajo.  
        /// (Ej: "Enviado a Proveedor", "En Demora", "Recibido Completo").*/
        [StringLength(50)]
        public EstadoPedido Estado { get; set; }
        public int IdUsuario { get; set; }

        // Navigation Properties
        public virtual Proveedor Proveedor { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; }
    }
}
