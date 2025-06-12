using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IPedidoRepository : IGenericRepository<Pedido>
    {
        // Obtiene un pedido incluyendo todos sus detalles (productos solicitados).
        Task<Pedido> GetPedidoWithDetailsAsync(int pedidoId);

        // Obtiene todos los pedidos realizados a un proveedor específico.
        Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId);
    }
}
