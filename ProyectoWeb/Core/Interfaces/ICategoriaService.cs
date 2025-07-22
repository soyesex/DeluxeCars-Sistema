using Aplicacion.Application.ViewModels;

namespace Aplicacion.Core.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaViewModel>> GetAllAsync();
        Task<CategoriaViewModel?> GetByIdAsync(int id);
        Task CreateAsync(CategoriaViewModel model);
        Task UpdateAsync(CategoriaViewModel model);
        Task DeleteAsync(int id);
    }
}
