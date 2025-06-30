using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    // UserAccountModel = "Usuario simplificado para mostrar"
    /*Cuándo se usa:

        - Después del login exitoso
        - Mostrar nombre en la barra superior
        - Foto de perfil en menús
        - Información de sesión*/
    public class UserAccountModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string DisplayName => $"Bienvenido, {FirstName}";

        public byte[] ProfilePicture { get; set; }
        // --- AÑADE ESTA PROPIEDAD ---
        // Devuelve la primera letra del DisplayName en mayúsculas.
        // Si DisplayName es nulo o vacío, devuelve un "?".
        public string Initial => !string.IsNullOrEmpty(FirstName) ? FirstName.Substring(0, 1).ToUpper() : "?";
    }
}
