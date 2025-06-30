using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public interface INavigationService
    {
        // Propiedad para saber cuál es la vista actual en el panel principal
        ViewModelBase CurrentMainView { get; }

        // Propiedad para saber si podemos retroceder
        bool CanGoBack { get; }

        // Evento que se dispara cuando la vista principal cambia
        event Action CurrentMainViewChanged;

        // Método para navegar a una nueva vista principal (Genérico y Asíncrono)
        Task NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

        // Método para retroceder en el historial
        void GoBack();

        // Tu método original para abrir ventanas de formulario
        Task OpenFormWindow(FormType formType, int entityId = 0);
        Task OpenSugerenciasDialogAsync();
    }
}
