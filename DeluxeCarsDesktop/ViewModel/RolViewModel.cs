using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RolViewModel : ViewModelBase
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
        public ICommand ShowRolFormCommand { get; }

        public RolViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ShowRolFormCommand = new ViewModelCommand(ExecuteShowRolFormCommand);
        }
        private void ExecuteShowRolFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Rol);
        }
    }
}
