using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeluxeCarsDesktop.Behaviors
{
    public static class InputBehaviors
    {
        // Definimos una "Propiedad Adjunta" que podremos usar en cualquier TextBox.
        public static readonly DependencyProperty SoloNumerosProperty =
            DependencyProperty.RegisterAttached(
                "SoloNumeros",
                typeof(bool),
                typeof(InputBehaviors),
                new PropertyMetadata(false, OnSoloNumerosChanged));

        public static bool GetSoloNumeros(DependencyObject obj)
        {
            return (bool)obj.GetValue(SoloNumerosProperty);
        }

        public static void SetSoloNumeros(DependencyObject obj, bool value)
        {
            obj.SetValue(SoloNumerosProperty, value);
        }

        // Este método se ejecuta cuando aplicamos la propiedad en XAML.
        private static void OnSoloNumerosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    // Si se activa, nos suscribimos a los eventos del TextBox.
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                    DataObject.AddPastingHandler(textBox, TextBox_Pasting);
                }
                else
                {
                    // Si se desactiva, nos damos de baja de los eventos.
                    textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                    DataObject.RemovePastingHandler(textBox, TextBox_Pasting);
                }
            }
        }

        // Este evento se dispara cada vez que el usuario presiona una tecla.
        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Usamos una expresión regular para verificar si el texto es un número.
            // Si no lo es, marcamos el evento como "manejado" para que no se escriba la letra.
            e.Handled = !IsTextAllowed(e.Text);
        }

        // Este evento se dispara cuando el usuario intenta pegar texto.
        private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    // Si el texto pegado no son solo números, cancelamos la operación de pegado.
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Expresión regular que solo permite dígitos del 0 al 9.
        private static readonly Regex _regex = new Regex("[^0-9]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}
