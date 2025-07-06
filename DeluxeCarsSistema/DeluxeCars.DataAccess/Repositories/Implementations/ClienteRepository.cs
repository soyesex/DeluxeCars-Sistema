using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        private readonly AppDbContext _context;
        public ClienteRepository(AppDbContext context) : base(context)
        { }
        public async Task<int> CountActiveAsync()
        {
            return await _dbSet.CountAsync(c => c.Estado == true);
        }

        public async Task<int> CountNewThisMonthAsync()
        {
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);

            return await _dbSet.CountAsync(c => c.FechaCreacion >= primerDiaDelMes);
        }
        public async Task<PagedResult<Cliente>> SearchAsync(string searchText, string tipoCliente, int? idCiudad, int pageNumber, int pageSize)
        {
            var query = _dbSet.AsQueryable();

            // Filtro por texto (sin cambios)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var term = searchText.ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(term) ||
                                         c.Email.ToLower().Contains(term) ||
                                         c.Telefono.Contains(term));
            }

            // --- LÓGICA DE FILTRO AÑADIDA ---
            if (!string.IsNullOrEmpty(tipoCliente) && tipoCliente != "Todos")
            {
                query = query.Where(c => c.TipoCliente == tipoCliente);
            }

            if (idCiudad.HasValue && idCiudad.Value != 0)
            {
                query = query.Where(c => c.IdCiudad == idCiudad.Value);
            }
            // --- FIN DE LA LÓGICA AÑADIDA ---

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(c => c.Nombre)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<Cliente> { Items = items, TotalCount = totalCount };
        }
        public Task<IEnumerable<Cliente>> SearchByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
