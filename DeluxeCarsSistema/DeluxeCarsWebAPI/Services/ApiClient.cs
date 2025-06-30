using DeluxeCarsShared;

namespace DeluxeCarsWebAPI.Services
{
    public interface IApiClient
    {
        Task<List<ProductoStockDto>> GetProductosAsync();
    }

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductoStockDto>> GetProductosAsync()
        {
            try
            {
                // Hacemos la llamada al endpoint relativo de nuestra API.
                // La URL base (ej: https://localhost:7123) se configurará en Program.cs
                var productos = await _httpClient.GetFromJsonAsync<List<ProductoStockDto>>("api/productos");
                return productos ?? new List<ProductoStockDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al intentar obtener productos de la API.");
                // Devolvemos una lista vacía para que la página no se rompa.
                return new List<ProductoStockDto>();
            }
        }
    }
}
