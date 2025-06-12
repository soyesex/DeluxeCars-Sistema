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
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;
        private readonly INavigationService _navigationService;
        public event Action LogoutSuccess;

        private IUsuarioRepository _usuarioRepository;
        private IServiceProvider _serviceProvider;
        
        public UserAccountModel CurrentUserAccount
        {
            get
            {
                return _currentUserAccount;
            }
            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }

        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public IconChar Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        // Commands
        public ICommand ShowCatalogoCommand { get; }
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowCatalogoViewCommand { get; }
        public ICommand ShowClienteViewCommand { get; }
        public ICommand ShowProveedorViewCommand { get; }
        public ICommand ShowComprasViewCommand { get; }
        public ICommand ShowFacturacionViewCommand { get; }
        public ICommand ShowReportesViewCommand { get; }
        public ICommand ShowUsuarioViewCommand { get; }
        public ICommand ShowRolViewCommand { get; }
        public ICommand ShowConfiguracionViewCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(IUsuarioRepository usuarioRepository, IServiceProvider serviceProvider, INavigationService navigationService)
        {
            _usuarioRepository = usuarioRepository;
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;

            CurrentUserAccount = new UserAccountModel();
            

            // Initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowCatalogoViewCommand = new ViewModelCommand(ExecuteShowCatalogViewCommand);
            ShowClienteViewCommand = new ViewModelCommand(ExecuteShowCustomerViewCommand);
            ShowProveedorViewCommand = new ViewModelCommand(ExecuteShowSupplierViewCommand);
            ShowComprasViewCommand = new ViewModelCommand(ExecuteShowShoppingViewCommand);
            ShowFacturacionViewCommand = new ViewModelCommand(ExecuteShowBillingViewCommand);
            ShowReportesViewCommand = new ViewModelCommand(ExecuteShowReportViewCommand);
            ShowUsuarioViewCommand = new ViewModelCommand(ExecuteShowUsuarioViewCommand);

            ShowRolViewCommand = new ViewModelCommand(ExecuteShowRollViewCommand);
            ShowConfiguracionViewCommand = new ViewModelCommand(ExecuteShowConfigurationViewCommand);

            LogoutCommand = new ViewModelCommand(ExecuteLogout);

            // Default view
            ExecuteShowHomeViewCommand(null);

            // Carga datos del usuario actual según Thread.CurrentPrincipal
            LoadCurrentUserData();
        }

        private void ExecuteLogout(object obj)
        {
            // 1. LIMPIAR la sesión guardada en Settings (esto está perfecto)
            Properties.Settings.Default.SavedUsername = string.Empty;
            Properties.Settings.Default.Save();

            // 2. LIMPIAR la identidad de seguridad del hilo actual (buena práctica)
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            // 3. REINICIAR LA APLICACIÓN
            // Inicia una nueva instancia del ejecutable de la aplicación.
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);

            // Cierra la instancia actual por completo.
            LogoutSuccess?.Invoke();
        }
        private void ExecuteShowRollViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<RolViewModel>();
            Caption = "Rol";
            Icon = IconChar.CriticalRole;
        }

        private void ExecuteShowUsuarioViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<UsuarioViewModel>();
            Caption = "Usuarios";
            Icon = IconChar.User;
        }

        private void ExecuteShowCustomerViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ClientesViewModel>();
            Caption = "Clientes";
            Icon = IconChar.Users;
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<DashboardViewModel>();
            Caption = "Panel";
            Icon = IconChar.Home;
        }

        private void ExecuteShowCatalogViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<CatalogoViewModel>(); // Pide la instancia al contenedor
            Caption = "Catalogo";
            Icon = IconChar.List;
        }

        private void ExecuteShowSupplierViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ProveedorViewModel>();
            Caption = "Proveedores";
            Icon = IconChar.Wallet;
        }

        private void ExecuteShowShoppingViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<PedidoViewModel>();
            Caption = "Compras";
            Icon = IconChar.ShoppingCart;
        }

        private void ExecuteShowBillingViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<FacturacionViewModel>();
            Caption = "Facturacion";
            Icon = IconChar.FileInvoiceDollar;
        }

        private void ExecuteShowReportViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ReportesViewModel>();
            Caption = "Reportes";
            Icon = IconChar.FileAlt;
        }

        private void ExecuteShowConfigurationViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetService<ConfiguracionViewModel>();
            Caption = "Configuracion";
            Icon = IconChar.Tools;
        }

        private async void LoadCurrentUserData()
        {
            var identity = Thread.CurrentPrincipal?.Identity;
            if (identity == null || !identity.IsAuthenticated || string.IsNullOrWhiteSpace(identity.Name))
            {
                CurrentUserAccount.DisplayName = "Usuario no autenticado";
                return;
            }

            try
            {
                var user = await _usuarioRepository.GetUserByEmail(identity.Name);
                if (user != null)
                {
                    CurrentUserAccount = new UserAccountModel
                    {
                        Username = user.Email,
                        DisplayName = $"Bienvenido/a {user.Nombre}",
                        //ProfilePicture = user.ProfilePicture // Asignar la foto de perfil si está disponible
                    };
                    //CurrentUserAccount.Username = user.Username;
                    //CurrentUserAccount.DisplayName = $"Bienvenido/a {user.Name}";
                    //CurrentUserAccount.ProfilePicture = null;
                }
                else
                {
                    CurrentUserAccount.DisplayName = "Usuario inválido";
                }
            }
            catch (ArgumentException)
            {
                // En caso de que GetByUsername siga recibiendo algo inválido, lo tomamos aquí
                CurrentUserAccount.DisplayName = "Error al cargar usuario";
            }
        }
    }
}
