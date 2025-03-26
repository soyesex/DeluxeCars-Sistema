using Aplicacion.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Aplicacion.Presentation.Controllers
{
    public class PedidoController : Controller
    {
        public IActionResult Confirmar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Confirmar(string cartItems)
        {
            if (string.IsNullOrEmpty(cartItems))
            {
                return RedirectToAction("Index", "Catalogo");
            }

            // Deserializar los ítems del carrito
            var items = JsonSerializer.Deserialize<List<ProductoViewModel>>(cartItems);

            if (items == null || !items.Any())
            {
                return RedirectToAction("Index", "Catalogo");
            }

            // Aquí puedes procesar el pedido (guardarlo en la base de datos, etc.)
            // Por ahora, solo mostramos una vista de confirmación
            return View(items);
        }
    }
}
