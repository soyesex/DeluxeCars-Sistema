using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class DepartamentoRepository : GenericRepository<Departamento>, IDepartamentoRepository
    {
        private readonly AppDbContext _context;
        public DepartamentoRepository(AppDbContext context) : base(context)
        { }
    }
}
