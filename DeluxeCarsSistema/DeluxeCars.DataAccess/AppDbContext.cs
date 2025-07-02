using DeluxeCarsEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        // --- DbSet por Grupo Lógico ---

        // Grupo 1: Catálogos y Datos Maestros
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Municipio> Municipios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<TipoServicio> TiposServicios { get; set; }
        public DbSet<EstadoFacturaElectronica> EstadosFacturaElectronica { get; set; }
        public DbSet<TipoDocumentoElectronico> TiposDocumentoElectronico { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }

        // Grupo 2: Entidades Principales (Core)
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }

        // Grupo 3: Entidades Transaccionales
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedidos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<ProductoProveedor> ProductoProveedores { get; set; }
        public DbSet<FacturaElectronica> FacturasElectronicas { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<PagoProveedor> PagosProveedores { get; set; }
        public DbSet<PagoProveedorPedido> PagoProveedorPedidos { get; set; }
        public DbSet<PagoCliente> PagosClientes { get; set; }
        public DbSet<PagoClienteFactura> PagoClienteFacturas { get; set; }
        public DbSet<NotaDeCredito> NotasDeCredito { get; set; }
        public DbSet<DetalleNotaDeCredito> DetallesNotaDeCredito { get; set; }

        /// <summary>
        /// Configura el modelo de la base de datos usando Fluent API.
        /// Este método es llamado por Entity Framework al crear el modelo por primera vez.
        /// Aquí se definen explícitamente las relaciones, llaves, índices, tipos de datos
        /// y restricciones que no se pueden inferir por convención o que requieren
        /// una configuración específica para que coincidan con el esquema de la base de datos.
        /// </summary>
        /// <param name="modelBuilder">El constructor que se usa para crear el modelo para este contexto.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- SEEDING DE DATOS ---
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema." },
                new Rol { Id = 2, Nombre = "Empleado", Descripcion = "Acceso limitado a ventas y operaciones diarias." }
            );
            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { Id = 1, Codigo = "EFE", Descripcion = "Efectivo", Disponible = true, Tipo = TipoMetodoPago.Efectivo, AplicaParaVentas = true, AplicaParaCompras = true },
                new MetodoPago { Id = 2, Codigo = "TDC", Descripcion = "Tarjeta de Crédito", Disponible = true, Tipo = TipoMetodoPago.Credito, AplicaParaVentas = true, AplicaParaCompras = false }
            );

            // 2. Sembrar la fila de configuración con TODOS los datos en UNA SOLA llamada.
            //    Usamos los datos reales de tu tienda que ya tenías.
            modelBuilder.Entity<Configuracion>().HasData(
                new Configuracion
                {
                    Id = 1,
                    NombreTienda = "Deluxe Cars",
                    Direccion = "La rosita",
                    Telefono = "3001234567",
                    Email = "deluxecars@gmail.com",
                    HorarioAtencion = "Lunes a Viernes de 8am a 6pm",
                    PorcentajeIVA = 19.0m,

                    // Pega los valores que copiaste de la consola aquí
                    AdminPINHash = new byte[] { 46, 201, 139, 121, 254, 155, 209, 14, 115, 229, 91, 103, 255, 176, 30, 131, 122, 65, 235, 77, 6, 150, 70, 115, 63, 221, 60, 247, 33, 228, 2, 101, 195, 83, 0, 52, 72, 89, 198, 33, 224, 70, 233, 125, 177, 74, 109, 102, 23, 223, 61, 33, 32, 20, 58, 102, 179, 205, 31, 83, 178, 63, 15, 90 },
                    AdminPINSalt = new byte[] { 200, 83, 160, 16, 78, 248, 207, 38, 92, 77, 89, 102, 153, 114, 81, 115, 83, 30, 186, 233, 102, 213, 73, 2, 96, 217, 113, 195, 170, 196, 242, 251, 129, 207, 218, 130, 213, 87, 159, 92, 195, 57, 218, 61, 141, 228, 57, 69, 141, 252, 79, 232, 234, 248, 158, 153, 105, 6, 107, 229, 59, 2, 96, 151, 68, 31, 241, 195, 1, 90, 56, 145, 138, 20, 210, 114, 191, 199, 75, 61, 21, 160, 101, 184, 223, 64, 29, 126, 194, 161, 64, 192, 154, 154, 89, 83, 176, 121, 121, 121, 206, 132, 229, 52, 47, 239, 88, 93, 102, 226, 96, 15, 61, 150, 137, 82, 38, 54, 163, 54, 136, 220, 154, 111, 209, 217, 250, 29 }
                }
            );

            // --- CONFIGURACIÓN DE ENTIDADES ---

            // Usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cliente
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Proveedor
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasIndex(p => p.NIT).IsUnique();
                entity.HasOne(p => p.Municipio)
                      .WithMany(m => m.Proveedores)
                      .HasForeignKey(p => p.IdMunicipio)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Precio).HasColumnType("decimal(18, 2)");
                entity.HasOne(p => p.Categoria)
                      .WithMany(c => c.Productos)
                      .HasForeignKey(p => p.IdCategoria)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasIndex(p => p.NumeroPedido).IsUnique();
                entity.HasOne(p => p.Proveedor)
                      .WithMany(prov => prov.Pedidos)
                      .HasForeignKey(p => p.IdProveedor)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.Usuario)
                      .WithMany(u => u.Pedidos)
                      .HasForeignKey(p => p.IdUsuario)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.MetodoPago)
                      .WithMany(mp => mp.Pedidos)
                      .HasForeignKey(p => p.IdMetodoPago)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // DetallePedido
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.Property(dp => dp.PrecioUnitario).HasColumnType("decimal(18, 2)");
                entity.Property(dp => dp.Descuento).HasColumnType("decimal(18, 2)");
                entity.Property(dp => dp.IVA).HasColumnType("decimal(18, 2)");
                entity.Property(dp => dp.Total).HasComputedColumnSql("((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))", stored: true);
                entity.HasOne(dp => dp.Pedido)
                      .WithMany(p => p.DetallesPedidos)
                      .HasForeignKey(dp => dp.IdPedido)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(dp => dp.Producto)
                      .WithMany(p => p.DetallesPedidos)
                      .HasForeignKey(dp => dp.IdProducto)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Factura
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasIndex(f => f.NumeroFactura).IsUnique();
                entity.HasOne(f => f.Cliente)
                      .WithMany(c => c.Facturas)
                      .HasForeignKey(f => f.IdCliente)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(f => f.Usuario)
                      .WithMany(u => u.Facturas)
                      .HasForeignKey(f => f.IdUsuario)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(f => f.MetodoPago)
                      .WithMany(mp => mp.Facturas)
                      .HasForeignKey(f => f.IdMetodoPago)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // DetalleFactura
            modelBuilder.Entity<DetalleFactura>(entity =>
            {
                entity.Property(df => df.PrecioUnitario).HasColumnType("decimal(18, 2)");
                entity.Property(df => df.Descuento).HasColumnType("decimal(18, 2)");
                entity.Property(df => df.IVA).HasColumnType("decimal(18, 2)");
                entity.Property(df => df.SubTotalLinea).HasComputedColumnSql("(Cantidad * PrecioUnitario - ISNULL(Descuento, 0))", stored: true);
                entity.Property(df => df.Total).HasComputedColumnSql("((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))", stored: true);
                entity.HasOne(df => df.Factura)
                      .WithMany(f => f.DetallesFactura)
                      .HasForeignKey(df => df.IdFactura)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductoProveedor
            modelBuilder.Entity<ProductoProveedor>(entity =>
            {
                entity.ToTable("ProductoProveedor");
                entity.HasIndex(pp => new { pp.IdProveedor, pp.IdProducto }).IsUnique();
                entity.Property(pp => pp.PrecioCompra).HasColumnType("decimal(18, 2)");
            });

            // NotaDeCredito y su Detalle
            modelBuilder.Entity<NotaDeCredito>(entity =>
            {
                entity.HasIndex(n => n.NumeroNota).IsUnique();
                entity.Property(n => n.MontoTotal).HasColumnType("decimal(18, 2)");
                entity.HasOne(n => n.FacturaOriginal)
                      .WithMany(f => f.NotasDeCredito)
                      .HasForeignKey(n => n.IdFacturaOriginal)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(n => n.Cliente).WithMany().HasForeignKey(n => n.IdCliente).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(n => n.Usuario).WithMany().HasForeignKey(n => n.IdUsuario).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DetalleNotaDeCredito>(entity =>
            {
                entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18, 2)");
                entity.Property(d => d.Total).HasColumnType("decimal(18, 2)");
                entity.HasOne(d => d.NotaDeCredito)
                      .WithMany(n => n.Detalles)
                      .HasForeignKey(d => d.IdNotaDeCredito);
                entity.HasOne(d => d.Producto)
                      .WithMany(p => p.DetallesNotaDeCredito)
                      .HasForeignKey(d => d.IdProducto)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // PagoProveedor y su enlace
            modelBuilder.Entity<PagoProveedor>(entity =>
            {
                entity.Property(p => p.MontoPagado).HasColumnType("decimal(18, 2)");
                entity.HasOne(p => p.Proveedor).WithMany().HasForeignKey(p => p.IdProveedor).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.MetodoPago).WithMany().HasForeignKey(p => p.IdMetodoPago).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.Usuario).WithMany().HasForeignKey(p => p.IdUsuario).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PagoProveedorPedido>(entity =>
            {
                entity.HasKey(pp => new { pp.IdPagoProveedor, pp.IdPedido });
                entity.HasOne(pp => pp.Pedido).WithMany(p => p.PagosAplicados).HasForeignKey(pp => pp.IdPedido);
                entity.HasOne(pp => pp.PagoProveedor).WithMany(p => p.PedidosCubiertos).HasForeignKey(pp => pp.IdPagoProveedor);
            });

            // PagoCliente y su enlace
            modelBuilder.Entity<PagoCliente>(entity =>
            {
                entity.Property(p => p.MontoRecibido).HasColumnType("decimal(18, 2)");
                entity.HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.IdCliente).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.MetodoPago).WithMany().HasForeignKey(p => p.IdMetodoPago).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(p => p.Usuario).WithMany().HasForeignKey(p => p.IdUsuario).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PagoClienteFactura>(entity =>
            {
                entity.HasKey(pf => new { pf.IdPagoCliente, pf.IdFactura });
                entity.HasOne(pf => pf.Factura).WithMany(f => f.PagosRecibidos).HasForeignKey(pf => pf.IdFactura);
                entity.HasOne(pf => pf.PagoCliente).WithMany(p => p.FacturasCubiertas).HasForeignKey(pf => pf.IdPagoCliente);
            });

            // Otros modelos
            modelBuilder.Entity<PasswordReset>().Property(e => e.Token).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Servicio>().Property(s => s.Precio).HasColumnType("decimal(18, 2)");
        }
    }
}
