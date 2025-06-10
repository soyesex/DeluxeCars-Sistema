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
        Task<Usuario> GetUserByEmail(string email); // Añadiremos este método útil
        // Aquí podrías añadir otros métodos que necesites, como GetAll, GetById, etc.
    }
}
