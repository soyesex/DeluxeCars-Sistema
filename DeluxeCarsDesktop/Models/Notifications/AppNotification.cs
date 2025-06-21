using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models.Notifications
{
    // Nuestra clase "AppNotification" HEREDA de la clase "NotificationContent" de la librería.
    // Esto significa que automáticamente ya tiene Title, Message y Type.
    public class AppNotification : NotificationContent
    {
        public int Id { get; set; }
        // Y aquí le añadimos la propiedad que NOSOTROS necesitamos.
        public DateTime Timestamp { get; set; }
    }
}
