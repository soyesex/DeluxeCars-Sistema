using DeluxeCarsEntities;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> SearchByNameAsync(string name);
    }
}
