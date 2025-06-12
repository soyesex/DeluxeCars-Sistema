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
    public class PedidoRepository : GenericRepository<Pedido>, IPedidoRepository
    {
        private readonly AppDbContext _context;
        public PedidoRepository(AppDbContext context) : base(context)
        { }

        public Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId)
        {
            throw new NotImplementedException();
        }

        public Task<Pedido> GetPedidoWithDetailsAsync(int pedidoId)
        {
            throw new NotImplementedException();
        }
    }
}
