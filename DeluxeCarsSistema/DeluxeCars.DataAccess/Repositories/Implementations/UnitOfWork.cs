using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Utils;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public AppDbContext Context => _context;

        public IProductoRepository Productos { get; private set; }
        public ICategoriaRepository Categorias { get; private set; }
        public IClienteRepository Clientes { get; private set; }
        public IFacturaRepository Facturas { get; private set; }
        public IPedidoRepository Pedidos { get; private set; }
        public IProveedorRepository Proveedores { get; private set; }
        public IUsuarioRepository Usuarios { get; private set; }
        public IRolesRepository Roles { get; private set; }
        public IDepartamentoRepository Departamentos { get; private set; }
        public IServicioRepository Servicios { get; private set; }
        public IMunicipioRepository Municipios { get; private set; }
        public IMetodoPagoRepository MetodosPago { get; private set; }
        public ITipoServicioRepository TiposServicios { get; private set; }
        public IProductoProveedorRepository ProductoProveedores { get; private set; }
        public INotificacionRepository Notificaciones { get; private set; }
        public IMovimientoInventarioRepository MovimientosInventario { get; private set; }
        public IPagoProveedorRepository PagosProveedores { get; private set; }
        public IPagoClienteRepository PagosClientes { get; private set; }
        public INotaDeCreditoRepository NotasDeCredito { get; private set; }
        public IConfiguracionRepository Configuraciones { get; private set; }

        // ...Y las demás propiedades

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            TiposServicios = new TipoServicioRepository(_context);
            Productos = new ProductoRepository(_context);
            Categorias = new CategoriaRepository(_context);
            Clientes = new ClienteRepository(_context);
            Facturas = new FacturaRepository(_context);
            Departamentos = new DepartamentoRepository(_context);
            Servicios = new ServicioRepository(_context);
            Municipios = new MunicipioRepository(_context);
            MetodosPago = new MetodoPagoRepository(_context);
            ProductoProveedores = new ProductoProveedorRepository(_context);
            Notificaciones = new NotificacionRepository(_context);
            MovimientosInventario = new MovimientoInventarioRepository(_context);
            PagosProveedores = new PagoProveedorRepository(_context);
            PagosClientes = new PagoClienteRepository(_context);
            NotasDeCredito = new NotaDeCreditoRepository(_context);
            Configuraciones = new ConfiguracionRepository(_context);

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

        // En UnitOfWork.cs
        public async Task<bool> ValidarPinAdministradorAsync(string pinIntroducido)
        {
            if (string.IsNullOrEmpty(pinIntroducido)) return false;

            try
            {
                var config = await Configuraciones.GetByIdAsync(1);

                // Si no hay configuración o no se ha establecido un PIN, la validación falla.
                if (config == null || config.AdminPINHash == null || config.AdminPINSalt == null)
                {
                    return false;
                }

                // ¡Usamos tu PasswordHelper existente!
                return PasswordHelper.VerifyPasswordHash(pinIntroducido, config.AdminPINHash, config.AdminPINSalt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al validar PIN: {ex.Message}");
                return false;
            }
        }
    }
}
