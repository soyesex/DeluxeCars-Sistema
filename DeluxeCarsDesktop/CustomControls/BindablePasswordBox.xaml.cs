using DeluxeCarsDesktop.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeluxeCarsDesktop.CustomControls
{
    /// <summary>
    /// Lógica de interacción para BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
        // 1. Añadimos nuestro "amortiguador de sonido"
        private bool _isUpdating = false;

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(SecureString), typeof(BindablePasswordBox),
                // Añadimos un callback para cuando la propiedad cambia desde el ViewModel
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        public SecureString Password
        {
            get { return (SecureString)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
        }

        // Este método se ejecuta cuando el usuario escribe (el grito en la Pared A)
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            // 2. Solo gritamos a la otra pared si no estamos en medio de una actualización interna
            if (!_isUpdating)
            {
                // Actualizamos la propiedad del ViewModel
                Password = txtPassword.SecurePassword;
            }
        }

        // Este método se ejecuta cuando el ViewModel cambia (el grito en la Pared B)
        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BindablePasswordBox;
            if (control != null)
            {
                // 3. Activamos el "amortiguador" para que el siguiente eco sea ignorado
                control._isUpdating = true;

                // Actualizamos el PasswordBox visual. Si el nuevo valor es null, ponemos un string vacío.
                control.txtPassword.Password = (e.NewValue as SecureString)?.Unsecure() ?? string.Empty;

                // 4. Desactivamos el amortiguador, listos para el siguiente grito del usuario.
                control._isUpdating = false;
            }
        }
    }
}
