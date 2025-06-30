using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public int? CurrentUserId { get; private set; }
        private string? _userRole;

        public bool IsAdmin => _userRole == "Administrador";

        public void SetCurrentUser(Usuario user)
        {
            CurrentUserId = user.Id;
            _userRole = user.Rol?.Nombre; // Guardamos el nombre del rol
        }

        public void ClearCurrentUser()
        {
            CurrentUserId = null;
            _userRole = null;
        }
    }
}
