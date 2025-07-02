
using Aplicacion.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;

        public CarritoController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        public IActionResult Index()
        {
            // 1. Llama al servicio para obtener el carrito actual de la sesión del usuario.
            var carrito = _carritoService.GetCarrito();

            // 2. Envía el objeto 'carrito' a la vista para que pueda mostrar los datos.
            return View(carrito);
        }

        // Endpoint para añadir productos
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productoId, int cantidad) // 1. Añadir async Task<>
        {
            if (cantidad <= 0)
            {
                return Json(new { success = false, message = "La cantidad debe ser mayor a cero." });
            }

            // 2. Usar await y llamar al método con sufijo Async
            await _carritoService.AddItemAsync(productoId, cantidad);

            var carrito = _carritoService.GetCarrito();
            return Json(new { success = true, message = "¡Producto añadido al carrito!", itemCount = carrito.Items.Sum(i => i.Cantidad) });
        }
        // En CarritoController.cs

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var carrito = _carritoService.GetCarrito();
            var itemCount = carrito.Items.Sum(i => i.Cantidad);
            return Json(new { itemCount = itemCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Añadimos seguridad
        public IActionResult RemoveFromCart(int productoId)
        {
            _carritoService.RemoveItem(productoId);

            // Después de eliminar, devolvemos el estado actualizado del carrito
            var carrito = _carritoService.GetCarrito();
            return Json(new
            {
                success = true,
                message = "Producto eliminado del carrito.",
                newTotal = carrito.Total.ToString("C0"),
                itemCount = carrito.Items.Sum(i => i.Cantidad),
                cartIsEmpty = !carrito.Items.Any()
            });
        }
    }
}