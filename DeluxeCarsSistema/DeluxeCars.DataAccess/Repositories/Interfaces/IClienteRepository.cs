using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> SearchByNameAsync(string name);
    }
}
