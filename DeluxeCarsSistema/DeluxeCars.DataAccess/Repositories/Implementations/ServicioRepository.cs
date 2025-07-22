using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    internal class ServicioRepository : GenericRepository<Servicio>, IServicioRepository
    {
        private readonly AppDbContext _context;
        public ServicioRepository(AppDbContext context) : base(context)
        { }

        public async Task<IEnumerable<Servicio>> GetAllWithTipoServicioAsync()
        {
            return await _context.Servicios
                                 .Include(s => s.TipoServicio)
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
