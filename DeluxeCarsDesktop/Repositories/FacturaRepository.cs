using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Dtos;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class FacturaRepository : GenericRepository<Factura>, IFacturaRepository
    {
        public FacturaRepository(AppDbContext context) : base(context)
        {
        }
        // En Repositories/FacturaRepository.cs

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
                              TotalVenta = f.Total ?? 0,
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
                         .IgnoreQueryFilters() // <-- AÑADE ESTA LÍNEA
                         .AsNoTracking()
                         .OrderByDescending(f => f.FechaEmision)
                         .ToListAsync();
        }
        public async Task<Factura> GetFacturaWithDetailsAsync(int facturaId)
        {
            return await _dbSet
                .Include(f => f.Cliente) // Incluye el cliente relacionado
                .Include(f => f.Usuario) // Incluye el usuario (vendedor)
                .Include(f => f.DetallesFactura) // Incluye las líneas de detalle
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
                .OrderByDescending(f => f.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
