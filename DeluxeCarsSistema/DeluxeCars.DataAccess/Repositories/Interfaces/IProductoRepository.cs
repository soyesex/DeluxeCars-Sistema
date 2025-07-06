using DeluxeCars.DataAccess.Repositories.Implementations.Search;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface IProductoRepository : IGenericRepository<Producto>
    {
        // Solo definimos los métodos que son únicos para este repositorio
        Task<IEnumerable<Producto>> SearchActivosConStockAsync(string searchTerm);
        Task<Dictionary<int, int>> GetCurrentStocksAsync(IEnumerable<int> productIds);
        Task<int> GetCurrentStockAsync(int productoId);
        Task<Producto> GetByIdWithCategoriaAsync(int productoId);
        Task<PagedResult<ProductoStockDto>> SearchAsync(ProductSearchCriteria criteria);
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();
        Task<IEnumerable<Producto>> GetLowStockProductsAsync();
        Task<int> CountLowStockProductsAsync();
        Task<IEnumerable<Producto>> SearchProductsBySupplierAsync(int proveedorId, string searchTerm);
        Task<IEnumerable<Producto>> GetAssociatedProductsAsync(int proveedorId);
        Task<IEnumerable<Producto>> GetUnassociatedProductsAsync(int proveedorId);
        Task<IEnumerable<ProductoStockDto>> GetProductosConStockPositivoAsync();
        Task<int> CountAllAsync();
        Task<IEnumerable<ProductoStockDto>> SearchPublicCatalogAsync(string categoria, string orden);
        Task<decimal> GetTotalInventoryValueAsync();
        Task<int> CountOutOfStockProductsAsync();
    }
}
