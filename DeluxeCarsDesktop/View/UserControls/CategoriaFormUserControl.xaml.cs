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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeluxeCarsDesktop.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para CategoriaFormUserControl.xaml
    /// </summary>
    public partial class CategoriaFormUserControl : UserControl
    {
        public CategoriaFormUserControl()
        {
            InitializeComponent();
        }


        //private void DeselectOnEmptySpaceClick(object sender, MouseButtonEventArgs e)
        //{
        //    // Usamos HitTest para determinar exactamente sobre qué se hizo clic.
        //    // Esto es más fiable que e.OriginalSource en plantillas complejas.
        //    var hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
        //    if (hitTestResult == null) return;

        //    var clickedElement = hitTestResult.VisualHit as DependencyObject;

        //    // Buscamos hacia arriba desde el elemento clickeado si es parte de una fila.
        //    // Si encontramos una fila, significa que el clic fue en un item y no hacemos nada.
        //    if (FindAncestor<DataGridRow>(clickedElement) != null)
        //    {
        //        return;
        //    }

        //    // Si el código llega aquí, el clic no fue en una fila. Procedemos a deseleccionar.
        //    CategoriasDataGrid.SelectedItem = null;
        //    Keyboard.ClearFocus();
        //}

        //// Método auxiliar para buscar un ancestro (padre) en el árbol visual.
        //private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        //{
        //    while (current != null)
        //    {
        //        if (current is T ancestor)
        //        {
        //            return ancestor;
        //        }
        //        current = VisualTreeHelper.GetParent(current);
        //    }
        //    return null;
        //}
    }
}
