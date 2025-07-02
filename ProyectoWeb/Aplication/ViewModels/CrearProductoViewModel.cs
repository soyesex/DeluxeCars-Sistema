using System.ComponentModel.DataAnnotations;

namespace Aplicacion.Application.ViewModels
{
    public class CrearProductoViewModel
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [Display(Name = "Nombre del Producto")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Número de Parte (OEM)")]
        public string? OEM { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio de Venta")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "La cantidad en stock es obligatoria.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Cantidad en Stock")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int IdCategoria { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "URL o Nombre de la Imagen")]
        public string? ImagenUrl { get; set; }
    }
}
