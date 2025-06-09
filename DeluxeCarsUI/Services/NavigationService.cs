using DeluxeCarsUI.Utils;
using DeluxeCarsUI.View;
using DeluxeCarsUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsUI.Services
{
    public class NavigationService : INavigationService
    {
        public void OpenFormWindow(FormType formType)
        {
            var viewModel = new FormularioViewModel(formType);
            var window = new FormularioView(formType);
            window.ShowDialog();
        }
    }
}
