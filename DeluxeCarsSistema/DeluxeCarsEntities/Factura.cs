using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class Factura
    {
        public Factura()
        {
            DetallesFactura = new HashSet<DetalleFactura>();
            PagosRecibidos = new HashSet<PagoClienteFactura>();
            NotasDeCredito = new HashSet<NotaDeCredito>();
        }
        // --- Propiedades que se guardan en la Base de Datos ---
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public int IdCliente { get; set; }
        public int? IdUsuario { get; set; }
        public int IdMetodoPago { get; set; }
        public string? Observaciones { get; set; }

        // Los totales se guardan como un registro permanente de la transacción.
        public decimal SubTotal { get; set; }
        public decimal TotalIVA { get; set; }
        public decimal Total { get; set; }

        // --- Propiedades de Navegación ---
        public virtual Cliente Cliente { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual ICollection<DetalleFactura> DetallesFactura { get; set; }
        public virtual ICollection<PagoClienteFactura> PagosRecibidos { get; set; }
        public virtual ICollection<NotaDeCredito> NotasDeCredito { get; set; }

        // --- PROPIEDADES CALCULADAS (NO GUARDADAS EN DB) ---
        // Estas propiedades calculan el estado ACTUAL de la factura en tiempo real.

        [NotMapped]
        public decimal MontoAbonado => PagosRecibidos?.Select(p => p.PagoCliente).Where(pc => pc.MontoRecibido > 0).Sum(pc => pc.MontoRecibido) ?? 0;

        [NotMapped]
        public decimal MontoAcreditado => PagosRecibidos?.Select(p => p.PagoCliente).Where(pc => pc.MontoRecibido < 0).Sum(pc => pc.MontoRecibido * -1) ?? 0;

        [NotMapped]
        public decimal SaldoPendiente => Total - MontoAbonado + MontoAcreditado; // El crédito se suma al saldo a favor del cliente

        [NotMapped]
        public EstadoPagoFactura EstadoPago
        {
            get
            {
                if (Total <= 0) return EstadoPagoFactura.Pagada;
                if (SaldoPendiente <= 0.01m) return EstadoPagoFactura.Pagada;
                if (MontoAbonado > 0 || MontoAcreditado > 0) return EstadoPagoFactura.Abonada;
                return EstadoPagoFactura.Pendiente;
            }
        }
    }
}
