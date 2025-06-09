using DeluxeCarsUI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace DeluxeCarsUI.ViewModel
{
    public class ProveedorViewModel : ViewModelBase
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
        public ICommand ShowProveedorFormCommand { get; }
        public ICommand ShowDepartamentosFormCommand { get; }

        public ProveedorViewModel() : this(new Services.NavigationService())
        { }
        public ProveedorViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ShowProveedorFormCommand = new ViewModelCommand(ExecuteShowProveedorFormCommand);
            ShowDepartamentosFormCommand = new ViewModelCommand(ExecuteShowDepartamentosFormCommand);
        }
        private void ExecuteShowDepartamentosFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Departamentos);
        }
        private void ExecuteShowProveedorFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Proveedor);
        }
    }
}
