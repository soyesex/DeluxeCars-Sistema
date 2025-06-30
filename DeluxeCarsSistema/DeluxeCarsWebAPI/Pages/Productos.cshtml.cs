using DeluxeCarsShared;
using DeluxeCarsWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeluxeCarsWebAPI.Pages
{
    public class ProductosModel : PageModel
    {
        private readonly IApiClient _apiClient;

        // La lista de productos que se mostrará en la página
        public List<ProductoStockDto> ProductosEnStock { get; set; } = new();

        public ProductosModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Este método se ejecuta cuando se carga la página (petición GET)
        public async Task OnGetAsync()
        {
            ProductosEnStock = await _apiClient.GetProductosAsync();
        }
    }
}
