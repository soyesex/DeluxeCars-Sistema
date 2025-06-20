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
        //// --- Dependencias y Estado ---
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO: Ahora usamos UnitOfWork
        //private readonly IServiceProvider _serviceProvider;

        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;

        public bool CanGoBack => _navigationService.CanGoBack;

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
        public bool IsAdmin => _currentUserService.IsAdmin;

        


        // --- Comandos ---
        public ICommand GoBackCommand { get; }
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

        public MainViewModel(ICurrentUserService currentUserService,
            INavigationService navigationService, IUnitOfWork unitOfWork)
        {

            _navigationService = navigationService;
            _unitOfWork = unitOfWork; // <-- Se asigna aquí
            _currentUserService = currentUserService;

            // Nos suscribimos al evento del servicio para reaccionar a los cambios de navegación
            _navigationService.CurrentMainViewChanged += OnCurrentMainViewChanged;

            // --- Comandos de Navegación SIMPLIFICADOS ---
            ShowHomeViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<DashboardViewModel>());
            ShowCatalogoViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<CatalogoViewModel>());
            ShowClienteViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ClientesViewModel>());
            ShowProveedorViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ProveedorViewModel>());
            ShowComprasViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<PedidoViewModel>());
            ShowPuntoDeVentaCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<FacturacionViewModel>());
            ShowHistorialVentasCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<FacturasHistorialViewModel>());
            ShowReportesViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ReportesViewModel>());
            ShowUsuarioViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<UsuarioViewModel>());
            ShowRolViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<RolViewModel>());
            ShowConfiguracionViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ConfiguracionViewModel>());

            // --- Comando para Volver ---
            GoBackCommand = new ViewModelCommand(p => _navigationService.GoBack(), p => _navigationService.CanGoBack);
            LogoutCommand = new ViewModelCommand(ExecuteLogout);
        }

        // Este método se ejecuta cada vez que el NavigationService cambia la vista.
        // Tu implementación actual ya es PERFECTA. No necesita cambios.
        private void OnCurrentMainViewChanged()
        {
            CurrentChildView = _navigationService.CurrentMainView;
            OnPropertyChanged(nameof(CurrentChildView)); // Notificamos a la UI para que actualice el ContentControl
            UpdateCaptionAndIcon(CurrentChildView); // Llamamos al método que pone el título/ícono
            (GoBackCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); // Notificamos al botón "Volver"
            OnPropertyChanged(nameof(CanGoBack)); // <-- AÑADE ESTA LÍNEA
        }

        // Este método se ejecuta una sola vez al iniciar la aplicación.
        // Tu implementación actual también es PERFECTA. No necesita cambios.
        public async Task InitializeAsync()
        {
            await LoadCurrentUserData(); // Carga los datos del usuario logueado
            await _navigationService.NavigateTo<DashboardViewModel>();// Carga la vista por defecto (el dashboard)
        }

        // Centraliza toda la lógica para actualizar el título y el ícono de la ventana.
        private void UpdateCaptionAndIcon(ViewModelBase viewModel)
        {
            if (viewModel is DashboardViewModel) { Caption = "Panel Principal"; Icon = IconChar.Home; }
            else if (viewModel is CatalogoViewModel) { Caption = "Inventario"; Icon = IconChar.List; }
            else if (viewModel is ClientesViewModel) { Caption = "Clientes"; Icon = IconChar.Users; }
            else if (viewModel is ProveedorViewModel) { Caption = "Proveedores"; Icon = IconChar.Truck; }
            else if (viewModel is PedidoViewModel) { Caption = "Pedidos"; Icon = IconChar.ShoppingCart; }
            else if (viewModel is FacturacionViewModel) { Caption = "Punto de Venta (POS)"; Icon = IconChar.CashRegister; }
            else if (viewModel is FacturasHistorialViewModel) { Caption = "Historial de Ventas"; Icon = IconChar.History; }
            else if (viewModel is ReportesViewModel) { Caption = "Reportes"; Icon = IconChar.ChartColumn; }
            else if (viewModel is UsuarioViewModel) { Caption = "Usuarios"; Icon = IconChar.UserGear; }
            else if (viewModel is RolViewModel) { Caption = "Roles de Usuario"; Icon = IconChar.UserShield; }
            else if (viewModel is ConfiguracionViewModel) { Caption = "Configuración"; Icon = IconChar.Tools; }
            else { Caption = "Deluxe Cars"; Icon = IconChar.Car; } // Un valor por defecto
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
        
        private async Task LoadCurrentUserData()
        {
            var identityName = Thread.CurrentPrincipal?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(identityName))
            {
                _currentUserService.ClearCurrentUser();
                // CORRECCIÓN: Asignamos un FirstName para que el DisplayName se genere solo.
                CurrentUserAccount = new UserAccountModel { FirstName = "Invitado" };
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
                            Username = user.Email, // <-- Esta línea ya no es necesaria
                            FirstName = user.Nombre, // <-- AÑADE ESTA LÍNEA
                            ProfilePicture = user.ProfilePicture
                        };
                    }
                    else
                    {
                        _currentUserService.ClearCurrentUser();
                        CurrentUserAccount = new UserAccountModel { FirstName = "Desconocido" };
                    }
                }
                catch (Exception ex)
                {
                    // --- Caso 4: Ocurrió un error ---
                    _currentUserService.ClearCurrentUser();
                    // CORRECCIÓN: Asignamos un FirstName para el estado de error.
                    CurrentUserAccount = new UserAccountModel { FirstName = "Error" };
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
