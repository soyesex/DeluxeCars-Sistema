using DeluxeCarsUI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsUI.ViewModel
{
    public class PedidoViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private bool _isViewVisible = true;
        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // commands
        public ICommand ShowPedidoFormCommand { get; }

        public PedidoViewModel() : this(new Services.NavigationService())
        { }
        public PedidoViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ShowPedidoFormCommand = new ViewModelCommand(ExecuteShowPedidoFormCommand);
        }
        private void ExecuteShowPedidoFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Pedido);
        }
    }
}
