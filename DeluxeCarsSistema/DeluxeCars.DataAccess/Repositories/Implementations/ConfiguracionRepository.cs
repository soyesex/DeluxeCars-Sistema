using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class ConfiguracionRepository : GenericRepository<Configuracion>, IConfiguracionRepository
    {
        private readonly AppDbContext _context;
        public ConfiguracionRepository(AppDbContext context) : base(context)
        { }
    }
}
