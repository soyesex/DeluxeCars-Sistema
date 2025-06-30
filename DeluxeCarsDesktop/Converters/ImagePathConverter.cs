using System;
using System.Collections.Generic;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageName = value as string;
            string imagePath = null;

            if (!string.IsNullOrEmpty(imageName))
            {
                // Intenta construir la ruta para una imagen de contenido (copiada en la carpeta /bin/)
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string potentialPath = Path.Combine(basePath, "Images", "Products", imageName);
                if (File.Exists(potentialPath))
                {
                    imagePath = potentialPath;
                }
            }

            // Si después de todo, no tenemos una ruta válida...
            if (string.IsNullOrEmpty(imagePath))
            {
                // ...usamos la ruta de Recurso para nuestra imagen por defecto.
                imagePath = "pack://application:,,,/Images/Products/placeholder.png";
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
