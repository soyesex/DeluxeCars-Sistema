using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeluxeCarsDesktop.Converters
{
    public class ViewModelTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' es el ViewModel actual (ej: DashboardViewModel)
            // 'parameter' es el nombre del ViewModel que queremos comprobar (ej: "DashboardViewModel")
            if (value == null || parameter == null)
                return false;

            // Comparamos el nombre de la clase del ViewModel actual con el nombre que pasamos como parámetro.
            // Devuelve 'true' si coinciden, 'false' si no.
            return value.GetType().Name == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No necesitamos la conversión inversa para este caso.
            return Binding.DoNothing;
        }
    }
}
