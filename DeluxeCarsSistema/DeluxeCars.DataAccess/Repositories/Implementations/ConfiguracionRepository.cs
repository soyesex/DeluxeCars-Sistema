using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class ConfiguracionRepository : GenericRepository<Configuracion>, IConfiguracionRepository
    {
        public ConfiguracionRepository(AppDbContext context) : base(context)
        { }

    // --- AÑADE ESTE MÉTODO ---
    public async Task<Configuracion?> GetFirstAsync()
        {
            // Devuelve la primera fila que encuentre en la tabla, o null si está vacía.
            return await _context.Configuraciones.FirstOrDefaultAsync();
        }
    }
}
