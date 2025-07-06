using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // (Opcional pero recomendado) Helper en ViewModelBase para no repetir OnPropertyChanged
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
        /// <summary>
        /// Asigna un valor a una propiedad y notifica el cambio, devolviendo 'true' si el valor cambió.
        /// </summary>
        /// <returns>True si el valor fue cambiado, de lo contrario False.</returns>
        protected bool SetPropertyAndCheck<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false; // No hubo cambio
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true; // Sí hubo un cambio
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        protected async Task ShowTemporaryErrorMessage(string message, int delayInSeconds = 5)
        {
            this.ErrorMessage = message;
            await Task.Delay(delayInSeconds * 1000);
            if (this.ErrorMessage == message)
            {
                this.ErrorMessage = string.Empty;
            }
        }
    }
}
