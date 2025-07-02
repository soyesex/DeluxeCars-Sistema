using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class MetodoPagoRepository : GenericRepository<MetodoPago>, IMetodoPagoRepository
    {
        private readonly AppDbContext _context;
        public MetodoPagoRepository(AppDbContext context) : base(context)
        { }
    }
}
