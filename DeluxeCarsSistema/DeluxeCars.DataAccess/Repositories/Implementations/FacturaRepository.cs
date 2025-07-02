using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class FacturaRepository : GenericRepository<Factura>, IFacturaRepository
    {
        public FacturaRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<TopProductoDto>> GetTopProductosVendidosAsync(DateTime startDate, DateTime endDate, int topN = 5)
        {
            var effectiveEndDate = endDate.Date.AddDays(1);

            // La consulta para calcular los productos más vendidos
            var topProductos = await _context.DetallesFactura
                // 1. Filtramos solo por detalles de tipo "Producto"
                .Where(d => d.TipoDetalle == "Producto")
                // 2. Filtramos por el rango de fechas, accediendo a la Factura relacionada
                .Where(d => d.Factura.FechaEmision >= startDate.Date && d.Factura.FechaEmision < effectiveEndDate)
                // 3. Agrupamos todos los detalles por el ID del producto
                .GroupBy(d => d.IdItem)
                // 4. Proyectamos el resultado en un objeto anónimo
                .Select(g => new
                {
                    ProductoId = g.Key,
                    TotalUnidades = g.Sum(d => d.Cantidad) // Sumamos las cantidades vendidas de cada producto
                })
                // 5. Ordenamos de mayor a menor cantidad vendida
                .OrderByDescending(x => x.TotalUnidades)
                // 6. Tomamos solo los primeros 'N' resultados (por defecto 5)
                .Take(topN)
                .ToListAsync();

            // 7. Ahora, unimos estos resultados con la tabla de Productos para obtener los nombres
            var resultadoFinal = from tp in topProductos
                                 join p in _context.Productos on tp.ProductoId equals p.Id
                                 select new TopProductoDto
                                 {
                                     NombreProducto = p.Nombre,
                                     UnidadesVendidas = tp.TotalUnidades
                                 };

            return resultadoFinal;
        }

        public async Task<IEnumerable<ReporteRentabilidadDto>> GetReporteRentabilidadAsync(DateTime startDate, DateTime endDate)
        {
            var effectiveEndDate = endDate.Date.AddDays(1);

            // 1. Obtenemos las facturas del rango de fechas con su cliente.
            var facturas = await _context.Facturas
                .Where(f => f.FechaEmision >= startDate.Date && f.FechaEmision < effectiveEndDate)
                .Include(f => f.Cliente)
                .ToListAsync();

            // 2. Obtenemos los IDs de los detalles de esas facturas que son de tipo "Producto".
            var idsDetallesFactura = await _context.DetallesFactura
                .Where(df => facturas.Select(f => f.Id).Contains(df.IdFactura) && df.TipoDetalle == "Producto")
                .Select(df => df.Id)
                .ToListAsync();

            // 3. Calculamos el costo total agrupando los movimientos por el ID de referencia (que es el ID del detalle).
            var costosPorDetalle = await _context.MovimientosInventario
                .Where(mi => mi.IdReferencia.HasValue && idsDetallesFactura.Contains(mi.IdReferencia.Value))
                .GroupBy(mi => mi.IdReferencia.Value)
                .Select(g => new {
                    IdDetalleFactura = g.Key,
                    CostoTotal = g.Sum(mi => mi.CostoUnitario * (mi.Cantidad * -1)) // Cantidad es negativa, la volvemos positiva
                })
                .ToDictionaryAsync(x => x.IdDetalleFactura, x => x.CostoTotal);

            // 4. Finalmente, construimos el DTO
            var reporte = from f in facturas
                          join df in _context.DetallesFactura on f.Id equals df.IdFactura into detalles
                          select new ReporteRentabilidadDto
                          {
                              NumeroFactura = f.NumeroFactura,
                              Fecha = f.FechaEmision,
                              Cliente = f.Cliente.Nombre,
                              TotalVenta = detalles.Sum(d => d.Total),
                              TotalCosto = detalles.Sum(df => costosPorDetalle.ContainsKey(df.Id) ? costosPorDetalle[df.Id] : 0)
                          };

            // Agrupamos por factura para tener un solo registro por factura, en caso de múltiples cálculos.
            return reporte.GroupBy(r => r.NumeroFactura)
                          .Select(g => g.First())
                          .OrderByDescending(r => r.Fecha)
                          .ToList();
        }
        public async Task<IEnumerable<Factura>> GetAllWithClienteYMetodoPagoAsync()
        {
            return await _context.Facturas
                           .Include(f => f.Cliente)
                           .Include(f => f.MetodoPago)

                           // --- LÍNEA CLAVE 1: Cargar los detalles para calcular el Total ---
                           .Include(f => f.DetallesFactura)

                           // --- LÍNEA CLAVE 2: Cargar los pagos para calcular el Saldo ---
                           .Include(f => f.PagosRecibidos)
                               .ThenInclude(pr => pr.PagoCliente) // Incluimos el pago real a través de la "grapa"

                           .AsNoTracking()
                           .OrderByDescending(f => f.FechaEmision)
                           .ToListAsync();
        }
        public async Task<Factura> GetFacturaWithDetailsAsync(int facturaId)
        {
            return await _dbSet
                         .Include(f => f.Cliente)
                         .Include(f => f.Usuario)
                         .Include(f => f.DetallesFactura)

                         // --- AÑADE ESTAS LÍNEAS PARA CARGAR LOS PAGOS RECIBIDOS ---
                         .Include(f => f.PagosRecibidos)
                             .ThenInclude(pr => pr.PagoCliente)
                         // -----------------------------------------------------------

                         .FirstOrDefaultAsync(f => f.Id == facturaId);
        }

        public async Task<IEnumerable<Factura>> GetFacturasByClienteAsync(int clienteId)
        {
            return await _dbSet
                .Where(f => f.IdCliente == clienteId)
                .OrderByDescending(f => f.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Factura>> GetFacturasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(f => f.FechaEmision >= startDate && f.FechaEmision <= endDate)
                .Include(f => f.Cliente)
                .Include(f => f.DetallesFactura)
                .OrderByDescending(f => f.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
