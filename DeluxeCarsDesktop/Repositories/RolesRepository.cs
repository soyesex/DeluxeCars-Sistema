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
    public class RolesRepository : GenericRepository<Rol>, IRolesRepository
    {
        private readonly AppDbContext _context;

        public RolesRepository(AppDbContext context) : base(context)
        { }

    }
}
