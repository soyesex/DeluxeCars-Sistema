using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsShared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace DeluxeCarsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IUnitOfWork unitOfWork, ILogger<ProductosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Le añadimos "stock" a la ruta para que sea más clara: api/productos/stock
        [HttpGet("stock")]
        [ProducesResponseType(typeof(IEnumerable<ProductoStockDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get() // Cambiamos el nombre para que sea más descriptivo
        {
            try
            {
                // 1. Llamamos al método que ahora nos devuelve los DTOs listos.
                var productosDto = await _unitOfWork.Productos.GetProductosConStockPositivoAsync();

                // 2. Simplemente los devolvemos. ¡No se necesita más mapeo!
                return Ok(productosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo productos con stock.");
                return StatusCode(500, "Ocurrió un error interno en el servidor.");
            }
        }
    }
}
