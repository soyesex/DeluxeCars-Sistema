using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> SearchByNameAsync(string name);
        Task<PagedResult<Cliente>> SearchAsync(string searchText, string tipoCliente, int? idCiudad, int pageNumber, int pageSize);
        Task<int> CountActiveAsync();
        Task<int> CountNewThisMonthAsync();
    }
}
