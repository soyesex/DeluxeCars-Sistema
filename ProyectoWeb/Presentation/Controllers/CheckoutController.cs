
using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json; // O Newtonsoft.Json si lo prefieres

namespace Aplicacion.Presentation.Controllers
{
    public class CheckoutController : Controller
    {
        // ANOTACIÓN 1: Ahora declaramos los DOS servicios que el controlador necesita.
        private readonly ICarritoService _carritoService;
        // ANOTACIÓN 2: El constructor ahora "pide" ambas herramientas.
        // El sistema de Inyección de Dependencias se las entregará automáticamente.
        public CheckoutController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        // GET: /Checkout/Index
        public IActionResult Index()
        {
            // Ahora esta línea funciona porque _carritoService SÍ existe.
            var carrito = _carritoService.GetCarrito();

            if (carrito == null || !carrito.Items.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                Carrito = carrito
            };

            return View(checkoutViewModel);
        }


        // GET: /Checkout/Confirmacion
        public IActionResult Confirmacion(int ordenId)
        {
            ViewBag.OrdenId = ordenId;
            return View();
        }
    }
}