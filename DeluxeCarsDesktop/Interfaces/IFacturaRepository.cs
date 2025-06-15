using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IFacturaRepository : IGenericRepository<Factura>
    {
        // Obtiene una factura incluyendo todos sus detalles (productos y servicios vendidos).
        Task<Factura> GetFacturaWithDetailsAsync(int facturaId);

        // Obtiene todas las facturas emitidas en un rango de fechas.
        Task<IEnumerable<Factura>> GetFacturasByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Obtiene el historial de facturas para un cliente específico.
        Task<IEnumerable<Factura>> GetFacturasByClienteAsync(int clienteId);
        Task<IEnumerable<Factura>> GetAllWithClienteYMetodoPagoAsync();
    }
}
