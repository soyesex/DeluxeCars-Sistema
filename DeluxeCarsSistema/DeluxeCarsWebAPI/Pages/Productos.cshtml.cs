using DeluxeCarsShared;
using DeluxeCarsWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeluxeCarsWebAPI.Pages
{
    public class ProductosModel : PageModel
    {
        private readonly IApiClient _apiClient;

        // La lista de productos que se mostrar� en la p�gina
        public List<ProductoStockDto> ProductosEnStock { get; set; } = new();

        public ProductosModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Este m�todo se ejecuta cuando se carga la p�gina (petici�n GET)
        public async Task OnGetAsync()
        {
            ProductosEnStock = await _apiClient.GetProductosAsync();
        }
    }
}
