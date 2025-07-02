using System.ComponentModel.DataAnnotations;

namespace Aplicacion.Application.ViewModels
{
    public class ProductoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre del Producto")]
        public string? Nombre { get; set; }

        // Typo corregido para consistencia.
        [Display(Name = "Número de Parte (OEM)")]
        public string? OriginalEquipmentManufacture { get; set; }

        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }

        // Propiedad eliminada porque no existe en la entidad Producto.
        // public DateTime FechaIngreso { get; set; }

        // Cambiado a ImagenUrl para ser más consistente con la entidad.
        public string? ImagenUrl { get; set; }

        public int IdCategoria { get; set; }

        [Display(Name = "Categoría")]
        public string? NombreCategoria { get; set; }
    }
}
