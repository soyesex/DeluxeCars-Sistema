using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View.UserControls;
using DeluxeCarsEntities;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ConfiguracionViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISnackbarMessageQueue _messageQueue;
        private Configuracion _configuracion;

        // --- Propiedades para la Vista (CORREGIDAS CON SetProperty) ---
        private string _nombreTienda;
        public string NombreTienda { get => _nombreTienda; set => SetProperty(ref _nombreTienda, value); }

        private string _direccion;
        public string Direccion { get => _direccion; set => SetProperty(ref _direccion, value); }

        private string _telefono;
        public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _horarioAtencion;
        public string HorarioAtencion { get => _horarioAtencion; set => SetProperty(ref _horarioAtencion, value); }

        private decimal _porcentajeIVA;
        public decimal PorcentajeIVA { get => _porcentajeIVA; set => SetProperty(ref _porcentajeIVA, value); }

        // --- Propiedades para Email (CORREGIDAS) ---
        private string _smtpHost;
        public string SmtpHost { get => _smtpHost; set => SetProperty(ref _smtpHost, value); }

        private int _smtpPort;
        public int SmtpPort { get => _smtpPort; set => SetProperty(ref _smtpPort, value); }

        private string _emailEmisor;
        public string EmailEmisor
        {
            get => _emailEmisor;
            set
            {
                if (SetPropertyAndCheck(ref _emailEmisor, value))
                {
                    if (!UsarConfiguracionManual) { AutoDetectarConfiguracion(); }
                }
            }
        }

        private bool _enableSsl;
        public bool EnableSsl { get => _enableSsl; set => SetProperty(ref _enableSsl, value); }

        private bool _usarConfiguracionManual;
        public bool UsarConfiguracionManual
        {
            get => _usarConfiguracionManual;
            set
            {
                if (SetPropertyAndCheck(ref _usarConfiguracionManual, value) && !value)
                {
                    AutoDetectarConfiguracion();
                }
            }
        }

        private string _smtpPassword;
        public string SmtpPassword { get => _smtpPassword; set => SetProperty(ref _smtpPassword, value); }
        public bool NotificacionesActivas { get; set; }

        // ... (Otras propiedades como IsPinDialogOpen, etc. sin cambios) ...
        private bool _isPinDialogOpen;
        public bool IsPinDialogOpen { get => _isPinDialogOpen; set => SetProperty(ref _isPinDialogOpen, value); }
        private object _dialogContent;
        public object DialogContent { get => _dialogContent; set => SetProperty(ref _dialogContent, value); }
        // --- Comandos ---
        public ICommand GuardarCambiosCommand { get; }
        public ICommand CambiarPinCommand { get; }
        public ICommand CambiarLogoCommand { get; }
        public ICommand CambiarBannerCommand { get; }

        public ConfiguracionViewModel(IUnitOfWork unitOfWork, ISnackbarMessageQueue messageQueue)
        {
            _unitOfWork = unitOfWork;
            _messageQueue = messageQueue; // Asignamos la dependencia faltante

            GuardarCambiosCommand = new ViewModelCommand(async _ => await ExecuteGuardarCambios());
            CambiarPinCommand = new ViewModelCommand(ExecuteCambiarPin);
            CambiarLogoCommand = new ViewModelCommand(p => { /* Lógica futura */ });
            CambiarBannerCommand = new ViewModelCommand(p => { /* Lógica futura */ });
        }

        // Método de inicialización asíncrona para cargar los datos
        public async Task LoadAsync()
        {
            _configuracion = await _unitOfWork.Configuraciones.GetFirstAsync();
            if (_configuracion == null)
            {
                _configuracion = new Configuracion();
            }

            // Poblamos las propiedades. Como ahora usan SetProperty, la UI se actualizará sola.
            NombreTienda = _configuracion.NombreTienda;
            Direccion = _configuracion.Direccion;
            Telefono = _configuracion.Telefono;
            Email = _configuracion.Email;
            HorarioAtencion = _configuracion.HorarioAtencion;
            PorcentajeIVA = _configuracion.PorcentajeIVA;
            SmtpHost = _configuracion.SmtpHost;
            SmtpPort = _configuracion.SmtpPort;
            EmailEmisor = _configuracion.EmailEmisor;
            EnableSsl = _configuracion.EnableSsl;
            SmtpPassword = EncryptionHelper.Decrypt(_configuracion.PasswordEmailEmisor);
            UsarConfiguracionManual = !string.IsNullOrEmpty(_configuracion.SmtpHost);
            NotificacionesActivas = _configuracion.NotificacionesActivas;
            OnPropertyChanged(nameof(NotificacionesActivas));
            OnPropertyChanged(nameof(SmtpPassword));
        }

        private async Task ExecuteGuardarCambios()
        {
            // Actualizamos la entidad con los valores del ViewModel
            _configuracion.NombreTienda = this.NombreTienda;
            _configuracion.Direccion = this.Direccion;
            _configuracion.Telefono = this.Telefono;
            _configuracion.Email = this.Email;
            _configuracion.HorarioAtencion = this.HorarioAtencion;
            _configuracion.PorcentajeIVA = this.PorcentajeIVA;
            _configuracion.SmtpHost = this.SmtpHost;
            _configuracion.SmtpPort = this.SmtpPort;
            _configuracion.EmailEmisor = this.EmailEmisor;
            _configuracion.EnableSsl = this.EnableSsl;
            _configuracion.NotificacionesActivas = this.NotificacionesActivas;

            if (!string.IsNullOrEmpty(SmtpPassword))
            {
                _configuracion.PasswordEmailEmisor = EncryptionHelper.Encrypt(SmtpPassword);
            }
            else if (_configuracion.PasswordEmailEmisor == null) // Solo borra si no había nada antes y sigue vacío
            {
                _configuracion.PasswordEmailEmisor = null;
            }


            try
            {
                if (_configuracion.Id == 0) // Si es un registro nuevo
                {
                    await _unitOfWork.Configuraciones.AddAsync(_configuracion);
                }
                else // Si es un registro existente que estamos actualizando
                {
                    // --- ESTA ES LA LÍNEA CLAVE QUE FALTABA ---
                    // Le decimos explícitamente a Entity Framework que la entidad ha sido modificada.
                    _unitOfWork.Context.Entry(_configuracion).State = EntityState.Modified;
                }

                // Ahora, al llamar a CompleteAsync, EF generará el comando UPDATE correctamente.
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Configuración guardada exitosamente.", "Éxito");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar la configuración: {ex.Message}", "Error");
            }
        }
        private void AutoDetectarConfiguracion()
        {
            if (string.IsNullOrEmpty(EmailEmisor)) return;

            if (EmailEmisor.Contains("@gmail.com"))
            {
                SmtpHost = "smtp.gmail.com";
                SmtpPort = 587;
                EnableSsl = true;
            }
            else if (EmailEmisor.Contains("@outlook.com") || EmailEmisor.Contains("@hotmail.com"))
            {
                SmtpHost = "smtp.office365.com";
                SmtpPort = 587;
                EnableSsl = true;
            }
            else
            {
                // Si no es un proveedor conocido, no hacemos nada y dejamos que el usuario lo ponga manual.
                // Opcional: podrías limpiar los campos si quieres.
                SmtpHost = "";
                SmtpPort = 0;
            }

            // Notificamos a la UI que estas propiedades han cambiado
            OnPropertyChanged(nameof(SmtpHost));
            OnPropertyChanged(nameof(SmtpPort));
            OnPropertyChanged(nameof(EnableSsl));
        }
        // AÑADIR ESTE NUEVO MÉTODO
        private void ExecuteCambiarPin(object parameter)
        {
            // 1. Creamos la vista que será el contenido del diálogo.
            var cambiarPinDialog = new CambiarPinDialog();

            // 2. Creamos el ViewModel para el diálogo, pasándole la lógica correcta.
            var cambiarPinViewModel = new CambiarPinDialogViewModel(

                // CORRECCIÓN: La firma ahora acepta los 3 parámetros que nos envía el diálogo.
                async (pinActual, nuevoPin, confirmarPin) =>
                {
                    try
                    {
                        // PASO DE SEGURIDAD CLAVE: Validar primero el PIN actual.
                        bool esPinActualValido = await _unitOfWork.ValidarPinAdministradorAsync(pinActual);
                        if (!esPinActualValido)
                        {
                            // Usamos el ErrorMessage del propio diálogo para notificar al usuario.
                            _messageQueue.Enqueue("Error: El PIN actual introducido es incorrecto.");
                            // No cerramos el diálogo, permitimos que el usuario reintente.
                            return;
                        }

                        // Si el PIN actual es válido, procedemos a hashear y guardar el nuevo PIN.
                        PasswordHelper.CreatePasswordHash(nuevoPin, out byte[] hash, out byte[] salt);

                        var config = await _unitOfWork.Configuraciones.GetByIdAsync(1);
                        if (config != null)
                        {
                            config.AdminPINHash = hash;
                            config.AdminPINSalt = salt;
                            await _unitOfWork.CompleteAsync();

                            _messageQueue.Enqueue("✅ PIN de Administrador actualizado correctamente.");
                            IsPinDialogOpen = false; // Cerramos el diálogo SÓLO si todo fue exitoso.
                        }
                    }
                    catch (Exception ex)
                    {
                        _messageQueue.Enqueue($"Error al guardar el PIN: {ex.Message}");
                        // Opcional: podrías querer cerrar el diálogo aquí también.
                        IsPinDialogOpen = false;
                    }
                },
                () => {
                    // Lógica a ejecutar cuando se cancela (simplemente cerrar)
                    IsPinDialogOpen = false;
                }
            );

            // 3. Asignamos el ViewModel al DataContext de la vista del diálogo
            cambiarPinDialog.DataContext = cambiarPinViewModel;

            // 4. Asignamos la vista del diálogo a nuestra propiedad y abrimos el DialogHost
            DialogContent = cambiarPinDialog;
            IsPinDialogOpen = true;
        }
    }
}
