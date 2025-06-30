using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Messages
{
    public class MessengerService : IMessengerService
    {
        // Un diccionario para guardar las suscripciones.
        // La 'Key' es el tipo de mensaje (ej: InventarioCambiadoMessage).
        // El 'Value' es una lista de todas las acciones (métodos) que se deben ejecutar cuando ese mensaje se publica.
        private readonly Dictionary<Type, List<Action<object>>> _subscriptions = new Dictionary<Type, List<Action<object>>>();

        public void Subscribe<TMessage>(Action<TMessage> action) where TMessage : class
        {
            var messageType = typeof(TMessage);

            // Si es la primera vez que alguien se suscribe a este tipo de mensaje, creamos la lista.
            if (!_subscriptions.ContainsKey(messageType))
            {
                _subscriptions[messageType] = new List<Action<object>>();
            }

            // Añadimos la acción a la lista de suscripciones para ese tipo de mensaje.
            _subscriptions[messageType].Add(message => action(message as TMessage));
        }

        public void Publish<TMessage>(TMessage message) where TMessage : class
        {
            var messageType = typeof(TMessage);

            // Si nadie está suscrito a este tipo de mensaje, no hacemos nada.
            if (!_subscriptions.ContainsKey(messageType))
            {
                return;
            }

            // Hacemos una copia de la lista de acciones para evitar problemas si la colección cambia durante la ejecución.
            var actions = _subscriptions[messageType].ToList();

            foreach (var action in actions)
            {
                // IMPORTANTE PARA WPF:
                // Nos aseguramos de que la acción se ejecute en el hilo de la UI para evitar errores de concurrencia.
                App.Current.Dispatcher.Invoke(() => action(message));
            }
        }
    }
}
