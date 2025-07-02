using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        private readonly AppDbContext _context;
        public ClienteRepository(AppDbContext context) : base(context)
        { }

        public Task<IEnumerable<Cliente>> SearchByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
