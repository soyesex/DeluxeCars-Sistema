using DeluxeCarsDesktop.Models.Notifications;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.Services
{
    public interface INotificationService
    {
        // ---> ASEGÚRATE QUE ESTA LÍNEA EXISTA TAL CUAL <---
        event Action<AppNotification> OnNotificationPosted;

        void PostOrUpdateByKey(AppNotification notification);
        ReadOnlyObservableCollection<AppNotification> AllNotifications { get; }
        void LoadInitialNotifications(IEnumerable<AppNotification> initialNotifications);
        void ShowSuccess(string message, string title = "Éxito", ICommand actionCommand = null, string actionText = null);
        void ShowInfo(string message, string title = "Información", ICommand actionCommand = null, string actionText = null);
        void ShowWarning(string message, string title = "Advertencia", ICommand actionCommand = null, string actionText = null);
        void ShowError(string message, string title = "Error", ICommand actionCommand = null, string actionText = null);
    }
}
