using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsShared.Dtos
{
    public class ProductoStockDto
    {
        /// Identificador único del producto. Proviene de la tabla Producto.
        public int Id { get; set; }

        /// Nombre del producto.
        public string? Nombre { get; set; }
        public string OEM { get; set; }

        /// Precio de venta al público.
        public decimal Precio { get; set; }
        public bool Estado { get; set; }
        public int? StockMinimo { get; set; }

        /// URL de la imagen del producto.
        public string? ImagenUrl { get; set; }
        public string Descripcion { get; set; }
        public string? NombreCategoria { get; set; }

        /// La cantidad de unidades disponibles en inventario.
        /// Este valor es calculado, no viene directamente de una columna de la base de datos.
        public int StockActual { get; set; }
        public string NombreProveedorPrincipal { get; set; }
    }
}
