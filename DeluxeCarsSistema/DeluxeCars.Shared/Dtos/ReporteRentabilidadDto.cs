using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsShared.Dtos
{
    public class ReporteRentabilidadDto
    {
        public string NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public decimal TotalVenta { get; set; }
        public decimal TotalCosto { get; set; }

        // Propiedad calculada para la ganancia
        public decimal GananciaBruta => TotalVenta - TotalCosto;

        // Propiedad calculada para el margen porcentual
        public decimal MargenPorcentual
        {
            get
            {
                if (TotalVenta == 0) return 0;
                return (GananciaBruta / TotalVenta); // Se formateará como porcentaje en la UI
            }
        }
    }
}
