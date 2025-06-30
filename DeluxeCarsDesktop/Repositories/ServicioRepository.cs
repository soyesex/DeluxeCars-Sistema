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
