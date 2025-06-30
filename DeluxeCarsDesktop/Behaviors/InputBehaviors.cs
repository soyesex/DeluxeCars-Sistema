using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        #region SoloNumeros Behavior (Tu código original)
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
        #endregion

        #region ValidaEmail Behavior
        public static readonly DependencyProperty ValidaEmailProperty =
            DependencyProperty.RegisterAttached("ValidaEmail", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnValidaEmailChanged));

        public static bool GetValidaEmail(DependencyObject obj) => (bool)obj.GetValue(ValidaEmailProperty);
        public static void SetValidaEmail(DependencyObject obj, bool value) => obj.SetValue(ValidaEmailProperty, value);

        private static void OnValidaEmailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.LostFocus += ValidaEmail_LostFocus;
                }
                else
                {
                    textBox.LostFocus -= ValidaEmail_LostFocus;
                }
            }
        }

        private static void ValidaEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Si está vacío, no lo marcamos como error.
                TextFieldAssist.SetHasClearButton(textBox, false);
                return;
            }

            bool esValido = Regex.IsMatch(textBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

            // Usamos las propiedades de MaterialDesign para mostrar el error
            textBox.SetValue(ValidationAssist.HasErrorProperty, !esValido);
        }
        #endregion

        // =================================================================
        // INICIO: NUEVO BEHAVIOR PARA EJECUTAR UN COMANDO CON ENTER
        // =================================================================
        #region EnterEjecutaComando Behavior
        public static readonly DependencyProperty EnterEjecutaComandoProperty =
            DependencyProperty.RegisterAttached("EnterEjecutaComando", typeof(ICommand), typeof(InputBehaviors), new PropertyMetadata(null, OnEnterEjecutaComandoChanged));

        public static ICommand GetEnterEjecutaComando(DependencyObject obj) => (ICommand)obj.GetValue(EnterEjecutaComandoProperty);
        public static void SetEnterEjecutaComando(DependencyObject obj, ICommand value) => obj.SetValue(EnterEjecutaComandoProperty, value);

        private static void OnEnterEjecutaComandoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    element.KeyDown += EnterEjecutaComando_KeyDown;
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    element.KeyDown -= EnterEjecutaComando_KeyDown;
                }
            }
        }

        private static void EnterEjecutaComando_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is UIElement element)
            {
                var command = GetEnterEjecutaComando(element);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                    // Marcamos el evento como manejado para evitar que el Enter haga un salto de línea
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Formato de Teléfono (Estilo Colombia)
        public static readonly DependencyProperty FormatoTelefonoProperty =
            DependencyProperty.RegisterAttached("FormatoTelefono", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnFormatoTelefonoChanged));

        public static bool GetFormatoTelefono(DependencyObject obj) => (bool)obj.GetValue(FormatoTelefonoProperty);
        public static void SetFormatoTelefono(DependencyObject obj, bool value) => obj.SetValue(FormatoTelefonoProperty, value);

        private static void OnFormatoTelefonoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += FormatoTelefono_TextChanged;
                }
                else
                {
                    textBox.TextChanged -= FormatoTelefono_TextChanged;
                }
            }
        }

        private static void FormatoTelefono_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Evitamos un bucle infinito al modificar el texto
                textBox.TextChanged -= FormatoTelefono_TextChanged;

                string textoSinFormato = new string(textBox.Text.Where(char.IsDigit).ToArray());
                string textoFormateado = textoSinFormato;

                if (textoSinFormato.Length > 3 && textoSinFormato.Length <= 6)
                {
                    textoFormateado = $"{textoSinFormato.Substring(0, 3)} {textoSinFormato.Substring(3)}";
                }
                else if (textoSinFormato.Length > 6)
                {
                    textoFormateado = $"{textoSinFormato.Substring(0, 3)} {textoSinFormato.Substring(3, 3)} {textoSinFormato.Substring(6, Math.Min(4, textoSinFormato.Length - 6))}";
                }

                textBox.Text = textoFormateado;
                textBox.CaretIndex = textBox.Text.Length; // Mover el cursor al final

                // Volvemos a suscribir el evento
                textBox.TextChanged += FormatoTelefono_TextChanged;
            }
        }
        #endregion

        #region NUEVO BEHAVIOR PARA FORMATEAR MONEDA

        public static readonly DependencyProperty FormatoMonedaProperty =
            DependencyProperty.RegisterAttached("FormatoMoneda", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnFormatoMonedaChanged));

        public static bool GetFormatoMoneda(DependencyObject obj) => (bool)obj.GetValue(FormatoMonedaProperty);
        public static void SetFormatoMoneda(DependencyObject obj, bool value) => obj.SetValue(FormatoMonedaProperty, value);

        private static void OnFormatoMonedaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += FormatoMoneda_TextChanged;
                    // También necesitamos saber cuándo el usuario sale del campo
                    textBox.LostFocus += FormatoMoneda_LostFocus;
                }
                else
                {
                    textBox.TextChanged -= FormatoMoneda_TextChanged;
                    textBox.LostFocus -= FormatoMoneda_LostFocus;
                }
            }
        }

        private static void FormatoMoneda_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.TextChanged -= FormatoMoneda_TextChanged;

                // Limpiamos el texto de todo lo que no sea un dígito
                string textoLimpio = new string(textBox.Text.Where(char.IsDigit).ToArray());

                if (long.TryParse(textoLimpio, out long valor))
                {
                    // Creamos una cultura para el formato colombiano (punto para miles, coma para decimales)
                    // Para la visualización, usaremos un formato sin decimales.
                    var cultureInfo = new CultureInfo("es-CO");
                    cultureInfo.NumberFormat.CurrencyDecimalDigits = 0; // Sin decimales

                    textBox.Text = valor.ToString("C", cultureInfo);
                    textBox.CaretIndex = textBox.Text.Length;
                }
                else if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = string.Empty;
                }

                textBox.TextChanged += FormatoMoneda_TextChanged;
            }
        }

        private static void FormatoMoneda_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Opcional: Si quieres un formato final específico al salir
                string textoLimpio = new string(textBox.Text.Where(char.IsDigit).ToArray());
                if (long.TryParse(textoLimpio, out long valor))
                {
                    var cultureInfo = new CultureInfo("es-CO");
                    cultureInfo.NumberFormat.CurrencyDecimalDigits = 0;
                    textBox.Text = valor.ToString("C", cultureInfo);
                }
            }
        }
        #endregion

        #region SoloLetras Behavior
        public static readonly DependencyProperty SoloLetrasProperty =
            DependencyProperty.RegisterAttached("SoloLetras", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnSoloLetrasChanged));

        public static bool GetSoloLetras(DependencyObject obj) => (bool)obj.GetValue(SoloLetrasProperty);
        public static void SetSoloLetras(DependencyObject obj, bool value) => obj.SetValue(SoloLetrasProperty, value);

        private static void OnSoloLetrasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.PreviewTextInput += SoloLetras_PreviewTextInput;
                }
                else
                {
                    textBox.PreviewTextInput -= SoloLetras_PreviewTextInput;
                }
            }
        }

        private static void SoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite letras y espacios, pero no números ni la mayoría de símbolos.
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z\s]+$");
        }
        #endregion

        #region EnterNavegaAlSiguiente Behavior
        public static readonly DependencyProperty EnterNavegaAlSiguienteProperty =
            DependencyProperty.RegisterAttached("EnterNavegaAlSiguiente", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnEnterNavegaAlSiguienteChanged));

        public static bool GetEnterNavegaAlSiguiente(DependencyObject obj) => (bool)obj.GetValue(EnterNavegaAlSiguienteProperty);
        public static void SetEnterNavegaAlSiguiente(DependencyObject obj, bool value) => obj.SetValue(EnterNavegaAlSiguienteProperty, value);

        private static void OnEnterNavegaAlSiguienteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.KeyDown += Element_KeyDown;
                }
                else
                {
                    element.KeyDown -= Element_KeyDown;
                }
            }
        }

        private static void Element_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is UIElement element)
            {
                // Mueve el foco al siguiente control en el orden de tabulación
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        #endregion
    }
}
