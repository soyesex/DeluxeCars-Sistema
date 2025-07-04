﻿using DeluxeCarsEntities;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeluxeCarsDesktop.Dtos;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IFacturaRepository : IGenericRepository<Factura>
    {
        Task<IEnumerable<TopProductoDto>> GetTopProductosVendidosAsync(DateTime startDate, DateTime endDate, int topN = 5);
        // Obtiene una factura incluyendo todos sus detalles (productos y servicios vendidos).
        Task<Factura> GetFacturaWithDetailsAsync(int facturaId);

        // Obtiene todas las facturas emitidas en un rango de fechas.
        Task<IEnumerable<Factura>> GetFacturasByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Obtiene el historial de facturas para un cliente específico.
        Task<IEnumerable<Factura>> GetFacturasByClienteAsync(int clienteId);
        Task<IEnumerable<Factura>> GetAllWithClienteYMetodoPagoAsync();
        Task<IEnumerable<ReporteRentabilidadDto>> GetReporteRentabilidadAsync(DateTime startDate, DateTime endDate);
    }
}
