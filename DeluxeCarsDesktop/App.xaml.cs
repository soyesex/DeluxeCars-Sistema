using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Properties;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.View;
using DeluxeCarsDesktop.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Security.Principal;
using System.Windows;

namespace DeluxeCarsDesktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Campo para nuestro "Proveedor de Servicios"
    private readonly ServiceProvider _serviceProvider;

    //Contructor de la clase App
    public App()
    {
        var services = new ServiceCollection();

        // Debemos llamar a ConfigureServices y construir el provider DENTRO del constructor.
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    // ConfigureServices: Este es el "catalogo" de tu aplicacion 
    // Aquí le dices al sistema qué clases existen y cómo deben ser creadas.
    private void ConfigureServices(IServiceCollection services)
    {
        // Registrar el DataContext
        services.AddDbContext<AppDbContext>(options =>
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;
            options.UseSqlServer(connectionString);
        });

        // Registrar los repositorios y servicios necesarios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<INavigationService, NavigationService>();

        // Registrar tus ViewModels. Transient significa que se creará una nueva instancia cada vez que se pida.
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<RegistroViewModel>();
        services.AddTransient<RolViewModel>();
        services.AddTransient<UsuarioViewModel>();
        services.AddTransient<ClientesViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CatalogoViewModel>();
        services.AddTransient<ProveedorViewModel>();
        services.AddTransient<PedidoViewModel>();
        services.AddTransient<FacturacionViewModel>();
        services.AddTransient<ReportesViewModel>();
        services.AddTransient<ConfiguracionViewModel>();

        // Registra tus Vistas (las ventanas). Singleton o Transient son opciones comunes.
        // Usaremos Transient para asegurar que siempre se abran limpias.
        services.AddTransient<LoginView>();
        services.AddTransient<MainView>();
        services.AddTransient<RegistroView>();
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. PRIMERO, leemos el valor guardado. Esta es la única decisión que importa al arrancar.
        var savedUsername = Settings.Default.SavedUsername;

        // 2. ¿Existe un nombre de usuario guardado?
        if (!string.IsNullOrEmpty(savedUsername))
        {
            // SÍ -> Hay una sesión activa.

            // Establecemos la identidad del usuario para toda la aplicación.
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(savedUsername), null);

            // Pedimos la MainView al contenedor y la mostramos. FIN.
            var mainView = _serviceProvider.GetService<MainView>();
            mainView.Show();
        }
        else
        {
            // NO -> No hay sesión. Debemos mostrar la pantalla de login.

            // Pedimos la LoginView al contenedor.
            var loginView = _serviceProvider.GetService<LoginView>();

            // Obtenemos su ViewModel para poder suscribirnos a su evento de éxito.
            var loginViewModel = loginView.DataContext as LoginViewModel;

            if (loginViewModel != null)
            {
                // Nos suscribimos al evento. Esto prepara a la aplicación para reaccionar
                // CUANDO el usuario finalmente inicie sesión de forma correcta.
                loginViewModel.LoginSuccess += () =>
                {
                    var mainView = _serviceProvider.GetService<MainView>();
                    mainView.Show();
                    loginView.Close();
                };
            }

            // Mostramos la LoginView para que el usuario pueda iniciar sesión.
            loginView.Show();
        }
    }
}

