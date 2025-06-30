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
    public class PagoClienteRepository : GenericRepository<PagoCliente>, IPagoClienteRepository
    {
        public PagoClienteRepository(AppDbContext context) : base(context)
        {
        }
    }
}
