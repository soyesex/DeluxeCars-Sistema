using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class NotaDeCredito
    {
        public int Id { get; set; }
        public string NumeroNota { get; set; } // Ej: NC-20250001
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; }
        public decimal MontoTotal { get; set; } // El valor total de la devolución

        public int IdFacturaOriginal { get; set; } // El vínculo a la factura que anula
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }

        // Navigation Properties
        public virtual Factura FacturaOriginal { get; set; }
        public virtual Cliente Cliente { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<DetalleNotaDeCredito> Detalles { get; set; }
    }
}
