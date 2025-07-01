using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeluxeCarsDesktop.View
{
    /// <summary>
    /// Lógica de interacción para RegistroView.xaml
    /// </summary>
    public partial class RegistroView : Window
    {
        private PasswordBox _pinPasswordBoxReference;
        public RegistroView()
        {
            InitializeComponent();

            // Nos suscribimos al evento del DataContext cuando la ventana ha cargado
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is RegistroViewModel vm)
                {
                    // Nos suscribimos al evento que creamos en el ViewModel
                    vm.FocusPinBoxRequested += OnFocusPinBoxRequested;
                }
            };
        }
        // Este método se ejecutará cuando el ViewModel pida el foco
        private void OnFocusPinBoxRequested()
        {
            // Usamos el operador '?' para asegurarnos de que no es nulo
            _pinPasswordBoxReference?.Focus();
        }

        private void PinPasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            // El 'sender' es el PasswordBox que acaba de cargarse.
            // Lo guardamos en nuestra variable para usarlo después.
            _pinPasswordBoxReference = sender as PasswordBox;
        }

        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }
    }
}
