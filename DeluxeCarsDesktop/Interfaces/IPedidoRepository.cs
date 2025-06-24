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
        Task<IEnumerable<Pedido>> GetPendingOrdersWithDetailsAsync(DateTime forDate);

        // Obtiene todos los pedidos realizados a un proveedor específico.
        Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId);
        Task<IEnumerable<Pedido>> GetPedidosByCriteriaAsync(DateTime startDate, DateTime endDate, int? proveedorId, EstadoPedido? estado);
    }
}
