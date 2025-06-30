using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class PagoProveedorPedido
    {
        /// <summary>
        /// FK al registro de pago (`PagoProveedor`).
        /// </summary>
        public int IdPagoProveedor { get; set; }

        /// <summary>
        /// FK al pedido (`Pedido`) que está siendo pagado.
        /// </summary>
        public int IdPedido { get; set; }

        /// <summary>
        /// Navegación a la entidad completa del pago.
        /// </summary>
        public virtual PagoProveedor PagoProveedor { get; set; }

        /// <summary>
        /// Navegación a la entidad completa del pedido.
        /// </summary>
        public virtual Pedido Pedido { get; set; }
    }
}
