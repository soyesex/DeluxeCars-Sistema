using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class MovimientoInventarioRepository : GenericRepository<MovimientoInventario>, IMovimientoInventarioRepository
    {
        public MovimientoInventarioRepository(AppDbContext context) : base(context) { }
    }
}
