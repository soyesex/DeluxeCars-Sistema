using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public enum TipoMetodoPago
    {
        Efectivo,
        Electronico, // Transferencias, PSE, Nequi, etc.
        Credito,     // Tarjetas de Crédito/Débito
        Otro
    }
    public class MetodoPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required]
        public bool Disponible { get; set; }

        // --- NUEVAS PROPIEDADES ---
        [Required]
        public TipoMetodoPago Tipo { get; set; }
        public decimal? ComisionPorcentaje { get; set; }
        public bool RequiereReferencia { get; set; }
        public bool AplicaParaVentas { get; set; }
        public bool AplicaParaCompras { get; set; }

        // --- Propiedades de Navegación ---
        public virtual ICollection<Factura> Facturas { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
    }
}
