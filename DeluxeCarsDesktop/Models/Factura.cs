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
        public int? IdUsuario { get; set; }
        public int IdMetodoPago { get; set; }
        public string? Observaciones { get; set; }

        // --- PROPIEDADES ESTÁTICAS ELIMINADAS ---
        // public decimal? SubTotal { get; set; } --> Eliminada
        // public decimal? TotalIVA { get; set; } --> Eliminada
        // public decimal? Total { get; set; }    --> Eliminada

        // --- Propiedades de Navegación ---
        public virtual Cliente Cliente { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual ICollection<DetalleFactura> DetallesFactura { get; set; }
        public virtual ICollection<PagoClienteFactura> PagosRecibidos { get; set; }
        public virtual ICollection<NotaDeCredito> NotasDeCredito { get; set; }

        // --- PROPIEDADES CALCULADAS EN TIEMPO REAL ---

        // Calcula el subtotal sumando los subtotales de cada línea de detalle.
        [NotMapped]
        public decimal SubTotal => DetallesFactura?.Sum(d => d.SubtotalCalculado) ?? 0;

        // Calcula el IVA total sumando el IVA de cada línea de detalle.
        [NotMapped]
        public decimal TotalIVA => DetallesFactura?.Sum(d => d.SubtotalCalculado * (d.IVA ?? 0) / 100) ?? 0;

        // Calcula el Total sumando el Subtotal y el IVA calculados.
        [NotMapped]
        public decimal Total => SubTotal + TotalIVA;

        // Suma solo los pagos positivos (abonos).
        [NotMapped]
        public decimal MontoAbonado => PagosRecibidos?.Select(p => p.PagoCliente).Where(pc => pc.MontoRecibido > 0).Sum(pc => pc.MontoRecibido) ?? 0;

        // Suma solo los pagos negativos (créditos por devolución) y los convierte a positivo para la resta.
        [NotMapped]
        public decimal MontoAcreditado => PagosRecibidos?.Select(p => p.PagoCliente).Where(pc => pc.MontoRecibido < 0).Sum(pc => pc.MontoRecibido * -1) ?? 0;

        // La fórmula final y correcta: Total Real - Abonos - Créditos.
        [NotMapped]
        public decimal SaldoPendiente => Total - MontoAbonado - MontoAcreditado;

        // Determina el estado de pago basado en el SaldoPendiente real.
        [NotMapped]
        public EstadoPagoFactura EstadoPago
        {
            get
            {
                if (Total <= 0) return EstadoPagoFactura.Pagada;
                if (SaldoPendiente <= 0.01m) return EstadoPagoFactura.Pagada; // Margen para errores de redondeo
                if (MontoAbonado > 0 || MontoAcreditado > 0) return EstadoPagoFactura.Abonada;
                return EstadoPagoFactura.Pendiente;
            }
        }
    }
}
