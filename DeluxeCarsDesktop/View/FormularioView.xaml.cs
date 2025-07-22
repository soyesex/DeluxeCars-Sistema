using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace DeluxeCarsDesktop.View
{
    /// <summary>
    /// Lógica de interacción para FormularioView.xaml
    /// </summary>
    public partial class FormularioView : Window
    {
        public FormularioView()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
        // Importación para poder mover la ventana
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        // --- MÉTODOS DE EVENTOS CORREGIDOS ---

        // Este método permite arrastrar la ventana
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        // Este método se asegura de que la ventana no se maximice por encima de la barra de tareas
        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        // Botón de cerrar
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Botón de minimizar
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Botón de maximizar (con el nombre corregido)
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                // Establecemos la altura máxima JUSTO ANTES de maximizar
                this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        // Se eliminan los otros métodos con nombres incorrectos que no estaban en uso
        // como Window_MouseDown_1, pnlControlBar_MouseEnter_2, etc.
    }
}

