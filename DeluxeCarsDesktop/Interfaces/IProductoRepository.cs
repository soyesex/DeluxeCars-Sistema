using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IProductoRepository : IGenericRepository<Producto>
    {
        // Solo definimos los métodos que son únicos para este repositorio
        Task<Dictionary<int, int>> GetCurrentStocksAsync(IEnumerable<int> productIds);
        Task<int> GetCurrentStockAsync(int productoId);
        Task<Producto> GetByIdWithCategoriaAsync(int productoId);
        Task<IEnumerable<Producto>> SearchAsync(ProductSearchCriteria criteria);
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();
        Task<IEnumerable<Producto>> GetLowStockProductsAsync(int stockThreshold);
        Task<IEnumerable<Producto>> SearchProductsBySupplierAsync(int proveedorId, string searchTerm);
        Task<IEnumerable<Producto>> GetAssociatedProductsAsync(int proveedorId);
        Task<IEnumerable<Producto>> GetUnassociatedProductsAsync(int proveedorId);
    }
}
