using DeluxeCarsUI.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DeluxeCarsUI.Properties;
using System.Threading;
using System.Security.Principal;



namespace DeluxeCarsUI
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            // Leer el username guardado en Settings
            var savedUsername = Settings.Default.SavedUsername; 

            if (!string.IsNullOrEmpty(savedUsername))
            {
                // Si existe, establece la identidad en el hilo
                Thread.CurrentPrincipal = new GenericPrincipal(
                    new GenericIdentity(savedUsername), null);

                // Abre MainView directamente
                var mainView = new MainView();
                mainView.Show();
            }
            else
            {
                // Si no hay usuario guardado, muestra LoginView
                var loginView = new LoginView();
                loginView.Show();
                
            }

            
        }
    }
}