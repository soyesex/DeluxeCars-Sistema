using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class FacturacionViewModel : ViewModelBase
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
        public ICommand ShowServicioFormCommand { get; }
        public ICommand ShowMetodoPagoFormCommand { get; }
        public ICommand ShowDetallesFacturaFormCommand { get; }
        public ICommand ShowFacturaFormCommand { get; }
        public FacturacionViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            ShowServicioFormCommand = new ViewModelCommand(ExecuteShowServicioFormCommand);

            ShowMetodoPagoFormCommand = new ViewModelCommand(ExecuteShowMetodoPagoFormCommand);

            ShowDetallesFacturaFormCommand = new ViewModelCommand(ExecuteShowFacturaFormCommand);

            ShowFacturaFormCommand = new ViewModelCommand(ExecuteShowDepartamentosFormCommand);
        }
        private void ExecuteShowServicioFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Servicio);
        }
        private void ExecuteShowMetodoPagoFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.MetodoPago);
        }
        private void ExecuteShowFacturaFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Factura);
        }
        private void ExecuteShowDepartamentosFormCommand(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.DetallesFactura);
        }
    }
}
