using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public interface IUserRepository
    {
        bool AuthenticateUser(NetworkCredential credential); // Crea un usuario con su nombre de usuario y contraseña
        // Devuelve true si el usuario es válido, false en caso contrario su alcance es amplio,
        // por que valida esas credenciales contra algún otro sistema de autenticación como por ejemplo una base 
        // de datos, un servicio web o Windows Authentication.
        /*Casos que retornan FALSE:
            - Usuario no existe en la base de datos
            - Contraseña incorrecta
            - Cuenta bloqueada o suspendida
            - Credenciales expiradas
            - Dominio inválido (en caso de AD)
            - Error de conexión a la base de datos/servicio
            - Formato inválido de usuario/contraseña
            - Demasiados intentos fallidos*/
        void Add(UserModel userModel);
        void Edit(UserModel userModel);
        void Remove(int id);
        UserModel GetById(int id);
        UserModel GetByUsername(string username);
        IEnumerable<UserModel> GetByAll();
        // Nuevo método para recuperación de contraseña:

        ///     Genera un token o envía un correo de recuperación para el usuario
        ///     identificado por usernameOrEmail. Devuelve true si se pudo iniciar
        ///     el flujo de recuperación (ej. el usuario existe), false en caso contrario.
        bool RecoverPassword(string usernameOrEmail);

        // Necesitamos otro método para validar token y cambiar contraseña
        bool ValidateTokenAndResetPassword(string correo, string token, string nuevaPassword);
    }
}
