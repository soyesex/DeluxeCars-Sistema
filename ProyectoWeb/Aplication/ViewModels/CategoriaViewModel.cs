using System.ComponentModel.DataAnnotations;

namespace Aplicacion.Application.ViewModels
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [Display(Name = "Nombre de la Categoría")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }
    }
}
