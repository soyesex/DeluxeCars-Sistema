using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int IdProveedor { get; set; }
        public int IdMetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaEstimadaEntrega { get; set; }
        public DateTime? FechaRecepcionReal { get; set; }
        public EstadoPedido Estado { get; set; }
        public int IdUsuario { get; set; }

        public virtual Proveedor Proveedor { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; }
        public virtual ICollection<PagoProveedorPedido> PagosAplicados { get; set; }

        [NotMapped]
        public decimal MontoTotal => DetallesPedidos?.Sum(d => d.Total) ?? 0;

        [NotMapped]
        public decimal MontoPagado => PagosAplicados?.Select(pp => pp.PagoProveedor).Sum(p => p.MontoPagado) ?? 0;

        [NotMapped]
        public decimal SaldoPendiente => MontoTotal - MontoPagado;

        [NotMapped]
        public EstadoPagoPedido EstadoPago
        {
            get
            {
                if (MontoPagado <= 0)
                    return EstadoPagoPedido.Pendiente;
                if (MontoPagado >= MontoTotal)
                    return EstadoPagoPedido.Pagado;

                return EstadoPagoPedido.PagadoParcialmente;
            }
        }
    }
}
