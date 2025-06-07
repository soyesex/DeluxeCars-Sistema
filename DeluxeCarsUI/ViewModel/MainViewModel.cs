using DeluxeCarsUI.Model;
using DeluxeCarsUI.Properties;
using DeluxeCarsUI.Repositories;
using DeluxeCarsUI.View;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        private IUserRepository userRepository;
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

        public ViewModelBase CurrentChildView { 
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
        public IconChar Icon {
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

        public MainViewModel()
        {
            userRepository = new UserRepository();
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

            LogoutCommand = new ViewModelCommand(ExecuteLogout, CanExecuteLogout);

            // Default view
            ExecuteShowHomeViewCommand(null);

            // Carga datos del usuario actual según Thread.CurrentPrincipal
            LoadCurrentUserData();
        }
        private bool CanExecuteLogout(object obj)
        {
            // Puedes poner aquí la lógica que habilite/deshabilite el “Cerrar sesión”:
            // por ejemplo, si el usuario está autenticado o no. 
            // Para simplificar, devolvemos true siempre:
            return true;
        }

        private void ExecuteLogout(object obj)
        {
            // 1) LIMPIAR la sesión guardada en Settings
            Settings.Default.SavedUsername = string.Empty;
            Settings.Default.Save();

            // 2) Abrimos la nueva ventana de LoginView:
            var loginView = new LoginView();
            loginView.Show();

            // 3) Cerramos esta MainView:
            var ventanaMain = Application.Current
                                      .Windows
                                      .OfType<MainView>()
                                      .FirstOrDefault();
            ventanaMain?.Close();
        }
        private void ExecuteShowRollViewCommand(object obj)
        {
            CurrentChildView = new RolViewModel();
            Caption = "Rol";
            Icon = IconChar.CriticalRole;
        }

        private void ExecuteShowUsuarioViewCommand(object obj)
        {
            CurrentChildView = new UsuarioViewModel();
            Caption = "Usuarios";
            Icon = IconChar.User;
        }

        private void ExecuteShowCustomerViewCommand(object obj)
        {
            CurrentChildView = new ClientesViewModel();
            Caption = "Clientes";
            Icon = IconChar.Users;
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Panel";
            Icon = IconChar.Home;
        }

        private void ExecuteShowCatalogViewCommand(object obj)
        {
            CurrentChildView = new CatalogoViewModel();
            Caption = "Catalogo";
            Icon = IconChar.List;
        }

        private void ExecuteShowSupplierViewCommand(object obj)
        {
            CurrentChildView = new ProveedorViewModel();
            Caption = "Proveedores";
            Icon = IconChar.Wallet;
        }

        private void ExecuteShowShoppingViewCommand(object obj)
        {
            CurrentChildView = new ComprasViewModel();
            Caption = "Compras";
            Icon = IconChar.ShoppingCart;
        }

        private void ExecuteShowBillingViewCommand(object obj)
        {
            CurrentChildView = new FacturacionViewModel();
            Caption = "Facturacion";
            Icon = IconChar.FileInvoiceDollar;
        }

        private void ExecuteShowReportViewCommand(object obj)
        {
            CurrentChildView = new ReportesViewModel();
            Caption = "Reportes";
            Icon = IconChar.FileAlt;
        }

        private void ExecuteShowConfigurationViewCommand(object obj)
        {
            CurrentChildView = new ConfiguracionViewModel();
            Caption = "Configuracion";
            Icon = IconChar.Tools;
        }

        private void LoadCurrentUserData()
        {
            var identity = Thread.CurrentPrincipal?.Identity;
            if (identity == null || !identity.IsAuthenticated || string.IsNullOrWhiteSpace(identity.Name))
            {
                CurrentUserAccount.DisplayName = "Usuario no autenticado";
                return;
            }

            try
            {
                var user = userRepository.GetByUsername(identity.Name);
                if (user != null)
                {
                    CurrentUserAccount.Username = user.Username;
                    CurrentUserAccount.DisplayName = $"Bienvenido/a {user.Name}";
                    CurrentUserAccount.ProfilePicture = null;
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
