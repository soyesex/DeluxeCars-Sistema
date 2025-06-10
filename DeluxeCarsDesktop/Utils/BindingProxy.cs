using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeluxeCarsDesktop.Utils
{
    public class BindingProxy : Freezable
    {
        // Sobrescribimos este método solo para cumplir con los requisitos de la clase Freezable.
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        // Esta es la propiedad clave. La usaremos para "capturar" y "mantener"
        // el DataContext de la ventana principal.
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Definimos 'Data' como una DependencyProperty para que pueda ser el objetivo
        // de un enlace de datos (Binding).
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
