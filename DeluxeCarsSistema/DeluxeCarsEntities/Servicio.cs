using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class Servicio
    {
        public int Id { get; set; }
        public int IdTipoServicio { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int? DuracionEstimada { get; set; } // Nullable
        public bool Estado { get; set; }

        // Navigation Property
        public virtual TipoServicio TipoServicio { get; set; }
    }
}
