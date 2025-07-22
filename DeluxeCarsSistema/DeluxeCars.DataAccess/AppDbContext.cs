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
        public DbSet<SiteContent> SiteContents { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<OrdenDetalle> OrdenDetalles { get; set; }

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
                    AdminPINHash = new byte[] { 46, 201, 139, 121, 254, 155, 209, 14, 115, 229, 91, 103, 255, 176, 30, 131, 122, 65, 235, 77, 6, 150, 70, 115, 63, 221, 60, 247, 33, 228, 2, 101, 195, 83, 0, 52, 72, 89, 198, 33, 224, 70, 233, 125, 177, 74, 109, 102, 23, 223, 61, 33, 32, 20, 58, 102, 179, 205, 31, 83, 178, 63, 15, 90 },
                    AdminPINSalt = new byte[] { 200, 83, 160, 16, 78, 248, 207, 38, 92, 77, 89, 102, 153, 114, 81, 115, 83, 30, 186, 233, 102, 213, 73, 2, 96, 217, 113, 195, 170, 196, 242, 251, 129, 207, 218, 130, 213, 87, 159, 92, 195, 57, 218, 61, 141, 228, 57, 69, 141, 252, 79, 232, 234, 248, 158, 153, 105, 6, 107, 229, 59, 2, 96, 151, 68, 31, 241, 195, 1, 90, 56, 145, 138, 20, 210, 114, 191, 199, 75, 61, 21, 160, 101, 184, 223, 64, 29, 126, 194, 161, 64, 192, 154, 154, 89, 83, 176, 121, 121, 121, 206, 132, 229, 52, 47, 239, 88, 93, 102, 226, 96, 15, 61, 150, 137, 82, 38, 54, 163, 54, 136, 220, 154, 111, 209, 217, 250, 29 },

                    // --- AÑADE VALORES POR DEFECTO PARA LOS NUEVOS CAMPOS ---
                    SmtpHost = "",
                    SmtpPort = 587, // 587 es un puerto estándar para SMTP
                    EmailEmisor = "",
                    PasswordEmailEmisor = null, // Inicia como nulo
                    EnableSsl = true // La mayoría de los servicios modernos lo requieren
                }
            );

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Frenos", Descripcion = "Componentes del sistema de frenado." },
                new Categoria { Id = 2, Nombre = "Suspensión y Dirección", Descripcion = "Amortiguadores, rótulas y componentes de dirección." },
                new Categoria { Id = 3, Nombre = "Motor", Descripcion = "Partes internas y externas del motor." },
                new Categoria { Id = 4, Nombre = "Filtros", Descripcion = "Filtros de aceite, aire, combustible y cabina." },
                new Categoria { Id = 5, Nombre = "Sistema Eléctrico", Descripcion = "Baterías, alternadores y sensores." },
                new Categoria { Id = 6, Nombre = "Transmisión", Descripcion = "Componentes de la caja de cambios y embrague." },
                new Categoria { Id = 7, Nombre = "Llantas y Rines", Descripcion = "Neumáticos y rines de varias medidas." }
            );

            // Seeding de Departamento y Municipios de Santander
            modelBuilder.Entity<Departamento>().HasData(
                new Departamento { Id = 1, Nombre = "Santander" },
                new Departamento { Id = 2, Nombre = "Antioquia" },
                new Departamento { Id = 3, Nombre = "Cundinamarca" },
                new Departamento { Id = 4, Nombre = "Valle del Cauca" }
            );

            modelBuilder.Entity<Municipio>().HasData(
                new Municipio { Id = 1, Nombre = "Bucaramanga", IdDepartamento = 1, Estado = true },
                new Municipio { Id = 2, Nombre = "Floridablanca", IdDepartamento = 1, Estado = true },
                new Municipio { Id = 3, Nombre = "Girón", IdDepartamento = 1, Estado = true },
                new Municipio { Id = 4, Nombre = "Piedecuesta", IdDepartamento = 1, Estado = true },
                new Municipio { Id = 5, Nombre = "San Gil", IdDepartamento = 1, Estado = true },
                new Municipio { Id = 6, Nombre = "Barichara", IdDepartamento = 1, Estado = true },
                // Antioquia
                new Municipio { Id = 7, Nombre = "Medellín", IdDepartamento = 2, Estado = true },
                new Municipio { Id = 8, Nombre = "Itagüí", IdDepartamento = 2, Estado = true },
                new Municipio { Id = 9, Nombre = "Envigado", IdDepartamento = 2, Estado = true },
                new Municipio { Id = 10, Nombre = "Bello", IdDepartamento = 2, Estado = true },
                new Municipio { Id = 11, Nombre = "Rionegro", IdDepartamento = 2, Estado = true },
                // Cundinamarca
                new Municipio { Id = 12, Nombre = "Bogotá D.C.", IdDepartamento = 3, Estado = true },
                new Municipio { Id = 13, Nombre = "Soacha", IdDepartamento = 3, Estado = true },
                new Municipio { Id = 14, Nombre = "Chía", IdDepartamento = 3, Estado = true },
                new Municipio { Id = 15, Nombre = "Funza", IdDepartamento = 3, Estado = true },
                // Valle del Cauca
                new Municipio { Id = 16, Nombre = "Cali", IdDepartamento = 4, Estado = true },
                new Municipio { Id = 17, Nombre = "Yumbo", IdDepartamento = 4, Estado = true },
                new Municipio { Id = 18, Nombre = "Palmira", IdDepartamento = 4, Estado = true },
                new Municipio { Id = 19, Nombre = "Buenaventura", IdDepartamento = 4, Estado = true }
            );

            // --- INICIO DEL CÓDIGO A PEGAR ---

            modelBuilder.Entity<Cliente>().HasData(
                 new Cliente
                 {
                     Id = 1,
                     Nombre = "Consumidor Final",
                     Telefono = "N/A",
                     Email = "consumidor@final.com",
                     Estado = true,
                     Identificacion = "999999999",
                     Direccion = "N/A",
                     TipoIdentificacion = "N/A", // Añadido para que no sea nulo
                     IdCiudad = null,
                     // --- VALORES AÑADIDOS ---
                     TipoCliente = "Persona Natural",
                     FechaCreacion = new DateTime(2024, 1, 1) // Usamos una fecha fija para datos iniciales
                 },
                 new Cliente
                 {
                     Id = 2,
                     Nombre = "Taller \"El Rápido\"",
                     Telefono = "3009988776",
                     Email = "compras@tallerrapido.com",
                     Estado = true,
                     Identificacion = "900123456-7",
                     Direccion = "Calle Falsa 123",
                     TipoIdentificacion = "NIT",
                     IdCiudad = null,
                     // --- VALORES AÑADIDOS ---
                     TipoCliente = "Taller",
                     FechaCreacion = new DateTime(2024, 1, 1)
                 },
                 new Cliente
                 {
                     Id = 3,
                     Nombre = "Lucía Fernandez",
                     Telefono = "3215554433",
                     Email = "lucia.f@email.com",
                     Estado = true,
                     Identificacion = "1098765432",
                     Direccion = "Avenida Siempre Viva 45",
                     TipoIdentificacion = "CC",
                     IdCiudad = null,
                     // --- VALORES AÑADIDOS ---
                     TipoCliente = "Persona Natural",
                     FechaCreacion = new DateTime(2024, 1, 1)
                 }
             );

            modelBuilder.Entity<Proveedor>().HasData(
                new Proveedor { Id = 1, RazonSocial = "AutoPartes El Frenazo S.A.S.", NIT = "900.123.456-7", Telefono = "3101234567", Email = "compras@elfrenazo.com", IdMunicipio = 7, Estado = true },
                new Proveedor { Id = 2, RazonSocial = "Importaciones Rueda Libre Ltda.", NIT = "830.987.654-1", Telefono = "3159876543", Email = "contacto@ruedalibre.co", IdMunicipio = 12, Estado = true },
                new Proveedor { Id = 3, RazonSocial = "Distribuciones El Pistón Veloz", NIT = "789.456.123-2", Telefono = "3005551212", Email = "proveedor@pistonveloz.com", IdMunicipio = 16, Estado = true }
            );

            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 6, IdCategoria = 1, Nombre = "Disco de Freno Ventilado", OriginalEquipamentManufacture = "DK-455A", Precio = 220000, Descripcion = "Disco de freno delantero para Chevrolet Captiva, Hyundai Tucson.", Estado = true, UnidadMedida = "Unidad", StockMinimo = 8, UltimoPrecioCompra = 150000 },
                new Producto { Id = 7, IdCategoria = 2, Nombre = "Rótula de Suspensión", OriginalEquipamentManufacture = "ROT-221B", Precio = 75000, Descripcion = "Rótula inferior para Toyota Hilux y Fortuner.", Estado = true, UnidadMedida = "Unidad", StockMinimo = 15, UltimoPrecioCompra = 45000 },
                new Producto { Id = 8, IdCategoria = 3, Nombre = "Bujía de Iridio", OriginalEquipamentManufacture = "IR-7700", Precio = 45000, Descripcion = "Bujía de alto rendimiento NGK para una mejor combustión.", Estado = true, UnidadMedida = "Unidad", StockMinimo = 20, UltimoPrecioCompra = 25000 },
                new Producto { Id = 9, IdCategoria = 5, Nombre = "Batería 12V 850A", OriginalEquipamentManufacture = "BAT-12850", Precio = 550000, Descripcion = "Batería sellada MAC Gold Plus, libre de mantenimiento.", Estado = true, UnidadMedida = "Unidad", StockMinimo = 5, UltimoPrecioCompra = 390000 },
                new Producto { Id = 10, IdCategoria = 6, Nombre = "Kit de Embrague", OriginalEquipamentManufacture = "ACK-2105", Precio = 750000, Descripcion = "Kit de clutch completo LUK para Chevrolet Spark GT.", Estado = true, UnidadMedida = "Kit", StockMinimo = 4, UltimoPrecioCompra = 550000 }
            );

            // En AppDbContext.cs, dentro de OnModelCreating

            modelBuilder.Entity<Producto>().HasData(
                // ... tus 10 productos existentes ...

                // --- AÑADE ESTOS NUEVOS PRODUCTOS ---
                new Producto
                {
                    Id = 11,
                    IdCategoria = 4,
                    Nombre = "Filtro de Combustible",
                    OriginalEquipamentManufacture = "FC-5501",
                    Precio = 65000,
                    Descripcion = "Filtro de gasolina para Chevrolet Sail, Onix.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 20,
                    UltimoPrecioCompra = 38000
                },
                new Producto
                {
                    Id = 12,
                    IdCategoria = 2,
                    Nombre = "Terminal de Dirección",
                    OriginalEquipamentManufacture = "TR-3320",
                    Precio = 95000,
                    Descripcion = "Terminal axial para Kia Rio, Hyundai Accent.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 10,
                    UltimoPrecioCompra = 55000
                },
                new Producto
                {
                    Id = 13,
                    IdCategoria = 5,
                    Nombre = "Alternador 12V 90A",
                    OriginalEquipamentManufacture = "ALT-9001",
                    Precio = 850000,
                    Descripcion = "Alternador genérico de alta capacidad para varios modelos.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 3,
                    UltimoPrecioCompra = 600000
                },
                new Producto
                {
                    Id = 14,
                    IdCategoria = 3,
                    Nombre = "Bomba de Agua",
                    OriginalEquipamentManufacture = "BMA-205B",
                    Precio = 280000,
                    Descripcion = "Bomba de agua con empaque para Ford Fiesta y Ecosport.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 5,
                    UltimoPrecioCompra = 180000
                },
                new Producto
                {
                    Id = 15,
                    IdCategoria = 1,
                    Nombre = "Líquido de Frenos DOT 4",
                    OriginalEquipamentManufacture = "LF-DOT4-500",
                    Precio = 38000,
                    Descripcion = "Botella de 500ml de líquido de frenos sintético.",
                    Estado = true,
                    UnidadMedida = "Botella",
                    StockMinimo = 30,
                    UltimoPrecioCompra = 22000
                },
                new Producto
                {
                    Id = 16,
                    IdCategoria = 6,
                    Nombre = "Aceite para Transmisión 75W-90",
                    OriginalEquipamentManufacture = "OIL-75W90",
                    Precio = 90000,
                    Descripcion = "Aceite sintético para caja de cambios manual.",
                    Estado = true,
                    UnidadMedida = "Litro",
                    StockMinimo = 12,
                    UltimoPrecioCompra = 65000
                },
                new Producto
                {
                    Id = 17,
                    IdCategoria = 4,
                    Nombre = "Filtro de Cabina Carbón Activado",
                    OriginalEquipamentManufacture = "FC-C220",
                    Precio = 55000,
                    Descripcion = "Filtro de aire para cabina con carbón activado anti-olores.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 15,
                    UltimoPrecioCompra = 30000
                },
                new Producto
                {
                    Id = 18,
                    IdCategoria = 2,
                    Nombre = "Bieleta de Suspensión Delantera",
                    OriginalEquipamentManufacture = "BIE-1090",
                    Precio = 60000,
                    Descripcion = "Bieleta o 'lápiz' de barra estabilizadora para Mazda 2.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 20,
                    UltimoPrecioCompra = 35000
                },
                new Producto
                {
                    Id = 19,
                    IdCategoria = 3,
                    Nombre = "Empaque de Culata",
                    OriginalEquipamentManufacture = "EMP-C16",
                    Precio = 120000,
                    Descripcion = "Empaque de culata multilámina para Renault Logan/Sandero 1.6L 8V.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 5,
                    UltimoPrecioCompra = 75000
                },
                new Producto
                {
                    Id = 20,
                    IdCategoria = 5,
                    Nombre = "Sensor de Oxígeno (Sonda Lambda)",
                    OriginalEquipamentManufacture = "O2S-4P",
                    Precio = 250000,
                    Descripcion = "Sensor de oxígeno de 4 pines, conector universal.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 8,
                    UltimoPrecioCompra = 160000
                },
                new Producto
                {
                    Id = 21,
                    IdCategoria = 1,
                    Nombre = "Cilindro Maestro de Freno",
                    OriginalEquipamentManufacture = "CMF-88A",
                    Precio = 310000,
                    Descripcion = "Bomba de freno principal para Nissan March / Versa.",
                    Estado = true,
                    UnidadMedida = "Unidad",
                    StockMinimo = 4,
                    UltimoPrecioCompra = 220000
                },
                new Producto
                {
                    Id = 22,
                    IdCategoria = 6,
                    Nombre = "Sincronizador de 3ra y 4ta",
                    OriginalEquipamentManufacture = "SYNC-34-RN",
                    Precio = 380000,
                    Descripcion = "Anillos sincronizadores para caja de Renault Twingo.",
                    Estado = true,
                    UnidadMedida = "Juego",
                    StockMinimo = 3,
                    UltimoPrecioCompra = 250000
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
                entity.Property(p => p.UltimoPrecioCompra).HasColumnType("decimal(18, 2)");
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
                //entity.Property(dp => dp.Total).HasColumnType("decimal(18, 2)");
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
                entity.Property(f => f.SubTotal).HasColumnType("decimal(18, 2)");
                entity.Property(f => f.TotalIVA).HasColumnType("decimal(18, 2)");
                entity.Property(f => f.Total).HasColumnType("decimal(18, 2)");
            });

            // DetalleFactura
            modelBuilder.Entity<DetalleFactura>(entity =>
            {
                entity.Property(df => df.PrecioUnitario).HasColumnType("decimal(18, 2)");
                entity.Property(df => df.Descuento).HasColumnType("decimal(18, 2)");
                entity.Property(df => df.IVA).HasColumnType("decimal(18, 2)");
                //entity.Property(df => df.SubTotalLinea).HasComputedColumnSql("(Cantidad * PrecioUnitario - ISNULL(Descuento, 0))", stored: true);
                //entity.Property(df => df.Total).HasComputedColumnSql("((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))", stored: true);
                entity.Property(df => df.SubTotalLinea).HasColumnType("decimal(18, 2)");
                entity.Property(df => df.Total).HasColumnType("decimal(18, 2)");
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

            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.Property(mp => mp.ComisionPorcentaje).HasColumnType("decimal(5, 2)"); // ej: 5 dígitos, 2 decimales para porcentajes
            });

            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.Property(mi => mi.CostoUnitario).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Orden>(entity =>
            {
                entity.Property(o => o.TotalOrden).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<OrdenDetalle>(entity =>
            {
                entity.Property(od => od.PrecioUnitario).HasColumnType("decimal(18, 2)");
            });

            // Otros modelos
            modelBuilder.Entity<PasswordReset>().Property(e => e.Token).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Servicio>().Property(s => s.Precio).HasColumnType("decimal(18, 2)");
        }
    }
}
