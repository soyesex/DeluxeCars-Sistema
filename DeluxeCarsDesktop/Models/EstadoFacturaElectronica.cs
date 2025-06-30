using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class EstadoFacturaElectronica
    {
        public int Id { get; set; }
        public string NombreEstado { get; set; }
        public string Descripcion { get; set; }

        // Navigation Property
        public virtual ICollection<FacturaElectronica> FacturasElectronicas { get; set; }
    }
}
