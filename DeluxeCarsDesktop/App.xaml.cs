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
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();

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

        // User Controls Hijos
        services.AddTransient<CategoriaFormViewModel>();

        // Registra tus Vistas (las ventanas). Singleton o Transient son opciones comunes.
        // Usaremos Transient para asegurar que siempre se abran limpias.
        services.AddTransient<LoginView>();
        services.AddTransient<MainView>();
        services.AddTransient<RegistroView>();
        services.AddTransient<FormularioView>();
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
            ShowMainViewAndSubscribe();
        }
        else
        {
            ShowLoginView();
        }
    }

    // MÉTODO AUXILIAR PARA MOSTRAR LA VISTA DE LOGIN
    private void ShowLoginView()
    {
        var loginView = _serviceProvider.GetService<LoginView>();
        var loginViewModel = loginView.DataContext as LoginViewModel;

        if (loginViewModel != null)
        {
            // Cuando el login es exitoso...
            loginViewModel.LoginSuccess += () =>
            {
                // ... cerramos el login y mostramos el MainView
                ShowMainViewAndSubscribe();
                loginView.Close();
            };
        }
        loginView.Show();
    }

    // MÉTODO AUXILIAR PARA MOSTRAR LA VISTA PRINCIPAL Y SUSCRIBIR SU EVENTO DE LOGOUT
    private void ShowMainViewAndSubscribe()
    {
        var mainView = _serviceProvider.GetService<MainView>();
        var mainViewModel = mainView.DataContext as MainViewModel;

        if (mainViewModel != null)
        {
            // ¡AQUÍ ESTÁ LA NUEVA LÓGICA!
            // Nos suscribimos al evento de Logout.
            mainViewModel.LogoutSuccess += () =>
            {
                // Cuando el logout es exitoso...
                // ... cerramos el MainView y mostramos el LoginView de nuevo.
                ShowLoginView();
                mainView.Close();
            };
        }
        mainView.Show();
    }
}

