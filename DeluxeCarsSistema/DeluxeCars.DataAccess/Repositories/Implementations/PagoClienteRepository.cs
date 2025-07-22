using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class PagoClienteRepository : GenericRepository<PagoCliente>, IPagoClienteRepository
    {
        public PagoClienteRepository(AppDbContext context) : base(context)
        {
        }
    }
}
