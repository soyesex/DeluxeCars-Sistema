using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    // Para ViewModels que NO necesitan parámetros para cargar.
    public interface IAsyncLoadable
    {
        Task LoadAsync();
    }
}
