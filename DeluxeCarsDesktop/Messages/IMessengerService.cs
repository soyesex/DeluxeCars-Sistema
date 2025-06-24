using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Messages
{
    public interface IMessengerService
    {
        void Subscribe<TMessage>(Action<TMessage> action) where TMessage : class;
        void Publish<TMessage>(TMessage message) where TMessage : class;
    }
}
