using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models.Search
{
    public class ProductSearchCriteria
    {
        // Para el campo de búsqueda de texto universal
        public string? UniversalSearchText { get; set; }

        // Para el ComboBox de Categorías
        public int? CategoryId { get; set; }

        // Para el ComboBox de Estado de Stock
        public string? StockStatus { get; set; } // Ej: "InStock", "LowStock", "OutOfStock"
    }
}
