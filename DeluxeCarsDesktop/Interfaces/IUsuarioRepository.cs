using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> RegisterUser(Usuario newUser, string password);
        Task<Usuario> AuthenticateUser(string email, string password);
        Task<Usuario> GetUserByEmail(string email); 
        Task<bool> ChangePassword(int userId, string oldPassword, string newPassword);

        // Métodos CRUD para la gestión de usuarios
        Task<Usuario> GetByIdAsync(int id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task UpdateAsync(Usuario user);
        Task DeactivateAsync(int id);
    }
}
