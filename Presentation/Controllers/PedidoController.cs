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
        public IActionResult Confirmar(string cartItemsInput)
        {
            System.Diagnostics.Debug.WriteLine("Datos recibidos en Confirmar: " + cartItemsInput);

            if (string.IsNullOrEmpty(cartItemsInput))
            {
                TempData["Error"] = "No se recibieron datos del carrito.";
                return RedirectToAction("Index", "Catalogo");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pedido = JsonSerializer.Deserialize<List<ProductoViewModel>>(cartItemsInput, options);

            return View(pedido);
        }



    }
}
