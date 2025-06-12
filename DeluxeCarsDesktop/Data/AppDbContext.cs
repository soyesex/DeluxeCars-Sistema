using DeluxeCarsDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        // --- DbSet por Grupo Lógico ---

        // Grupo 1: Catálogos y Datos Maestros
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Municipio> Municipios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<TipoServicio> TiposServicios { get; set; }
        public DbSet<EstadoFacturaElectronica> EstadosFacturaElectronica { get; set; }
        public DbSet<TipoDocumentoElectronico> TiposDocumentoElectronico { get; set; }

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
            // Llama a la implementación base para que aplique sus propias convenciones antes de nuestras configuraciones.
            base.OnModelCreating(modelBuilder);

            // --- SECCIÓN 1: Configuraciones de Llaves Únicas (UNIQUE CONSTRAINTS) ---
            // Asegura que ciertos campos no puedan tener valores duplicados en la base de datos.

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.NIT)
                .IsUnique();

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.NumeroPedido)
                .IsUnique();

            modelBuilder.Entity<Factura>()
                .HasIndex(f => f.NumeroFactura)
                .IsUnique();

            // Configuración para la llave única compuesta en la tabla de unión ProductoProveedor.
            modelBuilder.Entity<ProductoProveedor>()
                .HasIndex(pp => new { pp.IdProveedor, pp.IdProducto })
                .IsUnique();

            // --- SECCIÓN 2: Configuraciones de Comportamiento al Eliminar (ON DELETE) ---
            // Define qué sucede con las entidades relacionadas cuando una entidad principal es eliminada.

            // ON DELETE SET NULL: Si se borra un Usuario, el IdUsuario en la Factura se establece a NULL.
            // Esto conserva el registro de la factura incluso si el empleado ya no trabaja en la empresa.
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Usuario)
                .WithMany(u => u.Facturas)
                .HasForeignKey(f => f.IdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

            // ON DELETE CASCADE: Si se borra una entidad principal, sus detalles asociados también se borran.
            // Ejemplo: Al eliminar un Pedido, se eliminan todas sus líneas de DetallePedido.
            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Pedido)
                .WithMany(p => p.DetallesPedidos)
                .HasForeignKey(dp => dp.IdPedido)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleFactura>()
                .HasOne(df => df.Factura)
                .WithMany(f => f.DetallesFactura)
                .HasForeignKey(df => df.IdFactura)
                .OnDelete(DeleteBehavior.Cascade);

            // ON DELETE RESTRICT (NO ACTION): Es el comportamiento por defecto de EF Core.
            // Previene que se elimine una entidad si hay otras que dependen de ella.
            // Ejemplo explícito: No se puede borrar una Categoría si tiene Productos asociados.
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);


            // --- SECCIÓN 3: Configuraciones de Precisión para Campos Decimales ---
            // Define el tipo de dato SQL exacto para todos los campos monetarios y decimales.
            // Esto es crucial para evitar problemas de redondeo y asegurar la consistencia con la DB.

            // Modelo: Producto
            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Producto>().Property(p => p.UltimoPrecioCompra).HasColumnType("decimal(18, 2)");

            // Modelo: Servicio
            modelBuilder.Entity<Servicio>().Property(s => s.Precio).HasColumnType("decimal(18, 2)");

            // Modelo: ProductoProveedor
            modelBuilder.Entity<ProductoProveedor>().Property(pp => pp.PrecioCompra).HasColumnType("decimal(18, 2)");

            // Modelo: Pedido y Detalles
            modelBuilder.Entity<DetallePedido>().Property(dp => dp.PrecioUnitario).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<DetallePedido>().Property(dp => dp.Descuento).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<DetallePedido>().Property(dp => dp.IVA).HasColumnType("decimal(18, 2)");

            // Modelo: Factura y Detalles
            modelBuilder.Entity<Factura>().Property(f => f.SubTotal).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Factura>().Property(f => f.TotalIVA).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Factura>().Property(f => f.Total).HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<DetalleFactura>().Property(df => df.PrecioUnitario).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<DetalleFactura>().Property(df => df.Descuento).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<DetalleFactura>().Property(df => df.IVA).HasColumnType("decimal(18, 2)");


            // --- SECCIÓN 4: Configuración de Columnas Calculadas (COMPUTED COLUMNS) ---
            // Informa a Entity Framework que estas propiedades son calculadas por la base de datos (PERSISTED).
            // EF no intentará insertar o actualizar valores en estas columnas.

            modelBuilder.Entity<DetallePedido>()
                .Property(dp => dp.Total)
                .HasComputedColumnSql("((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))", stored: true);

            modelBuilder.Entity<DetalleFactura>()
                .Property(df => df.SubTotalLinea)
                .HasComputedColumnSql("(Cantidad * PrecioUnitario - ISNULL(Descuento, 0))", stored: true);

            modelBuilder.Entity<DetalleFactura>()
                .Property(df => df.Total)
                .HasComputedColumnSql("((Cantidad * PrecioUnitario - ISNULL(Descuento, 0)) * (1 + ISNULL(IVA, 0)/100))", stored: true);
        }

    }
}
