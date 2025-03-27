using Aplicacion.Application.ViewModels;

namespace Aplicacion.Core.Interfaces
{
    public interface IProductoService 
    {
        Task<IEnumerable<ProductoViewModel>> GetAll();
        Task<ProductoViewModel> GetById(int id);
        Task<bool> Add(ProductoViewModel model);
        Task<bool> Update(ProductoViewModel model);
        Task<bool> Delete(int id);
        //bool ExisteDocumento(string documento);
        //bool ExisteCelular(string celular);
        //bool ExisteEmail(string email);
        Task<IEnumerable<ProductoViewModel>> Search(string filtro);
        Task<IEnumerable<ProductoViewModel>> GetLimitedProducts(int count = 3);
    }
}
