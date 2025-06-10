using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class UsuarioViewModel : ViewModelBase
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
        public ICommand ShowUsuarioFormCommand { get; }
        public ICommand ShowCambiarPasswordFormCommand { get; }

        public UsuarioViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ShowUsuarioFormCommand = new ViewModelCommand(ExecuteShowUsuarioFormCommand);
            ShowCambiarPasswordFormCommand = new ViewModelCommand(ExecuteShowCambiarPasswordFormCommand);
        }
        private void ExecuteShowUsuarioFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Usuario);
        }
        private void ExecuteShowCambiarPasswordFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.CambiarPassword);
        }
    }
}
