using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class FacturaElectronica
    {
        public int Id { get; set; }
        public int IdFactura { get; set; }
        public int IdTipoDocumentoElectronico { get; set; }
        public string CUFE { get; set; }
        public string RutaXMLFirmado { get; set; }
        public string RutaRepresentacionGrafica { get; set; }
        public DateTime? FechaEnvioPT { get; set; }
        public DateTime? FechaRecepcionDIAN { get; set; }
        public int IdEstadoFacturaElectronica { get; set; }
        public string MensajeRespuestaPT { get; set; }
        public string MensajeRespuestaDIAN { get; set; }
        public string ObservacionesInternas { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Navigation Properties
        public virtual Factura Factura { get; set; }
        public virtual TipoDocumentoElectronico TipoDocumentoElectronico { get; set; }
        public virtual EstadoFacturaElectronica EstadoFacturaElectronica { get; set; }
    }
}
