using Aplicacion.Application.ViewModels;

namespace Aplicacion.Core.Interfaces
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoViewModel>> GetAllWithPositiveStockAsync();
        Task<IEnumerable<ProductoViewModel>> GetFeaturedProductsAsync(int count = 3);
        Task<ProductoViewModel> GetByIdAsync(int id);
        Task<bool> CreateAsync(ProductoViewModel model);
        Task<bool> UpdateAsync(ProductoViewModel model);
        Task<bool> DeleteAsync(int id);
        Task<int> CountAllAsync();
        Task<int> CountLowStockAsync(int umbral = 10);
    }
}
