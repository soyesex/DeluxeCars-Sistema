using System.ComponentModel.DataAnnotations;

namespace DeluxeCarsEntities
{
    public class SiteContent
    {
        [Key] // La clave será un identificador único, ej: "HomePageHeroTitle"
        public string Key { get; set; }

        [Required]
        public string Value { get; set; } // El texto que se mostrará
    }
}
