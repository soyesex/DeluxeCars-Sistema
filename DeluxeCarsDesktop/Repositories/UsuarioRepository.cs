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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Usuario>> GetAllWithRolAsync()
        {
            return await _context.Usuarios
                                 .Include(u => u.Rol)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public bool RecoverPassword(string usernameOrEmail)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }

        public bool ValidateTokenAndResetPassword(string correo, string token, string nuevaPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<Usuario> RegisterUser(Usuario newUser, string password)
        {
            // 1. Verificar si el email ya está en uso. (Perfecto)
            if (await _context.Usuarios.AnyAsync(u => u.Email == newUser.Email))
            {
                throw new InvalidOperationException("El correo electrónico ya está registrado.");
            }

            // 2. Crear el hash y la sal de la contraseña. (Perfecto)
            PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // 3. Asignar los valores al objeto de usuario. (Perfecto)
            newUser.PasswordHash = passwordHash;
            newUser.PasswordSalt = passwordSalt;
            newUser.Activo = true;

            // 4. Añadir el nuevo usuario al contexto de Entity Framework. (Perfecto)
            await _context.Usuarios.AddAsync(newUser); // Se usa await aquí porque AddAsync devuelve un ValueTask

            // 5. --- LÍNEA ELIMINADA ---
            // await _context.SaveChangesAsync(); // Se quita esta línea. El UnitOfWork se encargará de guardar.

            // 6. Devolver el objeto de usuario preparado. (Perfecto)
            return newUser;
        }

        public async Task<Usuario> AuthenticateUser(string email, string password)
        {
            // 1. Buscar al usuario por su email en la base de datos.
            //    Es importante usar 'SingleOrDefaultAsync' que devuelve el usuario o 'null' si no lo encuentra.
            //    Incluimos el Rol para tener esa información disponible después del login.
            var user = await _context.Usuarios
                                     .Include(u => u.Rol) // Carga la información del Rol asociado
                                     .SingleOrDefaultAsync(u => u.Email == email);

            // 2. Si el usuario no existe (user es null), la autenticación falla inmediatamente.
            if (user == null)
            {
                return null;
            }

            // 3. Si el usuario existe, ahora verificamos la contraseña.
            //    Usamos nuestro helper para comparar la contraseña ingresada con el hash y la sal guardados.
            if (!PasswordHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                // La contraseña no coincide, la autenticación falla.
                return null;
            }

            // 4. ¡Autenticación exitosa! Devolvemos el objeto de usuario completo.
            return user;
        }
        public async Task<Usuario> GetByIdAsync(int id) => await _context.Usuarios.FindAsync(id);
        public async Task<IEnumerable<Usuario>> GetAllAsync() => await _context.Usuarios.Include(u => u.Rol).AsNoTracking().ToListAsync();
        public async Task<Usuario> GetUserByEmail(string email) => await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email);
        
        public Task UpdateAsync(Usuario user)
        {
            _context.Usuarios.Update(user);
            return Task.CompletedTask;
        }

        public async Task DeactivateAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                user.Activo = false;
                _context.Usuarios.Update(user);
            }
        }

        public Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
