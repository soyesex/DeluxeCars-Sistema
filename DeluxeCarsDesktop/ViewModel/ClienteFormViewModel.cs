using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ClienteFormViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public ClienteFormViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public ICommand ShowClienteViewCommand;

    }
}
