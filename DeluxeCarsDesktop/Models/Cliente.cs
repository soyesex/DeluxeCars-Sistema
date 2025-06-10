using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(80)]
        public string Email { get; set; }

        [Required]
        public bool Estado { get; set; }
    }
}
