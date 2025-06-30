using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
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
