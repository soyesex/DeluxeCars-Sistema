using DeluxeCarsDesktop.Data;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        AppDbContext Context { get; }
        IProductoRepository Productos { get; }
        ICategoriaRepository Categorias { get; }
        IClienteRepository Clientes { get; }
        IFacturaRepository Facturas { get; }
        IPedidoRepository Pedidos { get; }
        IProveedorRepository Proveedores { get; }
        IUsuarioRepository Usuarios { get; }
        IRolesRepository Roles { get; }
        IDepartamentoRepository Departamentos { get; }
        IServicioRepository Servicios { get; }
        IMetodoPagoRepository MetodosPago { get; }
        IMunicipioRepository Municipios { get; }
        ITipoServicioRepository TiposServicios { get; }
        IProductoProveedorRepository ProductoProveedores { get; }
        INotificacionRepository Notificaciones { get; }
        IMovimientoInventarioRepository MovimientosInventario { get; }
        IPagoProveedorRepository PagosProveedores { get; }
        IPagoClienteRepository PagosClientes { get; }
        INotaDeCreditoRepository NotasDeCredito { get; }
        IConfiguracionRepository Configuraciones { get; }
        // ...Añadir aquí todas las demás interfaces de repositorio

        /// <summary>
        /// Guarda todos los cambios realizados en este contexto en la base de datos.
        /// </summary>
        /// <returns>El número de objetos de estado escritos en la base de datos.</returns>
        Task<int> CompleteAsync();
    }
}
