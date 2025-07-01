using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class CambiarPinDialogViewModel : ViewModelBase
    {
        // Acciones que el ViewModel padre (ConfiguracionViewModel) nos pasará.
        private readonly Func<string, string, string, Task> _accionGuardar;
        private readonly Action _accionCancelar;

        // --- Propiedades para el Binding ---
        private string _pinActual;
        public string PinActual { get => _pinActual; set => SetProperty(ref _pinActual, value); }

        private string _nuevoPin;
        public string NuevoPin { get => _nuevoPin; set => SetProperty(ref _nuevoPin, value); }

        private string _confirmarPin;
        public string ConfirmarPin { get => _confirmarPin; set => SetProperty(ref _confirmarPin, value); }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        public CambiarPinDialogViewModel(Func<string, string, string, Task> accionGuardar, Action accionCancelar)
        {
            _accionGuardar = accionGuardar;
            _accionCancelar = accionCancelar;

            GuardarCommand = new ViewModelCommand(async _ => await ExecuteGuardar(), CanExecuteGuardar);
            CancelarCommand = new ViewModelCommand(_ => ExecuteCancelar());
        }

        private bool CanExecuteGuardar(object obj)
        {
            // El botón de guardar solo se activa si todos los campos están llenos.
            return !string.IsNullOrEmpty(PinActual) &&
                   !string.IsNullOrEmpty(NuevoPin) &&
                   !string.IsNullOrEmpty(ConfirmarPin);
        }

        private void ExecuteCancelar()
        {
            _accionCancelar?.Invoke();
        }

        private async Task ExecuteGuardar()
        {
            ErrorMessage = ""; // Limpiamos errores previos

            if (NuevoPin != ConfirmarPin)
            {
                ErrorMessage = "El nuevo PIN y su confirmación no coinciden.";
                return;
            }

            if (NuevoPin.Length < 4) // Una validación de ejemplo
            {
                ErrorMessage = "El nuevo PIN debe tener al menos 4 dígitos.";
                return;
            }

            // Si las validaciones básicas pasan, llamamos a la lógica principal
            // que nos pasó el ConfiguracionViewModel.
            await _accionGuardar?.Invoke(PinActual, NuevoPin, ConfirmarPin);
        }
    }
}
