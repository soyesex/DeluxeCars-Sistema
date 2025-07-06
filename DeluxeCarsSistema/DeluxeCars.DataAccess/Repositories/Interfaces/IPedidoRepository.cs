using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface IPedidoRepository : IGenericRepository<Pedido>
    {
        Task<PagedResult<Pedido>> SearchAsync(string searchText, DateTime fechaInicio, DateTime fechaFin, int? proveedorId, EstadoPedido? estado, int pageNumber, int pageSize);
        // Obtiene un pedido incluyendo todos sus detalles (productos solicitados).
        Task<Pedido> GetPedidoWithDetailsAsync(int pedidoId);
        Task<IEnumerable<Pedido>> GetPendingOrdersWithDetailsAsync(DateTime forDate);

        // Obtiene todos los pedidos realizados a un proveedor específico.
        Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId);
        Task<IEnumerable<Pedido>> GetPedidosByCriteriaAsync(DateTime startDate, DateTime endDate, int? proveedorId, EstadoPedido? estado);
    }
}
