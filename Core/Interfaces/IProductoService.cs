using Aplicacion.Application.ViewModels;

namespace Aplicacion.Core.Interfaces
{
    public interface IProductoService 
    {
        IEnumerable<ProductoViewModel> GetAll();
        ProductoViewModel GetById(int id);
        Task<bool> Add(ProductoViewModel model);
        Task<bool> Update(ProductoViewModel model);
        Task<bool> Delete(int id);
        //bool ExisteDocumento(string documento);
        //bool ExisteCelular(string celular);
        //bool ExisteEmail(string email);
        IEnumerable<ProductoViewModel> Search(string filtro);
    }
}
