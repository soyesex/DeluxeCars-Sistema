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
    public class NotaDeCreditoRepository : GenericRepository<NotaDeCredito>, INotaDeCreditoRepository
    {
        public NotaDeCreditoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
