using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class TipoServicioRepository : GenericRepository<TipoServicio>, ITipoServicioRepository
    {
        private readonly AppDbContext _context;
        public TipoServicioRepository(AppDbContext context) : base(context)
        { }
    }
}
