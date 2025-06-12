using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IProductoRepository Productos { get; private set; }
        public ICategoriaRepository Categorias { get; private set; }
        public IClienteRepository Clientes { get; private set; }
        public IFacturaRepository Facturas { get; private set; }
        public IPedidoRepository Pedidos { get; private set; }
        public IProveedorRepository Proveedores { get; private set; }
        public IUsuarioRepository Usuarios { get; private set; }
        public IRolesRepository Roles { get; private set; }
        // ...Y las demás propiedades

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Productos = new ProductoRepository(_context);
            Categorias = new CategoriaRepository(_context);
            Clientes = new ClienteRepository(_context);
            Facturas = new FacturaRepository(_context);
            // ...Inicializar aquí todos los demás repositorios
            // Ejemplo para los que faltan:
            Pedidos = new PedidoRepository(_context);
            Proveedores = new ProveedorRepository(_context);
            Usuarios = new UsuarioRepository(_context);
            Roles = new RolesRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
