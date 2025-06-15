using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View;
using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public class NavigationService  : INavigationService
    {

        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OpenFormWindow(FormType formType, int entityId = 0)
        {
            // 1. Crea la ventana anfitriona.
            var formWindow = new FormularioView();

            // 2. Crea el ViewModel anfitrión con 'new', porque necesita parámetros.
            // Esta es la corrección clave para el error CS1501.
            var formViewModel = new FormularioViewModel(formType, _serviceProvider);

            // 3. Inicializamos el ViewModel "hijo" que se creó dentro.
            if (formViewModel.CurrentChildView is IFormViewModel childWithLoad)
            {
                await childWithLoad.LoadAsync(entityId);
            }

            // 4. Conectamos la acción de cierre.
            if (formViewModel.CurrentChildView is ICloseable childWithClose)
            {
                childWithClose.CloseAction = formWindow.Close;
            }

            // 5. Asignamos el DataContext y mostramos la ventana.
            formWindow.DataContext = formViewModel;
            formWindow.ShowDialog();
        }

    }
}
