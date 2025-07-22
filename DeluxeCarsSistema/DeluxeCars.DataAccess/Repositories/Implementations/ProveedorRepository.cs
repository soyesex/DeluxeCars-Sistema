using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(AppDbContext context) : base(context)
        { }
        public async Task<PagedResult<Proveedor>> SearchAsync(ProveedorSearchCriteria criteria)
        {
            var query = _context.Proveedores.AsQueryable();

            // Aplicar filtro de texto
            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                var searchTextLower = criteria.SearchText.ToLower();
                query = query.Where(p => p.RazonSocial.ToLower().Contains(searchTextLower) ||
                                         p.NIT.ToLower().Contains(searchTextLower));
            }

            // Aplicar filtro de Departamento
            if (criteria.DepartamentoId.HasValue && criteria.DepartamentoId.Value > 0)
            {
                query = query.Where(p => p.Municipio.IdDepartamento == criteria.DepartamentoId.Value);
            }

            // Aplicar filtro de Municipio
            if (criteria.MunicipioId.HasValue && criteria.MunicipioId.Value > 0)
            {
                query = query.Where(p => p.IdMunicipio == criteria.MunicipioId.Value);
            }

            // Aplicar filtro de Estado
            if (criteria.Estado.HasValue)
            {
                query = query.Where(p => p.Estado == criteria.Estado.Value);
            }

            // Contar el total de resultados ANTES de paginar
            var totalCount = await query.CountAsync();

            // Aplicar ordenamiento, paginación e includes
            var items = await query
                .Include(p => p.Municipio)
                    .ThenInclude(m => m.Departamento)
                .OrderBy(p => p.RazonSocial)
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Proveedor> { Items = items, TotalCount = totalCount };
        }
        public Task<IEnumerable<Producto>> GetSuppliedProductsAsync(int proveedorId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Proveedor>> GetAllWithLocationAsync()
        {
            // Ahora, '_context' se refiere al campo 'protected' de la clase base,
            // que SÍ fue inicializado. Ya no será null.
            return await _context.Proveedores
                                 .Include(p => p.Municipio)
                                    .ThenInclude(m => m.Departamento)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<Proveedor> GetByIdWithLocationAsync(int id)
        {
            return await _context.Proveedores
                                 .Include(p => p.Municipio)
                                    .ThenInclude(m => m.Departamento)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

