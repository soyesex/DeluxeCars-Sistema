namespace Aplicacion.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ProductoViewModel> ProductosDestacados { get; set; } = new List<ProductoViewModel>();
        public Dictionary<string, string> PageContent { get; set; } = new Dictionary<string, string>();
    }
}
