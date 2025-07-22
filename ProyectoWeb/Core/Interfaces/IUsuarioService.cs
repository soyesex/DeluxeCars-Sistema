using Aplicacion.Application.ViewModels;

namespace Aplicacion.Core.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<UsuarioViewModel>> GetAllUsersWithRolesAsync();
    }
}
