using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Properties;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.View;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        // --- Dependencias y Estado ---
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO: Ahora usamos UnitOfWork
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;

        // Nueva propiedad para el binding en XAML
        public bool IsAdmin => _currentUserService.IsAdmin;

        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        public event Action LogoutSuccess;

        // --- Propiedades Públicas para Binding ---
        public UserAccountModel CurrentUserAccount { get => _currentUserAccount; set => SetProperty(ref _currentUserAccount, value); }
        public ViewModelBase CurrentChildView { get => _currentChildView; set => SetProperty(ref _currentChildView, value); }
        public string Caption { get => _caption; set => SetProperty(ref _caption, value); }
        public IconChar Icon { get => _icon; set => SetProperty(ref _icon, value); }

        // --- Comandos ---
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowCatalogoViewCommand { get; }
        public ICommand ShowClienteViewCommand { get; }
        public ICommand ShowProveedorViewCommand { get; }
        public ICommand ShowComprasViewCommand { get; }
        public ICommand ShowPuntoDeVentaCommand { get; }
        public ICommand ShowHistorialVentasCommand { get; }
        public ICommand ShowReportesViewCommand { get; }
        public ICommand ShowUsuarioViewCommand { get; }
        public ICommand ShowRolViewCommand { get; }
        public ICommand ShowConfiguracionViewCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IServiceProvider serviceProvider, INavigationService navigationService)
        {
            // --- Inyección de Dependencias (CAMBIO) ---
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            _currentUserService = currentUserService;

            // --- Inicialización de Comandos ---
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowCatalogoViewCommand = new ViewModelCommand(ExecuteShowCatalogViewCommand);
            ShowClienteViewCommand = new ViewModelCommand(ExecuteShowCustomerViewCommand);
            ShowProveedorViewCommand = new ViewModelCommand(ExecuteShowSupplierViewCommand);
            ShowComprasViewCommand = new ViewModelCommand(ExecuteShowShoppingViewCommand);
            ShowPuntoDeVentaCommand = new ViewModelCommand(ExecuteShowPuntoDeVentaCommand);
            ShowHistorialVentasCommand = new ViewModelCommand(ExecuteShowHistorialVentasCommand);
            ShowReportesViewCommand = new ViewModelCommand(ExecuteShowReportViewCommand);
            ShowUsuarioViewCommand = new ViewModelCommand(ExecuteShowUsuarioViewCommand);
            ShowRolViewCommand = new ViewModelCommand(ExecuteShowRollViewCommand);
            ShowConfiguracionViewCommand = new ViewModelCommand(ExecuteShowConfigurationViewCommand);
            LogoutCommand = new ViewModelCommand(ExecuteLogout);

            // --- Carga Inicial ---
            ExecuteShowHomeViewCommand(null); // Vista por defecto
        }

        // --- Métodos de Ejecución de Comandos ---
        // --- NUEVO MÉTODO DE INICIALIZACIÓN ASÍNCRONA ---
        public async Task InitializeAsync()
        {
            await LoadCurrentUserData();
            ExecuteShowHomeViewCommand(null); // Carga la vista por defecto
        }
        private void ExecuteLogout(object obj)
        {
            // Limpiamos la sesión guardada
            Properties.Settings.Default.SavedUsername = string.Empty;
            Properties.Settings.Default.Save();

            // Limpiamos la identidad del hilo
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            // Invocamos el evento. App.xaml.cs se encargará de cerrar esta ventana y abrir el Login.
            // Es más limpio que reiniciar la aplicación.
            LogoutSuccess?.Invoke();
        }
        private void ExecuteShowPuntoDeVentaCommand(object obj)
        {
            // Carga el ViewModel del Punto de Venta
            CurrentChildView = _serviceProvider.GetService<FacturacionViewModel>();
            Caption = "Punto de Venta (POS)";
            Icon = IconChar.CashRegister;
        }

        private void ExecuteShowHistorialVentasCommand(object obj)
        {
            // Carga el ViewModel del Historial
            CurrentChildView = _serviceProvider.GetService<FacturasHistorialViewModel>();
            Caption = "Historial de Ventas";
            Icon = IconChar.History;
        }
        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<DashboardViewModel>();
            Caption = "Panel Principal";
            Icon = IconChar.Home;
        }

        private void ExecuteShowCatalogViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<CatalogoViewModel>();
            Caption = "Inventario";
            Icon = IconChar.List;
        }

        private void ExecuteShowCustomerViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ClientesViewModel>();
            Caption = "Clientes";
            Icon = IconChar.Users;
        }

        private void ExecuteShowSupplierViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ProveedorViewModel>();
            Caption = "Proveedores";
            Icon = IconChar.Truck;
        }

        private void ExecuteShowShoppingViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<PedidoViewModel>();
            Caption = "Pedidos";
            Icon = IconChar.ShoppingCart;
        }

        private void ExecuteShowBillingViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<FacturacionViewModel>();
            Caption = "Facturación (Punto de Venta)";
            Icon = IconChar.FileInvoiceDollar;
        }

        private void ExecuteShowReportViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ReportesViewModel>();
            Caption = "Reportes";
            Icon = IconChar.ChartColumn;
        }

        private void ExecuteShowUsuarioViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<UsuarioViewModel>();
            Caption = "Usuarios";
            Icon = IconChar.UserGear;
        }

        private void ExecuteShowRollViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<RolViewModel>();
            Caption = "Roles de Usuario";
            Icon = IconChar.UserShield;
        }

        private void ExecuteShowConfigurationViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ConfiguracionViewModel>();
            Caption = "Configuración";
            Icon = IconChar.Tools;
        }

        private async Task LoadCurrentUserData()
        {
            var identityName = Thread.CurrentPrincipal?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(identityName))
            {
                _currentUserService.ClearCurrentUser();
                CurrentUserAccount = new UserAccountModel { DisplayName = "Usuario no autenticado" };
            }
            else
            {
                try
                {
                    // El repositorio ya nos trae el usuario CON su rol
                    var user = await _unitOfWork.Usuarios.GetUserByEmail(identityName);
                    if (user != null && user.Activo)
                    {
                        // Guardamos el usuario completo en nuestro servicio de sesión
                        _currentUserService.SetCurrentUser(user);

                        // Actualizamos la UI con los datos de la cuenta
                        CurrentUserAccount = new UserAccountModel
                        {
                            Username = user.Email,
                            DisplayName = $"Bienvenido, {user.Nombre}",
                        };
                    }
                    else
                    {
                        _currentUserService.ClearCurrentUser();
                        CurrentUserAccount = new UserAccountModel { DisplayName = "Usuario no encontrado o inactivo" };
                    }
                }
                catch (Exception ex)
                {
                    _currentUserService.ClearCurrentUser();
                    CurrentUserAccount = new UserAccountModel { DisplayName = "Error al cargar" };
                    System.Diagnostics.Debug.WriteLine($"Error cargando datos de usuario: {ex.Message}");
                }
            }

            // --- LÍNEA CLAVE AÑADIDA ---
            // Después de todo el proceso, notificamos a la UI que el valor de 'IsAdmin'
            // (y cualquier otra propiedad que dependa del usuario) debe ser re-evaluado.
            OnPropertyChanged(nameof(IsAdmin));
        }
    }
}
