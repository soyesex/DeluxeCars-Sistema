using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DeluxeCarsDesktop.Behaviors
{
    public static class DeselectOnOutsideClickBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DeselectOnOutsideClickBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    window.PreviewMouseLeftButtonDown += Window_PreviewMouseLeftButtonDown;
                }
                else
                {
                    window.PreviewMouseLeftButtonDown -= Window_PreviewMouseLeftButtonDown;
                }
            }
        }

        /// <summary>
        /// Lógica principal que se ejecuta con cada clic en la ventana.
        /// </summary>
        private static void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as DependencyObject;

            // --- EXCEPCIONES ---
            // Si el clic fue en un control que no debe causar la pérdida de foco, salimos.
            // Esto evita que al hacer clic en un botón de acción, se deseleccione un grid.
            if (ShouldIgnoreClick(clickedElement))
            {
                return;
            }

            // Obtenemos el elemento que actualmente tiene el foco.
            var focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement == null)
            {
                return; // No hay nada enfocado, no hay nada que hacer.
            }

            // Si el clic ocurrió dentro del elemento que ya tiene el foco, no hacemos nada.
            if (IsDescendantOf(clickedElement, focusedElement))
            {
                return;
            }

            // --- ACCIÓN ---
            // Si llegamos aquí, el clic fue "fuera" de un control con foco.

            // 1. Quitar el foco del teclado. Esto funciona para TextBox, ComboBox, etc.
            Keyboard.ClearFocus();

            // 2. Específicamente para DataGrids, nos aseguramos de quitar la selección.
            if (sender is DependencyObject window)
            {
                foreach (var dataGrid in FindVisualChildren<DataGrid>(window))
                {
                    dataGrid.UnselectAll();
                    // Opcional: si usas SelectedItem en lugar de UnselectAll
                    // dataGrid.SelectedItem = null; 
                }
            }
        }

        /// <summary>
        /// Determina si el clic se realizó sobre un control que debe ser ignorado por este comportamiento.
        /// </summary>
        private static bool ShouldIgnoreClick(DependencyObject clickedElement)
        {
            // ✅ AÑADE ESTA LÍNEA AL PRINCIPIO DE LAS COMPROBACIONES
            // Esto asegura que si hacemos clic en un item de la lista desplegable, se ignore.
            if (FindAncestor<ComboBoxItem>(clickedElement) != null) return true;

            // --- El resto de tus comprobaciones existentes ---
            if (FindAncestor<ButtonBase>(clickedElement) != null) return true;
            if (FindAncestor<ScrollBar>(clickedElement) != null) return true;
            if (FindAncestor<Thumb>(clickedElement) != null) return true;
            if (FindAncestor<DataGridColumnHeader>(clickedElement) != null) return true;
            if (FindAncestor<Popup>(clickedElement) != null) return true;
            return false;
        }

        /// <summary>
        /// ✅ MÉTODO CORREGIDO: Ahora maneja elementos no visuales como 'Run'.
        /// </summary>
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null) return null;

            // Si el elemento no es visual (ej. un Run), sube por el árbol lógico hasta encontrar uno que sí lo sea.
            while (current != null && !(current is Visual) && !(current is Visual3D))
            {
                current = LogicalTreeHelper.GetParent(current);
            }

            // Ahora 'current' es un elemento visual (o null) y podemos recorrer el árbol visual de forma segura.
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static bool IsDescendantOf(DependencyObject descendant, DependencyObject ancestor)
        {
            if (descendant == null || ancestor == null) return false;
            if (descendant == ancestor) return true;
            return FindAncestor<DependencyObject>(descendant) == ancestor;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T tChild) yield return tChild;
                foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
            }
        }
    }
}
