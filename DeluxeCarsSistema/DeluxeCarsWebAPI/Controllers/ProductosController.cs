
using DeluxeCarsShared;
using DeluxeCarsWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeCarsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : Controller
    {

        private readonly FileStockDataService _stockDataService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(FileStockDataService stockDataService, ILogger<ProductosController> logger)
        {
            _stockDataService = stockDataService;
            logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductoStockDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var productos = await _stockDataService.GetProductosEnStockAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el endpoint GET /api/productos.");
                return StatusCode(500, "Ocurrió un error interno en el servidor.");
            }
        }
    }
}
