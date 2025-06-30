using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class TipoDocumentoElectronico
    {
        public int Id { get; set; }
        public string CodigoDIAN { get; set; }
        public string NombreTipo { get; set; }
        public string Descripcion { get; set; }

        // Navigation Property
        public virtual ICollection<FacturaElectronica> FacturasElectronicas { get; set; }
    }
}
