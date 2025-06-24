using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Messages;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Models.Notifications;
using DeluxeCarsDesktop.Properties;
using DeluxeCarsDesktop.Repositories;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.View;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessengerService _messengerService;
        public event Action LogoutSuccess;
        public bool CanGoBack => _navigationService.CanGoBack;

        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        // --- Propiedades Públicas para Binding ---
        public UserAccountModel CurrentUserAccount { get => _currentUserAccount; set => SetProperty(ref _currentUserAccount, value); }
        public ViewModelBase CurrentChildView { get => _currentChildView; set => SetProperty(ref _currentChildView, value); }
        
        public string Caption { get => _caption; set => SetProperty(ref _caption, value); }
        public IconChar Icon { get => _icon; set => SetProperty(ref _icon, value); }
        public bool IsAdmin => _currentUserService.IsAdmin;

        // --- Sistema de Notificaciones ---
        public ObservableCollection<AppNotification> AlertasNuevas { get; }
        public ObservableCollection<AppNotification> HistorialYEstados { get; }
        public int NotificationCount => AlertasNuevas.Count;
        private bool _isNotificationsPanelVisible;
        public bool IsNotificationsPanelVisible { get => _isNotificationsPanelVisible; set => SetProperty(ref _isNotificationsPanelVisible, value); }


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
        public ICommand ToggleNotificationsPanelCommand { get; }
        public ICommand ShowSugerenciasCompraViewCommand { get; }

        public MainViewModel(ICurrentUserService currentUserService, IMessengerService messengerService, INavigationService navigationService, IUnitOfWork unitOfWork, IServiceProvider serviceProvider, INotificationService notificationService)
        {

            _navigationService = navigationService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork; // <-- Se asigna aquí
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
            _messengerService = messengerService;

            AlertasNuevas = new ObservableCollection<AppNotification>();
            HistorialYEstados = new ObservableCollection<AppNotification>();
            AlertasNuevas.CollectionChanged += (s, e) => OnPropertyChanged(nameof(NotificationCount));
            _navigationService.CurrentMainViewChanged += OnCurrentMainViewChanged;

            AppEvents.OnNotificationsChanged += async () => await CargarEstadoDeNotificacionesAsync();

            _messengerService.Subscribe<InventarioCambiadoMessage>(async msg => await CargarEstadoDeNotificacionesAsync());

            // --- Comandos de Navegación SIMPLIFICADOS ---
            ShowHomeViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<DashboardViewModel>());
            ShowCatalogoViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<CatalogoViewModel>());
            ShowClienteViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ClientesViewModel>());
            ShowProveedorViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ProveedorViewModel>());
            ShowComprasViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<PedidoViewModel>());
            ShowPuntoDeVentaCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<FacturacionViewModel>());
            ShowHistorialVentasCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<FacturasHistorialViewModel>());
            ShowReportesViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ReportesRentabilidadViewModel>());
            ShowUsuarioViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<UsuarioViewModel>());
            ShowRolViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<RolViewModel>());
            ShowConfiguracionViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<ConfiguracionViewModel>());
            ShowSugerenciasCompraViewCommand = new ViewModelCommand(async p => await _navigationService.NavigateTo<SugerenciasCompraViewModel>());
            ToggleNotificationsPanelCommand = new ViewModelCommand(ExecuteToggleNotificationsPanel);
                        
            // --- Comando para Volver ---
            GoBackCommand = new ViewModelCommand(p => _navigationService.GoBack(), p => _navigationService.CanGoBack);
            LogoutCommand = new ViewModelCommand(ExecuteLogout);
        }

        public async Task InitializeAsync()
        {
            await LoadCurrentUserData(); // Carga los datos del usuario logueado
            await CargarEstadoDeNotificacionesAsync();
            await _navigationService.NavigateTo<DashboardViewModel>();// Carga la vista por defecto (el dashboard)
        }
                
        // Este método se ejecuta cada vez que el NavigationService cambia la vista.
        private void OnCurrentMainViewChanged()
        {
            CurrentChildView = _navigationService.CurrentMainView;
            OnPropertyChanged(nameof(CurrentChildView)); // Notificamos a la UI para que actualice el ContentControl
            UpdateCaptionAndIcon(CurrentChildView); // Llamamos al método que pone el título/ícono
            (GoBackCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); // Notificamos al botón "Volver"
            OnPropertyChanged(nameof(CanGoBack)); // <-- AÑADE ESTA LÍNEA
        }



        private async Task CargarEstadoDeNotificacionesAsync()
        {
            if (_currentUserService.CurrentUser == null) return;

            // Limpiamos las listas para evitar duplicados en cada recarga.
            AlertasNuevas.Clear();
            HistorialYEstados.Clear();

            try
            {
                var userId = _currentUserService.CurrentUser.Id;

                // 1. Manejar el Panel de Estado del Inventario (siempre lo buscamos y mostramos)
                var stockSummaryDB = await _unitOfWork.Notificaciones.GetUnreadSummaryAlertAsync("LowStockSummary", userId);
                var lowStockCount = stockSummaryDB?.DataCount ?? await _unitOfWork.Productos.CountLowStockProductsAsync();
                var haSidoLeido = stockSummaryDB?.Leida ?? true;

                var panelDeEstado = CrearPanelDeEstado(lowStockCount, haSidoLeido);
                HistorialYEstados.Add(panelDeEstado);

                // DESPUÉS (usando el nuevo método especializado y correcto):
                var otrasAlertasDB = await _unitOfWork.Notificaciones.GetActiveNotificationsWithDetailsAsync(userId);

                foreach (var notificacionDB in otrasAlertasDB.OrderByDescending(n => n.FechaCreacion))
                {
                    // --- INICIO DE LA NUEVA LÓGICA ---
                    ICommand? actionCommand = null;
                    string? actionText = null;

                    // Verificamos el tipo de notificación para asignarle una acción específica.
                    if (notificacionDB.Tipo == "PedidoPendiente" && notificacionDB.PedidoId.HasValue)
                    {
                        actionText = "Recepcionar Pedido";

                        // Creamos el comando específico para esta notificación.
                        actionCommand = new ViewModelCommand(async p =>
                        {
                            // Primero, marcamos esta notificación específica como leída.
                            await _unitOfWork.Notificaciones.MarcarComoLeidasAsync(new[] { notificacionDB.Id });
                            await _unitOfWork.CompleteAsync();

                            // Cerramos el panel de notificaciones para que el usuario vea la acción.
                            IsNotificationsPanelVisible = false;

                            // Navegamos a la ventana de recepción, pasándole el ID del pedido.
                            await _navigationService.OpenFormWindow(Utils.FormType.RecepcionPedido, notificacionDB.PedidoId.Value);

                            // Al volver, recargamos todo para asegurar que la UI está 100% actualizada.
                            await InitializeAsync();
                        });
                    }
                    // Aquí podrías añadir otros 'else if' para futuros tipos de notificaciones.

                    // --- FIN DE LA NUEVA LÓGICA ---

                    var notificacionUI = new AppNotification
                    {
                        Id = notificacionDB.Id,
                        Title = notificacionDB.Titulo,
                        Message = notificacionDB.Mensaje,
                        Timestamp = notificacionDB.FechaCreacion,
                        Type = Enum.TryParse<NotificationType>(notificacionDB.Tipo, true, out var typeEnum) ? typeEnum : NotificationType.Information,

                        // Asignamos el comando y el texto que acabamos de crear.
                        // Si no se creó ninguno, serán 'null' y el botón no aparecerá gracias a tu Converter.
                        ActionCommand = actionCommand,
                        ActionText = actionText
                    };
                    AlertasNuevas.Add(notificacionUI);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar notificaciones: {ex.Message}");
            }
        }

        private AppNotification CrearPanelDeEstado(int lowStockCount, bool haSidoLeido)
        {
            string title = (lowStockCount > 0) ? "Inventario Crítico" : "Estado del Inventario";
            string message = (lowStockCount > 0)
                ? $"Hay {lowStockCount} producto(s) con bajo stock que requieren atención."
                : "¡Todo en orden! No hay productos con bajo stock.";

            ICommand? command = null;
            string? commandText = null;
            var type = NotificationType.Success; // Por defecto, es un estado positivo

            // Solo creamos un comando si hay productos con bajo stock
            if (lowStockCount > 0)
            {
                type = NotificationType.Warning;
                commandText = "Revisar Sugerencias";

                // Creamos el comando que ejecuta la navegación al diálogo de sugerencias
                command = new ViewModelCommand(p =>
                {
                    // Usamos el ServiceProvider para obtener una instancia FRESCA del NavigationService
                    // y llamar a su método. Esto asegura que las dependencias son correctas.
                    var navService = _serviceProvider.GetRequiredService<INavigationService>();
                    navService.OpenSugerenciasDialogAsync();
                });
            }

            return new AppNotification
            {
                IsStatusPanel = true,
                NotificationKey = "LowStockSummary",
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.Now,
                ActionCommand = command, // <-- Aquí se asigna el comando creado
                ActionText = commandText
            };
        }

        private async void ExecuteToggleNotificationsPanel(object obj)
        {
            IsNotificationsPanelVisible = !IsNotificationsPanelVisible;

            if (IsNotificationsPanelVisible && AlertasNuevas.Any())
            {
                var alertasParaMarcar = AlertasNuevas.ToList();
                AlertasNuevas.Clear();

                foreach (var alerta in alertasParaMarcar.OrderByDescending(a => a.Timestamp))
                {
                    HistorialYEstados.Insert(0, alerta);
                }

                var ids = alertasParaMarcar.Select(a => a.Id).ToList();
                if (ids.Any())
                {
                    await _unitOfWork.Notificaciones.MarcarComoLeidasAsync(ids);
                    await _unitOfWork.CompleteAsync();
                }
            }
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
            else if (viewModel is ReportesRentabilidadViewModel) { Caption = "Reportes"; Icon = IconChar.ChartColumn; }
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
