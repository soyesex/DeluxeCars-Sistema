using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    /// <summary>
    /// Define un contrato para un ViewModel que puede ser inicializado con un parámetro.
    /// </summary>
    /// <typeparam name="TParameter">El tipo del parámetro que el ViewModel espera.</typeparam>
    public interface IParameterReceiver<TParameter>
    {
        /// <summary>
        /// Método que el NavigationService llamará para pasar el parámetro al ViewModel.
        /// </summary>
        void LoadWithParameter(TParameter parameter);
    }
}
