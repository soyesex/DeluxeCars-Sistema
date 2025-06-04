using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsUI.Model
{
    // UserModel = "Usuario completo para operaciones"
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        // Mapean directamente a la tabla "Usuarios"
        public string Nombre { get; set; }            // Nombre o Username
        public string Telefono { get; set; }          // Teléfono (opcional)
        public byte[] PasswordHash { get; set; }      // El hash de la contraseña
        public byte[] PasswordSalt { get; set; }      // La sal usada para el hash
        public int IdRol { get; set; }                // Llave foránea a Roles
        public bool Activo { get; set; }              // Estado de la cuenta

        // Estas propiedades NO se almacenan directamente en la BD,
        // pero las creamos para el flujo de login/registro.
        public string PasswordSinHash { get; set; }   // Para recibir la contraseña en texto plano (temporal)
    }
}
