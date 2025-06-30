using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class DetalleNotaDeCredito
    {
        public int Id { get; set; }
        public int IdNotaDeCredito { get; set; }
        public int IdProducto { get; set; }

        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; } // El precio al que se vendió originalmente
        public decimal Total { get; set; }

        /// <summary>
        /// Indica si este producto devuelto debe reingresar al inventario.
        /// </summary>
        public bool ReingresaAInventario { get; set; }

        // Navigation Properties
        public virtual NotaDeCredito NotaDeCredito { get; set; }
        public virtual Producto Producto { get; set; }

    }
}
