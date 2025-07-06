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
        public async Task<int> CountAllAsync()
        {
            return await _context.Productos.CountAsync();
        }
        public async Task<IEnumerable<Producto>> SearchActivosConStockAsync(string searchTerm)
        {
            // 1. Buscamos productos activos que coincidan por nombre u OEM.
            //    Limitamos la búsqueda a 20 para no sobrecargar la lista de resultados.
            var productosCoincidentes = await _dbSet
                .Where(p => p.Estado == true &&
                            (p.Nombre.Contains(searchTerm) || p.OriginalEquipamentManufacture.Contains(searchTerm)))
                .Take(20)
                .AsNoTracking()
                .ToListAsync();

            if (!productosCoincidentes.Any())
            {
                return Enumerable.Empty<Producto>(); // Devuelve una lista vacía si no hay coincidencias
            }

            // 2. Obtenemos el stock actual SOLO para los productos que encontramos.
            var ids = productosCoincidentes.Select(p => p.Id);
            var stocks = await GetCurrentStocksAsync(ids);

            // 3. Devolvemos únicamente los productos cuyo stock es mayor a cero.
            return productosCoincidentes.Where(p => stocks.GetValueOrDefault(p.Id, 0) > 0);
        }
        private async Task<Dictionary<int, int>> GetAllProductStocksAsync()
        {
            // Esta consulta va a la tabla de movimientos UNA SOLA VEZ, agrupa por producto y suma.
            return await _context.MovimientosInventario
                .GroupBy(m => m.IdProducto)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Stock = g.Sum(m => m.TipoMovimiento == "Entrada por Compra" ? m.Cantidad : -m.Cantidad)
                })
                .ToDictionaryAsync(r => r.ProductoId, r => r.Stock);
        }
        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            // 1. Obtenemos todos los productos y todos los stocks en dos consultas simples.
            var todosLosProductos = await _context.Productos.AsNoTracking().ToListAsync();
            var todosLosStocks = await GetAllProductStocksAsync();

            // 2. Calculamos el valor total en la memoria de la aplicación.
            return todosLosProductos.Sum(p => p.Precio * todosLosStocks.GetValueOrDefault(p.Id, 0));
        }

        public async Task<int> CountOutOfStockProductsAsync()
        {
            var todosLosProductos = await _context.Productos.Select(p => p.Id).ToListAsync();
            var todosLosStocks = await GetAllProductStocksAsync();

            // Contamos en memoria.
            int count = 0;
            foreach (var id in todosLosProductos)
            {
                if (todosLosStocks.GetValueOrDefault(id, 0) == 0)
                {
                    count++;
                }
            }
            return count;
        }
        private IQueryable<ProductoStockDto> GetProductosConStockCalculado()
        {
            return _dbSet.Select(p => new ProductoStockDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                ImagenUrl = p.ImagenUrl,
                Descripcion = p.Descripcion,
                NombreCategoria = p.Categoria.Nombre,
                StockActual = _context.MovimientosInventario
                                      .Where(m => m.IdProducto == p.Id)
                                      .Sum(m => m.TipoMovimiento == "Entrada por Compra" ? m.Cantidad : -m.Cantidad)
            });
        }
        public async Task<IEnumerable<ProductoStockDto>> SearchPublicCatalogAsync(string categoria, string orden)
        {
            var query = GetProductosConStockCalculado().Where(dto => dto.StockActual > 0);

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.NombreCategoria.Equals(categoria));
            }

            switch (orden)
            {
                case "precio-asc":
                    query = query.OrderBy(p => p.Precio);
                    break;
                case "precio-desc":
                    query = query.OrderByDescending(p => p.Precio);
                    break;
                case "nombre-asc":
                default:
                    query = query.OrderBy(p => p.Nombre);
                    break;
            }
            return await query.AsNoTracking().ToListAsync();
        }
        public async Task<IEnumerable<ProductoStockDto>> GetProductosConStockPositivoAsync()
        {
            // Ahora usa la lógica centralizada y correcta
            return await GetProductosConStockCalculado()
                .Where(dto => dto.StockActual > 0)
                .ToListAsync();
        }
        public async Task<int> CountLowStockProductsAsync()
        {
            var productosConStockMinimo = await _context.Productos
                .Where(p => p.StockMinimo.HasValue && p.StockMinimo > 0)
                .Select(p => new { p.Id, p.StockMinimo })
                .ToListAsync();

            if (!productosConStockMinimo.Any()) return 0;

            var ids = productosConStockMinimo.Select(p => p.Id);
            var stocksActuales = await GetCurrentStocksAsync(ids);

            // Contamos en memoria.
            return productosConStockMinimo.Count(p => stocksActuales.GetValueOrDefault(p.Id, 0) < p.StockMinimo.Value);
        }
        public async Task<Dictionary<int, int>> GetCurrentStocksAsync(IEnumerable<int> productIds)
        {
            return await _context.MovimientosInventario
               .Where(m => productIds.Contains(m.IdProducto))
               .GroupBy(m => m.IdProducto)
               .Select(g => new { ProductId = g.Key, Stock = g.Sum(m => m.TipoMovimiento == "Entrada por Compra" ? m.Cantidad : -m.Cantidad) })
               .ToDictionaryAsync(r => r.ProductId, r => r.Stock);
        }

        private async Task<Dictionary<int, string>> GetPrimaryProvidersAsync(IEnumerable<int> productIds)
        {
            return await _context.ProductoProveedores
                .Where(pp => productIds.Contains(pp.IdProducto))
                .Include(pp => pp.Proveedor)
                .GroupBy(pp => pp.IdProducto)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProviderName = g.Select(x => x.Proveedor.RazonSocial).FirstOrDefault()
                })
                .ToDictionaryAsync(r => r.ProductId, r => r.ProviderName);
        }

        public async Task<int> GetCurrentStockAsync(int productoId)
        {
            return await _context.MovimientosInventario
                .Where(m => m.IdProducto == productoId)
                .SumAsync(m => m.TipoMovimiento == "Entrada por Compra" ? m.Cantidad : -m.Cantidad);
        }
        public async Task<Producto> GetByIdWithCategoriaAsync(int productoId)
        {
            return await _context.Productos
                                 .Include(p => p.Categoria)
                                 .FirstOrDefaultAsync(p => p.Id == productoId);
        }

        public async Task<PagedResult<ProductoStockDto>> SearchAsync(ProductSearchCriteria criteria)
        {
            var query = _context.Productos.AsQueryable();

            // --- Aplicamos filtros que sí se pueden traducir bien a SQL ---
            if (criteria.CategoryId.HasValue && criteria.CategoryId.Value != 0)
            {
                query = query.Where(p => p.IdCategoria == criteria.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.UniversalSearchText))
            {
                string searchText = criteria.UniversalSearchText;
                query = query.Where(p => p.Nombre.Contains(searchText) || p.OriginalEquipamentManufacture.Contains(searchText));
            }

            // --- La paginación se hace sobre esta consulta base ---
            var totalCount = await query.CountAsync();

            var productosDePagina = await query
                .OrderBy(p => p.Id)
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Include(p => p.Categoria) // Incluimos la categoría para tener el nombre
                .AsNoTracking()
                .ToListAsync();

            var productIds = productosDePagina.Select(p => p.Id).ToList();

            // --- Obtenemos datos relacionados (stock, proveedor) en consultas separadas y eficientes ---
            var stocks = await GetCurrentStocksAsync(productIds);
            var proveedores = await GetPrimaryProvidersAsync(productIds);

            // --- Construimos el DTO final en memoria ---
            var items = productosDePagina.Select(p => new ProductoStockDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                OEM = p.OriginalEquipamentManufacture,
                NombreCategoria = p.Categoria?.Nombre,
                Precio = p.Precio,
                Estado = p.Estado,
                StockMinimo = p.StockMinimo,
                Descripcion = p.Descripcion,
                ImagenUrl = p.ImagenUrl,
                StockActual = stocks.GetValueOrDefault(p.Id, 0),
                NombreProveedorPrincipal = proveedores.GetValueOrDefault(p.Id)
            }).ToList();

            // --- Filtro por estado de stock (se aplica en memoria después de obtener los datos) ---
            if (!string.IsNullOrWhiteSpace(criteria.StockStatus) && criteria.StockStatus != "Todos")
            {
                switch (criteria.StockStatus)
                {
                    case "En Stock":
                        items = items.Where(p => p.StockActual > 0).ToList();
                        break;
                    case "Agotado":
                        items = items.Where(p => p.StockActual == 0).ToList();
                        break;
                    case "Bajo Stock":
                        items = items.Where(p => p.StockMinimo.HasValue && p.StockMinimo > 0 && p.StockActual < p.StockMinimo.Value).ToList();
                        break;
                }
            }

            return new PagedResult<ProductoStockDto>
            {
                Items = items,
                TotalCount = totalCount
            };
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

