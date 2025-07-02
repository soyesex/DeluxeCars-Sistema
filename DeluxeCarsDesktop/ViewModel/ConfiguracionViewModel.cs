using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View.UserControls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ConfiguracionViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISnackbarMessageQueue _messageQueue;

        // --- Propiedades para el Diálogo ---
        private bool _isPinDialogOpen;
        public bool IsPinDialogOpen
        {
            get => _isPinDialogOpen;
            set => SetProperty(ref _isPinDialogOpen, value);
        }

        private object _dialogContent;
        public object DialogContent
        {
            get => _dialogContent;
            set => SetProperty(ref _dialogContent, value);
        }

        // --- Propiedades para la Vista ---
        private string _nombreTienda;
        public string NombreTienda
        {
            get => _nombreTienda;
            set => SetProperty(ref _nombreTienda, value);
        }

        private string _direccion;
        public string Direccion
        {
            get => _direccion;
            set => SetProperty(ref _direccion, value);
        }

        private string _telefono;
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _horarioAtencion;
        public string HorarioAtencion
        {
            get => _horarioAtencion;
            set => SetProperty(ref _horarioAtencion, value);
        }

        private decimal _porcentajeIVA;
        public decimal PorcentajeIVA
        {
            get => _porcentajeIVA;
            set => SetProperty(ref _porcentajeIVA, value);
        }

        // --- Comandos ---
        public ICommand GuardarCambiosCommand { get; }
        public ICommand CambiarPinCommand { get; }
        public ICommand CambiarLogoCommand { get; }
        public ICommand CambiarBannerCommand { get; }

        public ConfiguracionViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCambiosCommand = new ViewModelCommand(async _ => await ExecuteGuardarCambios());

            CambiarPinCommand = new ViewModelCommand(ExecuteCambiarPin);

            CambiarLogoCommand = new ViewModelCommand(p => { /* Lógica futura */ });
            CambiarBannerCommand = new ViewModelCommand(p => { /* Lógica futura */ });
        }

        // Método de inicialización asíncrona para cargar los datos
        public async Task LoadAsync()
        {
            // Buscamos la única fila de configuración (la que tiene Id = 1)
            var config = await _unitOfWork.Configuraciones.GetByIdAsync(1);
            if (config != null)
            {
                NombreTienda = config.NombreTienda;
                Direccion = config.Direccion;
                Telefono = config.Telefono;
                Email = config.Email;
                HorarioAtencion = config.HorarioAtencion;
                PorcentajeIVA = config.PorcentajeIVA;
            }
            else
            {
                // Manejar el caso de que la fila no exista (aunque el Seeding debería prevenirlo)
                // Podríamos mostrar un error o usar valores por defecto.
                ShowTemporaryErrorMessage("No se encontró la configuración de la empresa.", 10);
            }
        }

        private async Task ExecuteGuardarCambios()
        {
            var config = await _unitOfWork.Configuraciones.GetByIdAsync(1);
            if (config != null)
            {
                // Actualizamos el objeto de la entidad con los valores de las propiedades del ViewModel
                config.NombreTienda = this.NombreTienda;
                config.Direccion = this.Direccion;
                config.Telefono = this.Telefono;
                config.Email = this.Email;
                config.HorarioAtencion = this.HorarioAtencion;
                config.PorcentajeIVA = this.PorcentajeIVA;

                // Guardamos los cambios en la base de datos
                await _unitOfWork.CompleteAsync();

                // Notificamos al usuario que todo salió bien
                ShowTemporaryErrorMessage("✅ Configuración guardada exitosamente.", 5);
            }
            else
            {
                ShowTemporaryErrorMessage("Error: No se encontró la configuración para guardar.", 10);
            }
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
