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
    public class PagoProveedorRepository : GenericRepository<PagoProveedor>, IPagoProveedorRepository
    {
        private readonly AppDbContext _context;
        public PagoProveedorRepository(AppDbContext context) : base(context)
        { } 
    }
}
