using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        // public decimal SaldoPendiente { get; set; } // <-- ELIMINAMOS esta propiedad estática
        public string? Observaciones { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalIVA { get; set; }
        public decimal? Total { get; set; }

        // --- Navigation Properties ---
        public virtual Cliente Cliente { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual ICollection<DetalleFactura> DetallesFactura { get; set; }
        public virtual ICollection<FacturaElectronica> FacturasElectronicas { get; set; }

        // --- NUEVA PROPIEDAD DE NAVEGACIÓN ---
        /// <summary>
        /// Colección de los enlaces a los pagos que se han recibido para esta factura.
        /// </summary>
        public virtual ICollection<PagoClienteFactura> PagosRecibidos { get; set; }
        public virtual ICollection<NotaDeCredito> NotasDeCredito { get; set; }

        /// <summary>
        /// Calcula la suma de todos los pagos positivos (abonos) recibidos para esta factura.
        /// </summary>
        [NotMapped]
        public decimal MontoAbonado => PagosRecibidos?
            .Select(p => p.PagoCliente)
            .Where(pc => pc.MontoRecibido > 0)
            .Sum(pc => pc.MontoRecibido) ?? 0;

        /// <summary>
        /// Calcula la suma de todos los créditos (devoluciones) aplicados a esta factura.
        /// </summary>
        [NotMapped]
        public decimal MontoAcreditado => PagosRecibidos?
            .Select(p => p.PagoCliente)
            .Where(pc => pc.MontoRecibido < 0)
            .Sum(pc => pc.MontoRecibido * -1) ?? 0; // Lo volvemos positivo para la resta

        /// <summary>
        /// Calcula el saldo real pendiente de pago. Total Original - Abonos - Créditos.
        /// </summary>
        [NotMapped]
        public decimal SaldoPendiente => (Total ?? 0) - MontoAbonado - MontoAcreditado;

        /// <summary>
        /// Determina el estado de pago de la factura basado en su saldo.
        /// </summary>
        [NotMapped]
        public EstadoPagoFactura EstadoPago
        {
            get
            {
                // Usamos un margen pequeño para evitar problemas de redondeo con decimales
                if (SaldoPendiente <= 0.01m) return EstadoPagoFactura.Pagada;
                if (MontoAbonado > 0 || MontoAcreditado > 0) return EstadoPagoFactura.Abonada;
                return EstadoPagoFactura.Pendiente;
            }
        }
    }
}
