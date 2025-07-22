namespace Aplicacion.Application.ViewModels
{
    public class CarritoViewModel
    {
        public List<CarritoItemViewModel> Items { get; set; } = new List<CarritoItemViewModel>();
        public decimal Total => Items.Sum(item => item.Subtotal);
    }
}
