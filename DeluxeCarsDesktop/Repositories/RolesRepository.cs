using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        private readonly AppDbContext _context;

        public RolesRepository(AppDbContext context)
        {
            _context = context;
        }
        public Task<bool> ActualizarAsync(Roles rol)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CrearAsync(Roles rol)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Roles?> ObtenerPorIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Roles>> GetAllAsync()
        {
            // 1. 'await' pausa la ejecución del método aquí, sin bloquear la aplicación.
            // 2. Cuando la base de datos responde, la ejecución continúa.
            // 3. El resultado de ToListAsync() se asigna a la variable 'roles'.
            var roles = await _context.Roles.ToListAsync();

            // 4. Simplemente retornas la lista. El compilador se encarga de envolverla en una Task.
            return roles;
        }
    }
}
