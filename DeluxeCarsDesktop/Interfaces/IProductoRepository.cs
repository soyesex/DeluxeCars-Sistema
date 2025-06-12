using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IProductoRepository : IGenericRepository<Producto> 
    {
        /// Obtiene una lista de productos cuyo stock está por debajo de un umbral específico.
        Task<IEnumerable<Producto>> GetLowStockProductsAsync(int stockThreshold);
    }
}
