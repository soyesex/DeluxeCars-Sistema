using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
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
        public string DisplayName { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}
