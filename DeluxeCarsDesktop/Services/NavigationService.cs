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

        public void OpenFormWindow(FormType formType)
        {
            // 1. Crea el ViewModel, pasándole el tipo y el proveedor de servicios.
            var viewModel = new FormularioViewModel(formType, _serviceProvider);

            // 2. Crea la Vista, pasándole el ViewModel que acabas de crear.
            var window = new FormularioView(viewModel);

            // 3. Muestra la ventana.
            window.ShowDialog();
        }
    }
}
