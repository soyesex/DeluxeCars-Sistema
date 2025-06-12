using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeluxeCarsDesktop.View
{
    /// <summary>
    /// Lógica de interacción para FormularioView.xaml
    /// </summary>
    public partial class FormularioView : Window
    {
        public FormularioView(FormularioViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void pnlControlBar_MouseEnter_1(object sender, MouseEventArgs e)
        {

        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void pnlControlBar_MouseEnter_2(object sender, MouseEventArgs e)
        {

        }

        private void btnMaximize_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }
    }
}
