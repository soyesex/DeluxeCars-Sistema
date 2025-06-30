using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class TipoServicioRepository : GenericRepository<TipoServicio>, ITipoServicioRepository
    {
        private readonly AppDbContext _context;
        public TipoServicioRepository(AppDbContext context) : base(context)
        { }
    }
}
