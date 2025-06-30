using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public static class AppEvents
    {
        public static event Action? OnNotificationsChanged;
        public static void RaiseNotificationsChanged() => OnNotificationsChanged?.Invoke();
    }
}
