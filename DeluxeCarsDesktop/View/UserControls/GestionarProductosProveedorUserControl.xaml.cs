using DeluxeCarsDesktop.ViewModel;
using DeluxeCarsEntities;
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
    /// Lógica de interacción para GestionarProductosProveedorUserControl.xaml
    /// </summary>
    public partial class GestionarProductosProveedorUserControl : UserControl
    {
        public GestionarProductosProveedorUserControl()
        {
            InitializeComponent();
        }
        private async void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Verificamos si la edición fue confirmada (ej. con Enter) y no cancelada (ej. con Esc)
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Obtenemos el ViewModel del DataContext
                if (DataContext is GestionarProductosProveedorViewModel viewModel)
                {
                    // Obtenemos la fila (el objeto ProductoProveedor) que se editó
                    if (e.Row.Item is ProductoProveedor productoEditado)
                    {
                        // Llamamos al método del ViewModel para guardar el cambio en la DB
                        await viewModel.GuardarCambioPrecio(productoEditado);
                    }
                }
            }
        }

    }
}
