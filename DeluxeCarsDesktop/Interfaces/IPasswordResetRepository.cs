using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IPasswordResetRepository
    {
        // Crea un nuevo token de reseteo para un usuario.
        Task<PasswordReset> CreateResetTokenAsync(int userId);

        // Valida un token y obtiene la solicitud de reseteo asociada.
        Task<PasswordReset> GetByTokenAsync(Guid token);

        // Marca un token como utilizado para que no pueda volver a usarse.
        Task MarkAsUsedAsync(Guid token);
    }
}
