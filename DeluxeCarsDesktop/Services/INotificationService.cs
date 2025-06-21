using DeluxeCarsDesktop.Models.Notifications;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public interface INotificationService
    {
        // ---> ASEGÚRATE QUE ESTA LÍNEA EXISTA TAL CUAL <---
        event Action<AppNotification> OnNotificationPosted;

        // ---> Y ASEGÚRATE QUE ESTA PROPIEDAD TAMBIÉN <---
        ReadOnlyObservableCollection<AppNotification> AllNotifications { get; }
        void LoadInitialNotifications(IEnumerable<AppNotification> initialNotifications);
        void ShowSuccess(string message);
        void ShowInfo(string message);
        void ShowWarning(string message);
        void ShowError(string message);
    }
}
