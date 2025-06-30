using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class PagoProveedor
    {
        /// <summary>
        /// Identificador único del registro de pago.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del proveedor que recibe el pago.
        /// </summary>
        public int IdProveedor { get; set; }

        /// <summary>
        /// ID del método de pago utilizado (Efectivo, Transferencia, etc.).
        /// </summary>
        public int IdMetodoPago { get; set; }

        /// <summary>
        /// ID del usuario del sistema que registró esta transacción de pago.
        /// </summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// La cantidad de dinero exacta que fue pagada en esta transacción.
        /// </summary>
        public decimal MontoPagado { get; set; }

        /// <summary>
        /// La fecha y hora en que se realizó el pago.
        /// </summary>
        public DateTime FechaPago { get; set; }

        /// <summary>
        /// Código de referencia de la transacción (N° de cheque, ID de transferencia, etc.).
        /// </summary>
        public string? Referencia { get; set; }

        /// <summary>
        /// Notas o comentarios adicionales sobre el pago.
        /// </summary>
        public string? Notas { get; set; }

        /// <summary>
        /// Navegación a la entidad completa del Proveedor.
        /// </summary>
        public virtual Proveedor Proveedor { get; set; }

        /// <summary>
        /// Navegación a la entidad completa del Método de Pago.
        /// </summary>
        public virtual MetodoPago MetodoPago { get; set; }

        /// <summary>
        /// Navegación a la entidad completa del Usuario que registró.
        /// </summary>
        public virtual Usuario Usuario { get; set; }

        /// <summary>
        /// Colección de los enlaces a los pedidos que este pago está cubriendo.
        /// </summary>
        public virtual ICollection<PagoProveedorPedido> PedidosCubiertos { get; set; }
    }
}
