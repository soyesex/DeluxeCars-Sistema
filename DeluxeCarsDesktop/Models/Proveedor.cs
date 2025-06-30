using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public int IdMunicipio { get; set; }
        public bool Estado { get; set; }
        public string NIT { get; set; }

        // Navigation Properties
        public virtual Municipio Municipio { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
        public virtual ICollection<ProductoProveedor> ProductoProveedores { get; set; }
    }
}
