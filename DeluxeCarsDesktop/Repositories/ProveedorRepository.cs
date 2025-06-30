using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(AppDbContext context) : base(context)
        { }

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

