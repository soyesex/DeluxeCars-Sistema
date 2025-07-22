using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCars.DataAccess.Repositories.Interfaces
{
    public interface IProveedorRepository : IGenericRepository<Proveedor>
    {
        Task<PagedResult<Proveedor>> SearchAsync(ProveedorSearchCriteria criteria);
        Task<IEnumerable<Producto>> GetSuppliedProductsAsync(int proveedorId);
        Task<IEnumerable<Proveedor>> GetAllWithLocationAsync();
        Task<Proveedor> GetByIdWithLocationAsync(int id);
    }
}
