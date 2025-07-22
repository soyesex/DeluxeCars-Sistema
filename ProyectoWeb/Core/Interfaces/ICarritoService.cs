using Aplicacion.Application.ViewModels;

namespace Aplicacion.Core.Interfaces
{
    public interface ICarritoService
    {
        CarritoViewModel GetCarrito();
        Task<bool> AddItemAsync(int productoId, int cantidad);
        // Aquí podrías añadir métodos como RemoveItem, UpdateQuantity, etc.
        void RemoveItem(int productoId);
    }
}
