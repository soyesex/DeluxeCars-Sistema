using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsEntities
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }

        /*Un campo genérico para guardar un dato numérico asociado a la notificación.
         Para las alertas de stock, guardaremos aquí el recuento de productos.*/
        public int? DataCount { get; set; }

        // --- NUEVA PROPIEDAD ---
        // Clave foránea opcional al pedido relacionado.
        // Es 'nullable' (int?) porque no todas las notificaciones
        // estarán relacionadas con un pedido (ej: las de bajo stock).
        public int? PedidoId { get; set; }

        // Esta es la propiedad que contendrá el valor de la clave foránea.
        public int IdUsuario { get; set; }

        // --- INICIO DE LA CORRECCIÓN ---
        // Con esta anotación, le decimos a EF que la propiedad de navegación 'Usuario'
        // debe usar la columna especificada en 'IdUsuario' como su clave foránea.
        // Ya no creará una columna extra 'UsuarioId'.
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }
    }
}
