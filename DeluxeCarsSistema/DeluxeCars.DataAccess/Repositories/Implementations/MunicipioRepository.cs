using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class MunicipioRepository : GenericRepository<Municipio>, IMunicipioRepository
    {
        private readonly AppDbContext _context;
        public MunicipioRepository(AppDbContext context) : base(context)
        { }

        public async Task<IEnumerable<Municipio>> GetAllWithDepartamentosAsync()
        {
            return await _context.Municipios
                .Include(m => m.Departamento) // Incluye la entidad Departamento relacionada
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
