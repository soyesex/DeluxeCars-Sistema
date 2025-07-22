using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class PedidoRepository : GenericRepository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(AppDbContext context) : base(context)
        { }
        public async Task<Pedido> GetForPaymentProcessingAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Proveedor)           // Necesario para el nombre del proveedor.
                .Include(p => p.DetallesPedidos)     // Necesario para calcular el MontoTotal.
                .Include(p => p.PagosAplicados)      // Necesario para la colección de pagos.
                    .ThenInclude(pa => pa.PagoProveedor) // Necesario para sumar los MontosPagados.
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }
        public async Task<PagedResult<Pedido>> SearchAsync(string searchText, DateTime fechaInicio, DateTime fechaFin, int? proveedorId, EstadoPedido? estado, int pageNumber, int pageSize)
        {
            var query = _context.Pedidos.AsQueryable();

            // 1. APLICA TODOS LOS FILTROS PRIMERO
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(p => p.NumeroPedido.Contains(searchText) ||
                                         (p.Proveedor != null && p.Proveedor.RazonSocial.Contains(searchText)));
            }

            query = query.Where(p => p.FechaEmision >= fechaInicio.Date && p.FechaEmision < fechaFin.Date.AddDays(1));

            if (proveedorId.HasValue && proveedorId.Value > 0)
            {
                query = query.Where(p => p.IdProveedor == proveedorId.Value);
            }

            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado.Value);
            }

            // 2. OBTÉN EL CONTEO TOTAL DESPUÉS DE FILTRAR
            var totalCount = await query.CountAsync();

            // 3. APLICA ORDEN, INCLUDES Y PAGINACIÓN AL FINAL
            var items = await query.OrderByDescending(p => p.FechaEmision)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   // Le decimos a EF que traiga toda la data relacionada justo antes de ejecutar
                                   .Include(p => p.Proveedor)
                                   .Include(p => p.DetallesPedidos)
                                   .Include(p => p.PagosAplicados)
                                       .ThenInclude(pa => pa.PagoProveedor)
                                   .AsNoTracking()
                                   .ToListAsync();

            return new PagedResult<Pedido> { Items = items, TotalCount = totalCount };
        }
        // --- IMPLEMENTACIÓN DEL NUEVO MÉTODO ---
        public async Task<IEnumerable<Pedido>> GetPendingOrdersWithDetailsAsync(DateTime forDate)
        {
            var effectiveDate = forDate.Date; // Nos aseguramos de comparar solo con la fecha

            return await _context.Pedidos
                .Where(p => p.Estado == EstadoPedido.Aprobado && p.FechaEstimadaEntrega.Date <= effectiveDate)
                .Include(p => p.Proveedor) // ¡Aquí sí podemos incluir lo que necesitemos!
                .AsNoTracking() // Es de solo lectura, así que podemos optimizar
                .ToListAsync();
        }
        // En Repositories/PedidoRepository.cs

        public async Task<IEnumerable<Pedido>> GetPedidosByCriteriaAsync(DateTime startDate, DateTime endDate, int? proveedorId, EstadoPedido? estado)
        {
            var query = _context.Pedidos.AsQueryable();

            // --- AÑADIDO: Eager Loading para las propiedades calculadas ---
            // Le decimos a EF que traiga toda la data necesaria en una sola consulta.
            // Esto llenará las colecciones para que MontoTotal y MontoPagado funcionen.
            query = query.Include(p => p.DetallesPedidos) // Incluimos los detalles para sumar el MontoTotal
                         .Include(p => p.PagosAplicados)  // Incluimos las "grapas"
                            .ThenInclude(pa => pa.PagoProveedor) // Y de cada "grapa", traemos el "recibo" para sumar el MontoPagado
                         .Include(p => p.Proveedor) // Incluimos datos del proveedor (ya lo tenías)
                         .Include(p => p.Usuario);   // Incluimos datos del usuario (ya lo tenías)

            // --- Filtro por Fechas (tu lógica es correcta) ---
            var effectiveEndDate = endDate.Date.AddDays(1);
            query = query.Where(p => p.FechaEmision >= startDate.Date && p.FechaEmision < effectiveEndDate);

            // --- Filtro por Proveedor (tu lógica es correcta) ---
            if (proveedorId.HasValue && proveedorId.Value > 0)
            {
                query = query.Where(p => p.IdProveedor == proveedorId.Value);
            }

            // --- Filtro por Estado (tu lógica es correcta y ahora debe funcionar) ---
            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado.Value);
            }

            // Finalmente, ordenamos y ejecutamos la consulta.
            return await query.OrderByDescending(p => p.FechaEmision).AsNoTracking().ToListAsync();
        }

        public Task<IEnumerable<Pedido>> GetPedidosByProveedorAsync(int proveedorId)
        {
            // Si no la vas a usar, es mejor dejar la excepción. Si la necesitas, la implementamos.
            throw new NotImplementedException();
        }

        public async Task<Pedido> GetPedidoWithDetailsAsync(int pedidoId)
        {
            return await _context.Pedidos
                  .Include(p => p.Proveedor)
                      .ThenInclude(pr => pr.Municipio)
                          .ThenInclude(m => m.Departamento)
                  .Include(p => p.Usuario)
                  .Include(p => p.DetallesPedidos) // <-- Necesario para MontoTotal
                      .ThenInclude(d => d.Producto)

                  // --- LÍNEAS CLAVE A AÑADIR ---
                  .Include(p => p.PagosAplicados) // <-- Le decimos que traiga las "grapas"
                      .ThenInclude(pa => pa.PagoProveedor) // <-- Y de cada "grapa", el "recibo de pago"

                  .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }
    }
}
