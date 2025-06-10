using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IRolesRepository
    {
        Task<IEnumerable<Roles>> GetAllAsync();
        Task<Roles?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(Roles rol);
        Task<bool> ActualizarAsync(Roles rol);
        Task<bool> EliminarAsync(int id);
    }
}
