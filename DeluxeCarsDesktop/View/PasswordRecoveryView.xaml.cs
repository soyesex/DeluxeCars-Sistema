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
    /// Lógica de interacción para PasswordRecoveryView.xaml
    /// </summary>
    public partial class PasswordRecoveryView : Window
    {
        public PasswordRecoveryView()
        {
            InitializeComponent();
        }
        // --- LÓGICA PARA LA VENTANA PERSONALIZADA ---
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
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
            this.Close();
        }

        // Esto es necesario para evitar un bug de WPF con el DragMove en la barra de título
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
