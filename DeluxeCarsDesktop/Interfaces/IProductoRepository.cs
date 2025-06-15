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
        // Solo definimos los métodos que son únicos para este repositorio
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();
        Task<IEnumerable<Producto>> GetLowStockProductsAsync(int stockThreshold);
        Task<IEnumerable<Producto>> SearchProductsBySupplierAsync(int proveedorId, string searchTerm);
        Task<IEnumerable<Producto>> GetAssociatedProductsAsync(int proveedorId);
        Task<IEnumerable<Producto>> GetUnassociatedProductsAsync(int proveedorId);
    }
}
