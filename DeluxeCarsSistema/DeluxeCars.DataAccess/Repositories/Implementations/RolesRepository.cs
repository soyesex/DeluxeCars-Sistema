using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class RolesRepository : GenericRepository<Rol>, IRolesRepository
    {
        private readonly AppDbContext _context;

        public RolesRepository(AppDbContext context) : base(context)
        { }

    }
}
