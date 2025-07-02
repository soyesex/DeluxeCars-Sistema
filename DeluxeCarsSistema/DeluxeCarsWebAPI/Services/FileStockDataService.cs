using DeluxeCarsShared.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

// Asegúrate de que el namespace sea el correcto
namespace DeluxeCarsWebAPI.Services
{
    public class FileStockDataService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FileStockDataService> _logger;
        private readonly string _jsonFilePath;
        private const string CacheKey = "ProductosStock";

        public FileStockDataService(IMemoryCache memoryCache, ILogger<FileStockDataService> logger, IWebHostEnvironment env)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _jsonFilePath = Path.Combine(env.ContentRootPath, "Data", "stock.json");
        }

        // El resto del código no debería tener errores
        public async Task<IEnumerable<ProductoStockDto>> GetProductosEnStockAsync()
        {
            return await _memoryCache.GetOrCreateAsync(CacheKey, async entry =>
            {
                _logger.LogInformation("Caché no encontrada. Leyendo desde archivo...");
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await ReadStockFileAsync();
            }) ?? new List<ProductoStockDto>();
        }

        private async Task<IEnumerable<ProductoStockDto>> ReadStockFileAsync()
        {
            if (!File.Exists(_jsonFilePath))
            {
                _logger.LogError("Archivo de stock no encontrado en {path}", _jsonFilePath);
                return Enumerable.Empty<ProductoStockDto>();
            }
            using var stream = File.OpenRead(_jsonFilePath);
            var productos = await JsonSerializer.DeserializeAsync<List<ProductoStockDto>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return productos ?? Enumerable.Empty<ProductoStockDto>();
        }
    }
}
