using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        // 2. El constructor solo necesita llamar al constructor base.
        // La clase base se encarga de inicializar '_context' y '_dbSet'.
        public UsuarioRepository(AppDbContext context) : base(context)
        {
        }

        // --- MÉTODOS ESPECIALIZADOS CORREGIDOS ---

        public async Task<IEnumerable<Usuario>> GetAllWithRolAsync()
        {
            // Usamos '_dbSet' que es el DbSet<Usuario> de la clase base.
            return await _dbSet
                .Include(u => u.Rol)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Usuario> GetAdminUserAsync()
        {
            return await _dbSet
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Rol.Nombre.ToUpper() == "ADMINISTRADOR");
        }

        public async Task<Usuario> GetUserByEmail(string email)
        {
            // Esta es la línea que daba error. Ahora usa '_dbSet'.
            return await _dbSet
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> AuthenticateUser(string email, string password)
        {
            // Usamos '_dbSet' aquí también.
            var user = await _dbSet
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || !PasswordHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        public async Task<Usuario> RegisterUser(Usuario newUser, string password)
        {
            // Usamos '_context' (el campo 'protected' de la clase base) para acceder a otros DbSets.
            if (await _context.Usuarios.AnyAsync(u => u.Email == newUser.Email))
            {
                throw new InvalidOperationException("El correo electrónico ya está registrado.");
            }

            PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            newUser.PasswordHash = passwordHash;
            newUser.PasswordSalt = passwordSalt;
            newUser.Activo = true;

            await _dbSet.AddAsync(newUser); // Usamos _dbSet para añadir un Usuario.
            return newUser;
        }

        public async Task DeactivateAsync(int id)
        {
            var user = await GetByIdAsync(id); // GetByIdAsync es heredado de GenericRepository
            if (user != null)
            {
                user.Activo = false;
                _dbSet.Update(user); // _dbSet.Update es equivalente a _context.Usuarios.Update
            }
        }

        public async Task UpdateUserPassword(int userId, string newPassword)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }
        }

        // ... Implementación de los otros métodos específicos de IUsuarioRepository ...
        public Task<PasswordReset> GeneratePasswordResetTokenAsync(string email)
        {
            // Estos métodos más complejos que acceden a otras tablas como 'PasswordResets'
            // deben usar el '_context' protegido que viene de la clase base.
            throw new NotImplementedException();
        }

        public Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
        //public async Task<IEnumerable<Usuario>> GetAllWithRolAsync()
        //{
        //    return await _context.Usuarios
        //                         .Include(u => u.Rol)
        //                         .AsNoTracking()
        //                         .ToListAsync();
        //}
        //public async Task<Usuario> GetAdminUserAsync()
        //{
        //    // Esta consulta busca al primer usuario cuyo rol asociado se llame "ADMIN"
        //    return await _context.Usuarios
        //        .Include(u => u.Rol) // Incluimos el Rol para poder filtrar por su nombre
        //        .FirstOrDefaultAsync(u => u.Rol.Nombre.ToUpper() == "ADMINISTRADOR");
        //}
        //// MÉTODO 1 ACTUALIZADO: Para generar el token
        //public async Task<PasswordReset> GeneratePasswordResetTokenAsync(string email)
        //{
        //    var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null)
        //    {
        //        return null; // No revelamos si el email existe
        //    }

        //    var passwordReset = new PasswordReset
        //    {
        //        UsuarioId = user.Id,
        //        // El Token y FechaCreacion se generan por defecto en la BD.
        //        // Establecemos una expiración, por ejemplo, de 1 hora.
        //        FechaExpiracion = DateTime.UtcNow.AddHours(1),
        //        Usado = false
        //    };

        //    await _context.PasswordResets.AddAsync(passwordReset);
        //    // El UnitOfWork.CompleteAsync() guardará este registro.

        //    // Devolvemos el token en formato string para que sea fácil de manejar.
        //    return passwordReset;
        //}


        //// MÉTODO 2 ACTUALIZADO: Para validar el token y cambiar la contraseña
        //public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        //{
        //    // Intentamos convertir el string del token a Guid
        //    if (!Guid.TryParse(token, out Guid tokenGuid))
        //    {
        //        return false; // El token no tiene el formato correcto.
        //    }

        //    // Buscamos al usuario para obtener su ID
        //    var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null) return false;

        //    // Buscamos un token válido para ese usuario
        //    var passwordResetRequest = await _context.PasswordResets
        //        .FirstOrDefaultAsync(pr => pr.UsuarioId == user.Id
        //                               && pr.Token == tokenGuid
        //                               && pr.FechaExpiracion > DateTime.UtcNow
        //                               && !pr.Usado);

        //    if (passwordResetRequest == null)
        //    {
        //        return false; // No se encontró un token válido.
        //    }

        //    // Creamos el nuevo hash y salt
        //    PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

        //    // Actualizamos el usuario
        //    user.PasswordHash = passwordHash;
        //    user.PasswordSalt = passwordSalt;

        //    // Marcamos el token como usado para que no se pueda volver a utilizar.
        //    passwordResetRequest.Usado = true;

        //    // El UnitOfWork.CompleteAsync() guardará ambos cambios (usuario y token).
        //    return true;
        //}

        //public async Task UpdateUserPassword(int userId, string newPassword)
        //{
        //    // 1. Buscamos al usuario en la base de datos por su ID.
        //    var user = await _context.Usuarios.FindAsync(userId);

        //    if (user != null)
        //    {
        //        // 2. Si el usuario existe, usamos tu PasswordHelper para crear el nuevo hash y salt.
        //        PasswordHelper.CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

        //        // 3. Actualizamos las propiedades del objeto 'user' que ya está siendo
        //        //    rastreado por el DbContext.
        //        user.PasswordHash = passwordHash;
        //        user.PasswordSalt = passwordSalt;

        //        // No es necesario llamar a _context.Update(user) aquí.
        //        // Con solo modificar el objeto, Entity Framework ya sabe que ha cambiado.
        //        // El UnitOfWork.CompleteAsync() se encargará de guardar este cambio.
        //    }
        //    // Si el usuario no se encuentra, el método simplemente no hace nada.
        //}

        //public async Task<Usuario> RegisterUser(Usuario newUser, string password)
        //{
        //    // 1. Verificar si el email ya está en uso. (Perfecto)
        //    if (await _context.Usuarios.AnyAsync(u => u.Email == newUser.Email))
        //    {
        //        throw new InvalidOperationException("El correo electrónico ya está registrado.");
        //    }

        //    // 2. Crear el hash y la sal de la contraseña. (Perfecto)
        //    PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        //    // 3. Asignar los valores al objeto de usuario. (Perfecto)
        //    newUser.PasswordHash = passwordHash;
        //    newUser.PasswordSalt = passwordSalt;
        //    newUser.Activo = true;

        //    // 4. Añadir el nuevo usuario al contexto de Entity Framework. (Perfecto)
        //    await _context.Usuarios.AddAsync(newUser); // Se usa await aquí porque AddAsync devuelve un ValueTask

        //    // 5. --- LÍNEA ELIMINADA ---
        //    // await _context.SaveChangesAsync(); // Se quita esta línea. El UnitOfWork se encargará de guardar.

        //    // 6. Devolver el objeto de usuario preparado. (Perfecto)
        //    return newUser;
        //}

        //public async Task<Usuario> AuthenticateUser(string email, string password)
        //{
        //    // 1. Buscar al usuario por su email en la base de datos.
        //    //    Es importante usar 'SingleOrDefaultAsync' que devuelve el usuario o 'null' si no lo encuentra.
        //    //    Incluimos el Rol para tener esa información disponible después del login.
        //    var user = await _context.Usuarios
        //                             .Include(u => u.Rol) // Carga la información del Rol asociado
        //                             .SingleOrDefaultAsync(u => u.Email == email);

        //    // 2. Si el usuario no existe (user es null), la autenticación falla inmediatamente.
        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    // 3. Si el usuario existe, ahora verificamos la contraseña.
        //    //    Usamos nuestro helper para comparar la contraseña ingresada con el hash y la sal guardados.
        //    if (!PasswordHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        //    {
        //        // La contraseña no coincide, la autenticación falla.
        //        return null;
        //    }

        //    // 4. ¡Autenticación exitosa! Devolvemos el objeto de usuario completo.
        //    return user;
        //}
        //public async Task<Usuario> GetUserByEmail(string email) => await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email);

        //public async Task DeactivateAsync(int id)
        //{
        //    var user = await GetByIdAsync(id);
        //    if (user != null)
        //    {
        //        user.Activo = false;
        //        _context.Usuarios.Update(user);
        //    }
        //}

        //public Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
