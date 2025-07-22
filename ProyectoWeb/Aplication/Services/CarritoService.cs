
using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Newtonsoft.Json; // Necesitarás añadir el paquete NuGet Newtonsoft.Json

namespace Aplicacion.Application.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        // 2. Inyectamos IUnitOfWork en lugar de IProductoService
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "ShoppingCart";

        public CarritoService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        // 3. El método ahora es asíncrono para esperar la consulta a la BD
        // En: CarritoService.cs

        // El método ahora debe devolver Task<bool> para implementar la interfaz correctamente
        public async Task<bool> AddItemAsync(int productoId, int cantidad)
        {
            var carrito = GetCarrito();
            var producto = await _unitOfWork.Productos.GetByIdAsync(productoId);

            // Si el producto no se encuentra, devuelve false
            if (producto == null)
            {
                return false;
            }

            var itemEnCarrito = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (itemEnCarrito != null)
            {
                itemEnCarrito.Cantidad += cantidad;
            }
            else
            {
                carrito.Items.Add(new CarritoItemViewModel
                {
                    ProductoId = productoId,
                    NombreProducto = producto.Nombre,
                    Cantidad = cantidad,
                    Precio = producto.Precio,
                    // Importante: Asegúrate que la URL de la imagen sea raíz
                    ImagenUrl = string.IsNullOrEmpty(producto.ImagenUrl) ? "/images/default.jpeg" : "/" + producto.ImagenUrl
                });
            }

            GuardarCarritoEnSesion(carrito);

            // Si todo fue bien, devuelve true
            return true;
        }

        public CarritoViewModel GetCarrito()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var jsonCart = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(jsonCart))
            {
                return new CarritoViewModel();
            }
            return JsonConvert.DeserializeObject<CarritoViewModel>(jsonCart);
        }

        public void RemoveItem(int productoId)
        {
            var carrito = GetCarrito();
            var itemToRemove = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (itemToRemove != null)
            {
                carrito.Items.Remove(itemToRemove);
                GuardarCarritoEnSesion(carrito);
            }
        }

        private void GuardarCarritoEnSesion(CarritoViewModel carrito)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var jsonCart = JsonConvert.SerializeObject(carrito);
            session.SetString(CartSessionKey, jsonCart);
        }
    }
}