using DeluxeCarsDesktop.Models.Notifications;
using System.Collections.ObjectModel;
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
