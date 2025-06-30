using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<PasswordReset> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<Usuario> RegisterUser(Usuario newUser, string password);
        Task<Usuario> AuthenticateUser(string email, string password);
        Task<Usuario> GetUserByEmail(string email); 
        Task<bool> ChangePassword(int userId, string oldPassword, string newPassword);
        Task<IEnumerable<Usuario>> GetAllWithRolAsync();
        Task<Usuario> GetAdminUserAsync();
        Task DeactivateAsync(int id);
        Task UpdateUserPassword(int userId, string newPassword);
    }
}
