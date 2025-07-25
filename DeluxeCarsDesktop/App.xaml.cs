﻿using DeluxeCarsDesktop.Data;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsDesktop.Properties;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View;
using DeluxeCarsDesktop.ViewModel;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using System.Configuration;
using System.Diagnostics;
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

    private LoginView _loginView;

    //Contructor de la clase App
    public App()
    {
        QuestPDF.Settings.License = LicenseType.Community;

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
        }, ServiceLifetime.Scoped);  

        // AHORA (El nuevo modo, registrando todo de una vez con Unit of Work)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IStockAlertService, StockAlertService>();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        services.AddTransient<IEmailService, EmailService>();

        services.AddSingleton<INotificationService, NotificationService>();

        services.AddSingleton<IMessengerService, MessengerService>();

        services.AddTransient<IPedidoStatusCheckService, PedidoStatusCheckService>();

        services.AddSingleton<IPinLockoutService, PinLockoutService>();

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
        services.AddTransient<FacturacionViewModel>(); // El POS
        services.AddTransient<FacturasHistorialViewModel>(); // El historial
        services.AddTransient<ReportesViewModel>();
        services.AddTransient<ConfiguracionViewModel>();
        services.AddTransient<GestionarProductosProveedorViewModel>();
        services.AddTransient<PasswordRecoveryViewModel>();

        // User Controls Hijos
        services.AddTransient<ProductoFormViewModel>();
        services.AddTransient<CategoriaFormViewModel>();
        services.AddTransient<ClienteFormViewModel>();
        services.AddTransient<DepartamentoFormViewModel>();
        services.AddTransient<DetalleFacturaFormViewModel>();
        services.AddTransient<FacturasHistorialViewModel>();
        services.AddTransient<MetodoPagoFormViewModel>();
        services.AddTransient<MunicipioFormViewModel>();
        services.AddTransient<PedidoFormViewModel>();
        services.AddTransient<ProveedorFormViewModel>();
        services.AddTransient<RolFormViewModel>();
        services.AddTransient<ServicioFormViewModel>();
        services.AddTransient<UsuarioFormViewModel>();
        services.AddTransient<SugerenciasCompraViewModel>();
        services.AddTransient<RecepcionPedidoViewModel>();
        services.AddTransient<RegistrarPagoProveedorViewModel>();
        services.AddTransient<ReportesRentabilidadViewModel>();
        services.AddTransient<RegistrarPagoClienteViewModel>();
        services.AddTransient<NotaDeCreditoViewModel>();
        services.AddTransient<ConfiguracionViewModel>();

        services.AddSingleton<ISnackbarMessageQueue>(new SnackbarMessageQueue());

        // Registra tus Vistas (las ventanas). Singleton o Transient son opciones comunes.
        // Usaremos Transient para asegurar que siempre se abran limpias.
        services.AddTransient<LoginView>();
        services.AddTransient<MainView>();
        services.AddTransient<RegistroView>();
        services.AddTransient<FormularioView>();
        services.AddTransient<PasswordRecoveryView>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // =====================================================================
        //           INICIO: CÓDIGO TEMPORAL PARA GENERAR PIN
        // =====================================================================
        #if DEBUG // Esto asegura que el código solo se compile en modo Debug
        // Cambia a 'if (false)' después de obtener los valores para que no se ejecute más.
        if (false)
        {
            // Asegúrate de que la clase PasswordHelper sea accesible aquí
            PasswordHelper.CreatePasswordHash("0000", out byte[] hash, out byte[] salt);

            string hashString = $"new byte[] {{ {string.Join(", ", hash)} }}";
            string saltString = $"new byte[] {{ {string.Join(", ", salt)} }}";

            Debug.WriteLine("--- VALORES DE PIN GENERADOS ---");
            Debug.WriteLine("Copia las siguientes líneas en tu AppDbContext.cs dentro del método OnModelCreating:\n");
            Debug.WriteLine("// HASH para el PIN '0000':");
            Debug.WriteLine(hashString);
            Debug.WriteLine("\n// SALT para el PIN '0000':");
            Debug.WriteLine(saltString);
            Debug.WriteLine("\n---------------------------------");

            // Detenemos la aplicación para no continuar.
            Application.Current.Shutdown();
            return;
        }
        #endif
        // =====================================================================
        //            FIN: CÓDIGO TEMPORAL PARA GENERAR PIN
        // =====================================================================

        base.OnStartup(e);

        var savedUsername = Settings.Default.SavedUsername;

        // Si hay un usuario guardado, lo validamos contra la base de datos
        if (!string.IsNullOrEmpty(savedUsername))
        {
            bool isAuthenticated = false;

            // Creamos un "scope" para usar nuestros servicios de forma segura
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.Usuarios.GetUserByEmail(savedUsername);

                // El inicio de sesión automático solo es válido si el usuario existe Y está activo
                if (user != null && user.Activo)
                {
                    isAuthenticated = true;
                    // Establecemos la identidad del usuario para la sesión actual
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(savedUsername), null);
                }
            }

            if (isAuthenticated)
            {
                // Si la validación es exitosa, mostramos la ventana principal
                ShowMainViewAndSubscribe();
            }
            else
            {
                // Si el usuario guardado no es válido, borramos el setting y mostramos el login
                Settings.Default.SavedUsername = string.Empty;
                Settings.Default.Save();
                ShowLoginView();
            }
        }
        else
        {
            // Si no hay ningún usuario guardado, vamos al login
            ShowLoginView();
        }
    }

    // MÉTODO AUXILIAR PARA MOSTRAR LA VISTA DE LOGIN
    private void ShowLoginView()
    {
        _loginView = _serviceProvider.GetService<LoginView>(); // Asignamos la instancia al campo de la clase
        var loginViewModel = _serviceProvider.GetService<LoginViewModel>(); // Obtenemos el ViewModel
        _loginView.DataContext = loginViewModel; // Asignamos el DataContext

        if (loginViewModel != null)
        {
            // Suscripción para cuando el login es exitoso
            loginViewModel.LoginSuccess += () =>
            {
                ShowMainViewAndSubscribe();
                _loginView.Close(); // Cerramos la ventana de login
            };

            // =================================================================
            // INICIO DEL CAMBIO: Suscripción para la navegación al registro
            // =================================================================
            loginViewModel.ShowRegisterViewRequested += OnShowRegisterViewRequested;
        }
        _loginView.Show();
    }

    private async void OnShowRegisterViewRequested()
    {
        if (_loginView != null)
        {
            _loginView.IsEnabled = false;
        }

        // Creamos las instancias de la vista y el viewmodel de registro
        var registerView = _serviceProvider.GetRequiredService<RegistroView>();
        var registerViewModel = _serviceProvider.GetRequiredService<RegistroViewModel>();
        registerView.DataContext = registerViewModel;

        // Esperamos a que el ViewModel de registro cargue sus datos (como los roles)
        await registerViewModel.InitializeAsync();

        // Nos suscribimos a su evento de cierre para saber cuándo volver
        registerViewModel.CloseAction += () =>
        {
            registerView.Close(); // Cierra la ventana de registro

            // Volvemos a habilitar la ventana de login
            if (_loginView != null)
            {
                _loginView.IsEnabled = true;
                _loginView.Activate(); // Nos aseguramos de que vuelva al frente
            }
        };

        // Paso 2: Mostramos la ventana de registro con Show(), que no es bloqueante.
        registerView.Show();
    }

    // MÉTODO AUXILIAR PARA MOSTRAR LA VISTA PRINCIPAL Y SUSCRIBIR SU EVENTO DE LOGOUT
    // --- MÉTODO CORREGIDO ---
    private async void ShowMainViewAndSubscribe()
    {
        // 1. Obtenemos el ViewModel primero.
        var mainViewModel = _serviceProvider.GetService<MainViewModel>();
        // 2. Obtenemos la Vista.
        var mainView = _serviceProvider.GetService<MainView>();

        // 3. ¡EL PASO CLAVE! Asignamos el ViewModel al DataContext de la Vista.
        mainView.DataContext = mainViewModel;

        // 4. Ahora la suscripción al evento funciona correctamente.
        if (mainViewModel != null)
        {
            mainViewModel.LogoutSuccess += () =>
            {
                ShowLoginView();
                mainView.Close();
            };
        }
        mainView.Show();
        await mainViewModel.InitializeAsync();

        // --- ESTE ES EL BLOQUE QUE EJECUTA EL SERVICIO ---
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var pedidoCheckService = scope.ServiceProvider.GetRequiredService<IPedidoStatusCheckService>();
                await pedidoCheckService.CheckPendingPedidosAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ejecutando servicios de arranque: {ex.Message}");
            }
        }
    }
}

