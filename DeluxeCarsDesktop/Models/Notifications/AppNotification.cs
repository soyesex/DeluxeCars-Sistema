using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.Models.Notifications
{
    // Nuestra clase "AppNotification" HEREDA de la clase "NotificationContent" de la librería.
    // Esto significa que automáticamente ya tiene Title, Message y Type.
    public class AppNotification : NotificationContent
    {
        public int Id { get; set; }
        // Y aquí le añadimos la propiedad que NOSOTROS necesitamos.
        public DateTime Timestamp { get; set; }

        /*
         El texto que se mostrará en el botón de acción de la notificación (ej: "Ver Detalles").
         Si es nulo o vacío, no se mostrará ningún botón.*/
        public string ActionText { get; set; }

        // Una clave única para identificar notificaciones de resumen, como "LowStockSummary".
        public string? NotificationKey { get; set; }
        public bool IsStatusPanel { get; set; } = false;

        /* El comando que se ejecutará cuando el usuario presione el botón de acción.
         Lo ignoramos en la serialización porque no se puede convertir a JSON.*/

        [JsonIgnore]
        public ICommand ActionCommand { get; set; }
    }
}
