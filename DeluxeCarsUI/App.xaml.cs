using DeluxeCarsUI.Properties;
using DeluxeCarsUI.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;



namespace DeluxeCarsUI
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        //public static IHost AppHost { get; private set; }

        //public App()
        //{
        //    AppHost = Host.CreateDefaultBuilder()
        //        .ConfigureServices((context, services) =>
        //        {
        //            // Agrega tu cadena de conexión aquí
        //            services.AddDbContext<AppDbContext>(options =>
        //                options.UseSqlServer("Server=AEROPAD\\SQLEXPRESS;Database=MVVMLoginDb;Integrated Security=true"));

        //            // Agrega tus servicios o repositorios aquí
        //            services.AddScoped<IUserRepository, UserRepository>();

        //            // Puedes registrar ventanas o ViewModels también
        //            services.AddSingleton<MainView>();
        //            services.AddSingleton<LoginView>();
        //        })
        //        .Build();
        //}
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    AppHost.Start();

        //    var savedUsername = Settings.Default.SavedUsername;

        //    if (!string.IsNullOrEmpty(savedUsername))
        //    {
        //        Thread.CurrentPrincipal = new GenericPrincipal(
        //            new GenericIdentity(savedUsername), null);

        //        var mainView = AppHost.Services.GetRequiredService<MainView>();
        //        mainView.Show();
        //    }
        //    else
        //    {
        //        var loginView = AppHost.Services.GetRequiredService<LoginView>();
        //        loginView.Show();
        //    }

        //    base.OnStartup(e);
        //}

        //protected override async void OnExit(ExitEventArgs e)
        //{
        //    await AppHost.StopAsync();
        //    AppHost.Dispose();
        //    base.OnExit(e);
        //}

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