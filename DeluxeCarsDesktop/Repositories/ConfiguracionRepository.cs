using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class ConfiguracionRepository : GenericRepository<Configuracion>, IConfiguracionRepository
    {
        private readonly AppDbContext _context;
        public ConfiguracionRepository(AppDbContext context) : base(context)
        { }
    }
}
