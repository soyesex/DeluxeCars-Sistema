using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsShared.Dtos
{
    public class FacturaSearchCriteria
    {
        public string SearchText { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? ClienteId { get; set; }
        public EstadoPagoFactura? EstadoPago { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }
}
