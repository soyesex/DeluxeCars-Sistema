using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class PagoProveedorRepository : GenericRepository<PagoProveedor>, IPagoProveedorRepository
    {
        private readonly AppDbContext _context;
        public PagoProveedorRepository(AppDbContext context) : base(context)
        { } 
    }
}
