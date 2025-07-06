using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DeluxeCarsDesktop.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        private static readonly BitmapImage _placeholderImage = CreatePlaceholderImage();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' ahora es la ruta relativa, ej: "images/products/archivo.jpg"
            string rutaRelativa = value as string;

            if (string.IsNullOrEmpty(rutaRelativa))
            {
                return _placeholderImage;
            }

            try
            {
                // --- CAMBIO 1: Leer la ruta base desde App.config ---
                string rutaBase = ConfigurationManager.AppSettings["RutaBaseImagenes"];
                if (string.IsNullOrEmpty(rutaBase)) return _placeholderImage;

                // --- CAMBIO 2: Construir la ruta completa ---
                // Reemplazamos las barras '/' por '\' para compatibilidad con Windows
                string rutaCompleta = Path.Combine(rutaBase, rutaRelativa.Replace('/', '\\'));

                if (File.Exists(rutaCompleta))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(rutaCompleta);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Libera el archivo después de cargarlo
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar la imagen '{rutaRelativa}': {ex.Message}");
                return _placeholderImage;
            }

            return _placeholderImage;
        }

        private static BitmapImage CreatePlaceholderImage()
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            // Esta es la ruta a un recurso incrustado en tu aplicación.
            bitmap.UriSource = new Uri("pack://application:,,,/Images/Products/placeholder.png");
            bitmap.EndInit();
            bitmap.Freeze(); // Mejora el rendimiento para imágenes que no cambian.
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
