using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface ICurrentUserService
    {
        int? CurrentUserId { get; }
        bool IsAdmin { get; }
        void SetCurrentUser(Usuario user);
        void ClearCurrentUser();
    }
}
