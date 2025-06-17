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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeluxeCarsDesktop.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para DashboardUserControl.xaml
    /// </summary>
    public partial class DashboardUserControl : UserControl
    {
        public DashboardUserControl()
        {
            InitializeComponent();
        }
        private async void DashboardUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Verificamos que el DataContext sea nuestro ViewModel
            if (this.DataContext is DashboardViewModel viewModel)
            {
                // ¡Llamamos aquí al método de carga!
                await viewModel.LoadAsync();
            }
        }
    }
}
