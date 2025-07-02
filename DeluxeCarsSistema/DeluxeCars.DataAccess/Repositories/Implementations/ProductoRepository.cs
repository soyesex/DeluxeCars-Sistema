using DeluxeCars.DataAccess.Repositories.Implementations.Search;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsShared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
    {
        public ProductoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductoStockDto>> GetProductosConStockPositivoAsync()
        {
            return await _dbSet
                .Select(p => new ProductoStockDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl,
                    Descripcion = p.Descripcion, // <-- AÑADE ESTA LÍNEA

                    StockActual = _context.MovimientosInventario
                                          .Where(m => m.IdProducto == p.Id)
                                          .Sum(m => m.TipoMovimiento == "ENTRADA" ? m.Cantidad : -m.Cantidad)
                })
                .Where(dto => dto.StockActual > 0)
                .ToListAsync();
        }
        public async Task<int> CountLowStockProductsAsync()
        {
            // Es la misma lógica que GetLowStockProductsAsync, pero en lugar de
            // devolver la lista completa, usamos CountAsync() para que EF
            // genere un "SELECT COUNT(*)" en SQL. Mucho más rápido.
            return await _context.Productos
                .CountAsync(p => p.StockMinimo.HasValue && p.StockMinimo > 0 &&
                            (_context.MovimientosInventario
                                   .Where(m => m.IdProducto == p.Id)
                                   .Sum(m => (int?)m.Cantidad) ?? 0) < p.StockMinimo);
        }
        public async Task<Dictionary<int, int>> GetCurrentStocksAsync(IEnumerable<int> productIds)
        {
            // Esta consulta va a la tabla de movimientos una sola vez
            return await _context.MovimientosInventario
                // Filtra solo por los IDs de los productos que nos interesan
                .Where(m => productIds.Contains(m.IdProducto))
                // Agrupa por IdProducto
                .GroupBy(m => m.IdProducto)
                // Para cada grupo, crea un par clave-valor: (IdProducto, Suma de Cantidades)
                .Select(g => new { ProductId = g.Key, Stock = g.Sum(m => m.Cantidad) })
                // Convierte el resultado final a un diccionario para un acceso súper rápido.
                .ToDictionaryAsync(r => r.ProductId, r => r.Stock);
        }
        public async Task<int> GetCurrentStockAsync(int productoId)
        {
            // Suma todas las cantidades (positivas y negativas) para un producto.
            // Si no hay movimientos, devuelve 0.
            return await _context.MovimientosInventario
                                 .Where(m => m.IdProducto == productoId)
                                 .SumAsync(m => (int?)m.Cantidad) ?? 0;
        }
        public async Task<Producto> GetByIdWithCategoriaAsync(int productoId)
        {
            return await _context.Productos
                                 .Include(p => p.Categoria)
                                 .FirstOrDefaultAsync(p => p.Id == productoId);
        }

        public async Task<IEnumerable<Producto>> SearchAsync(ProductSearchCriteria criteria)
        {
            // 1. Empezamos con una consulta base que trae todos los productos
            //    Usamos AsQueryable() para poder añadirle filtros dinámicamente.
            var query = _context.Productos
                                .Include(p => p.Categoria) // Incluimos la categoría para mostrar su nombre
                                .AsQueryable();

            // 2. Aplicamos cada filtro solo si tiene un valor

            // --- Filtro por Categoría ---
            if (criteria.CategoryId.HasValue && criteria.CategoryId.Value != 0)
            {
                query = query.Where(p => p.IdCategoria == criteria.CategoryId.Value);
            }

            // --- Filtro por Estado de Stock ---
            if (!string.IsNullOrWhiteSpace(criteria.StockStatus))
            {

                switch (criteria.StockStatus)
                {
                    case "En Stock":
                        // Un producto está "En Stock" si la suma de sus movimientos es > 0
                        query = query.Where(p => _context.MovimientosInventario
                                                       .Where(m => m.IdProducto == p.Id)
                                                       .Sum(m => (int?)m.Cantidad) > 0);
                        break;
                    case "Agotado":
                        // Un producto está "Agotado" si la suma es 0 o no tiene movimientos
                        query = query.Where(p => (_context.MovimientosInventario
                                                      .Where(m => m.IdProducto == p.Id)
                                                      .Sum(m => (int?)m.Cantidad) ?? 0) == 0);
                        break;
                    case "Bajo Stock":
                        // "Bajo Stock" si la suma es > 0 Y es menor que su StockMinimo configurado
                        query = query.Where(p => p.StockMinimo.HasValue && p.StockMinimo > 0 &&
                                                 (_context.MovimientosInventario
                                                        .Where(m => m.IdProducto == p.Id)
                                                        .Sum(m => (int?)m.Cantidad) ?? 0) < p.StockMinimo);
                        break;
                }
            }

            // --- Filtro de Texto Universal ---
            if (!string.IsNullOrWhiteSpace(criteria.UniversalSearchText))
            {
                string searchText = criteria.UniversalSearchText;

                // NOTA: No usamos .ToLower() aquí porque ya configuramos la base de datos
                // para que sea case-insensitive (CI). La BD lo hará más rápido.
                query = query.Where(p =>
                    p.Nombre.Contains(searchText) ||
                    p.OriginalEquipamentManufacture.Contains(searchText) ||
                    p.Descripcion.Contains(searchText)
                );
            }

            // 3. Finalmente, ejecutamos la consulta en la base de datos
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetAssociatedProductsAsync(int proveedorId)
        {
            return await _context.ProductoProveedores
                .Where(pp => pp.IdProveedor == proveedorId)
                // 1. PRIMERO, incluimos el Producto y, DENTRO de él, su Categoría.
                .Include(pp => pp.Producto)
                    .ThenInclude(p => p.Categoria)
                // 2. AHORA, al final, seleccionamos el Producto que queremos devolver.
                .Select(pp => pp.Producto)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetUnassociatedProductsAsync(int proveedorId)
        {
            var associatedProductIds = await _context.ProductoProveedores
                .Where(pp => pp.IdProveedor == proveedorId)
                .Select(pp => pp.IdProducto)
                .ToListAsync();

            return await _context.Productos
                .Where(p => !associatedProductIds.Contains(p.Id))
                .Include(p => p.Categoria)
                .ToListAsync();
        }
        // ÚNICA RESPONSABILIDAD: Implementar los métodos especializados.
        public async Task<IEnumerable<Producto>> SearchProductsBySupplierAsync(int proveedorId, string searchTerm)
        {
            var query = from pp in _context.ProductoProveedores
                        join p in _context.Productos on pp.IdProducto equals p.Id
                        where pp.IdProveedor == proveedorId && p.Nombre.Contains(searchTerm)
                        select p;

            // Añadimos AsNoTracking() para que EF solo lea los datos sin "vigilarlos"
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetAllWithCategoriaAsync()
        {
            // La consulta final y limpia para traer productos con su categoría
            return await _context.Productos
                                 .Include(p => p.Categoria)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetLowStockProductsAsync()
        {
            // Este método busca todos los productos cuyo stock calculado
            // sea menor que su StockMinimo configurado.
            return await _context.Productos
                                // --- INICIO DE LA CORRECCIÓN ---
                                // Le decimos a EF que incluya en la consulta la tabla de unión...
                                .Include(p => p.ProductoProveedores)
                                // ...y DENTRO de esa tabla de unión, que incluya los datos del Proveedor.
                                .ThenInclude(pp => pp.Proveedor)
                                // --- FIN DE LA CORRECCIÓN ---
                                .Where(p => p.StockMinimo.HasValue && p.StockMinimo > 0 &&
                                            (_context.MovimientosInventario
                                                .Where(m => m.IdProducto == p.Id)
                                                .Sum(m => (int?)m.Cantidad) ?? 0) < p.StockMinimo)
                                .AsNoTracking()
                                .ToListAsync();
        }
    }
}

