using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DeluxeCarsDesktop.Converters
{
    public class NameToBrushConverter : IValueConverter
    {
        private readonly Brush[] _brushes = new Brush[]
        {
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B39DDB")), // Lavanda 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81D4FA")), // Azul Cielo
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A5D6A7")), // Menta
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAB91")), // Coral
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F48FB1")), // Rosa Suave
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80CBC4")), // Turquesa Pálido
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE082")), // Amarillo Pálido
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CE93D8")), // Lila
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name && !string.IsNullOrEmpty(name))
            {
                // Usamos el HashCode del nombre para elegir siempre el mismo color de la paleta.
                int index = Math.Abs(name.GetHashCode()) % _brushes.Length;
                return _brushes[index];
            }
            return Brushes.Gray; // Un color por defecto
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
