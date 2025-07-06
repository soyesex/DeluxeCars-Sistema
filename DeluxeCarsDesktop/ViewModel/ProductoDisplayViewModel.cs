using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ProductoDisplayViewModel : ViewModelBase
    {
        // 1. Las propiedades ahora son independientes, con get y set.
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string OEM { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public bool Estado { get; set; }
        public string CategoriaNombre { get; set; }
        public int? StockMinimo { get; set; } // Se añade para calcular el estado
        public int StockCalculado { get; }
        public string NombreProveedorPrincipal { get; } 

        // 2. La lógica de EstadoStock ahora usa las propiedades de este ViewModel.
        public string EstadoStock
        {
            get
            {
                if (StockCalculado <= 0) return "Agotado";
                if (StockMinimo.HasValue && StockCalculado < StockMinimo.Value) return "Bajo Stock";
                return "En Stock";
            }
        }

        // 3. El constructor ahora funciona correctamente.
        public ProductoDisplayViewModel(ProductoStockDto dto)
        {
            // Mapeo directo desde el DTO a las propiedades de esta clase.
            Id = dto.Id;
            Nombre = dto.Nombre;
            OEM = dto.OEM;
            Descripcion = dto.Descripcion;       // <-- Añadir mapeo
            ImagenUrl = dto.ImagenUrl;
            CategoriaNombre = dto.NombreCategoria;
            Precio = dto.Precio;
            Estado = dto.Estado;
            StockCalculado = dto.StockActual;
            StockMinimo = dto.StockMinimo; // Se mapea la nueva propiedad
            this.NombreProveedorPrincipal = dto.NombreProveedorPrincipal; // ✅ LÍNEA AÑADIDA
        }
    }
}
