using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IProveedorRepository : IGenericRepository<Proveedor>
    {
        Task<IEnumerable<Producto>> GetSuppliedProductsAsync(int proveedorId);
        Task<IEnumerable<Proveedor>> GetAllWithLocationAsync();
        Task<Proveedor> GetByIdWithLocationAsync(int id);
    }
}
