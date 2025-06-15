using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Usuario CurrentUser { get; private set; }

        public bool IsAdmin => CurrentUser?.Rol?.Nombre == "Administrador";

        public void SetCurrentUser(Usuario user)
        {
            CurrentUser = user;
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
        }
    }
}
