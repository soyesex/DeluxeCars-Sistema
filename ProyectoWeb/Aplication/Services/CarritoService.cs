
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
        public async Task AddItemAsync(int productoId, int cantidad)
        {
            var carrito = GetCarrito();

            // 4. Obtenemos el producto directamente del repositorio, de forma asíncrona
            var producto = await _unitOfWork.Productos.GetByIdAsync(productoId);

            if (producto == null) return;

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
                    // Asegúrate de que tu entidad Producto tenga ImagenUrl
                    ImagenUrl = producto.ImagenUrl
                });
            }

            GuardarCarritoEnSesion(carrito);
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