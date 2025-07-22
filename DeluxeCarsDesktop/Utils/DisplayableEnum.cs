using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Utils
{
    public class DisplayableEnum<T>
    {
        // El valor real del enum (ej: EstadoPedido.Aprobado)
        public T Value { get; set; }
        // El texto que verá el usuario (ej: "Aprobado")
        public string DisplayName { get; set; }

        public override string ToString() => DisplayName;
    }
}
