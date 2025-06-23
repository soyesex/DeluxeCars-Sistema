using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Models.Notifications;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationManager _notificationManager = new NotificationManager();

        // --> AÑADIDO: Campos para guardar las dependencias inyectadas
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        private readonly ObservableCollection<AppNotification> _notifications = new ObservableCollection<AppNotification>();
        public ReadOnlyObservableCollection<AppNotification> AllNotifications { get; }
        public event Action<AppNotification> OnNotificationPosted;

        // --> MODIFICADO: El constructor ahora recibe las nuevas dependencias
        public NotificationService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;

            AllNotifications = new ReadOnlyObservableCollection<AppNotification>(_notifications);
        }

        public void PostOrUpdateByKey(AppNotification notification)
        {
            // Buscamos en la lista EN MEMORIA si ya existe una notificación con la misma clave.
            var existingNotification = _notifications.FirstOrDefault(n => n.NotificationKey == notification.NotificationKey);

            if (existingNotification != null)
            {
                // SI EXISTE: No creamos una nueva. Actualizamos las propiedades de la existente.
                // Gracias a que AppNotification hereda de ViewModelBase, la UI se actualizará automáticamente.
                existingNotification.Title = notification.Title;
                existingNotification.Message = notification.Message;
                existingNotification.Type = notification.Type;
                existingNotification.Timestamp = DateTime.Now; // Actualizamos la fecha para que aparezca como nueva

                // Movemos la notificación actualizada al principio de la lista.
                _notifications.Move(_notifications.IndexOf(existingNotification), 0);
            }
            else
            {
                // SI NO EXISTE: La añadimos como una notificación normal.
                Show(notification);
            }
        }
        private async void Show(AppNotification content)
        {
            // 1. Lógica en memoria (para la UI en tiempo real, como antes)
            _notifications.Insert(0, content);
            OnNotificationPosted?.Invoke(content);

            // 2. Lógica de persistencia en la Base de Datos (la parte nueva)
            try
            {
                // Primero, nos aseguramos de que haya un usuario en sesión.
                if (_currentUserService.CurrentUser != null)
                {
                    var notificacionEntity = new Notificacion
                    {
                        Titulo = content.Title,
                        Mensaje = content.Message,
                        Tipo = content.Type.ToString(),
                        FechaCreacion = content.Timestamp,
                        Leida = false,
                        // La línea corregida: Accedemos al Id DENTRO del objeto CurrentUser
                        IdUsuario = _currentUserService.CurrentUser.Id
                    };

                    await _unitOfWork.Notificaciones.AddAsync(notificacionEntity);
                    await _unitOfWork.CompleteAsync();
                }
                else
                {
                    // Si no hay usuario, no guardamos en BD porque no sabríamos a quién asignarla.
                    // Simplemente lo registramos en la ventana de depuración.
                    Debug.WriteLine("No se guardó la notificación en BD: No hay usuario logueado.");
                }
            }
            catch (Exception ex)
            {
                // Si algo falla al guardar en la BD, no queremos que la app se rompa.
                // Lo registramos para poder depurarlo, pero la notificación visual seguirá apareciendo.
                Debug.WriteLine($"Error al guardar la notificación en la base de datos: {ex.Message}");
            }

            // 3. Mostrar el "toast" visual (como antes)
            await _notificationManager.ShowAsync(content, areaName: "WindowArea", expirationTime: TimeSpan.FromSeconds(5));
        }

        public void LoadInitialNotifications(IEnumerable<AppNotification> initialNotifications)
        {
            _notifications.Clear();
            foreach (var notification in initialNotifications)
            {
                _notifications.Add(notification);
            }
            // Disparamos el evento para cada una para que el contador en MainViewModel se actualice
            foreach (var notification in initialNotifications)
            {
                OnNotificationPosted?.Invoke(notification);
            }
        }

        public void ShowSuccess(string message, string title = "Éxito", ICommand actionCommand = null, string actionText = null)
        {
            // Esta versión ya no llama al método Show() que guarda en la base de datos.
            // Solo muestra el "toast" efímero.
            var content = new NotificationContent { Title = title, Message = message, Type = NotificationType.Success };
            _notificationManager.ShowAsync(content, areaName: "WindowArea", expirationTime: TimeSpan.FromSeconds(4));
        }
        public void ShowInfo(string message, string title = "Información", ICommand actionCommand = null, string actionText = null) =>
            Show(new AppNotification { Title = title, Message = message, Type = NotificationType.Information, Timestamp = DateTime.Now, ActionCommand = actionCommand, ActionText = actionText });

        public void ShowWarning(string message, string title = "Advertencia", ICommand actionCommand = null, string actionText = null) =>
            Show(new AppNotification { Title = title, Message = message, Type = NotificationType.Warning, Timestamp = DateTime.Now, ActionCommand = actionCommand, ActionText = actionText });

        public void ShowError(string message, string title = "Error", ICommand actionCommand = null, string actionText = null) =>
            Show(new AppNotification { Title = title, Message = message, Type = NotificationType.Error, Timestamp = DateTime.Now, ActionCommand = actionCommand, ActionText = actionText });
    }
}
