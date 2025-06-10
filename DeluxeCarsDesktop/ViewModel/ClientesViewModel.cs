using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ClientesViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        // Commands
        public ICommand ShowClienteFormCommand { get; }
        public ClientesViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ShowClienteFormCommand = new ViewModelCommand(ExecuteNuevoCliente);
        }

        private void ExecuteNuevoCliente(object parameter)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Cliente);
        }
    }
}
