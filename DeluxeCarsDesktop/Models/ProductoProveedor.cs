using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class ProductoProveedor
    {
        public int Id { get; set; }
        public int IdProveedor { get; set; }
        public int IdProducto { get; set; }
        public string TipoSuministro { get; set; }
        public decimal PrecioCompra { get; set; }
        public DateTime? Fecha { get; set; } // Nullable

        // Navigation Properties
        public virtual Proveedor Proveedor { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
