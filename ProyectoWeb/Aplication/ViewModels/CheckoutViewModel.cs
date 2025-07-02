using System.ComponentModel.DataAnnotations;

namespace Aplicacion.Application.ViewModels
{
    public class CheckoutViewModel
    {
        public CarritoViewModel Carrito { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La dirección de envío es obligatoria.")]
        [Display(Name = "Dirección de Envío")]
        public string? Direccion { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string? Telefono { get; set; }
    }
}
