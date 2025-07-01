using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class PasswordRecoveryViewModel : ViewModelBase
    {
        // --- Dependencias y Estado ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private string _email;
        private string _token;
        private string _newPassword;
        private string _confirmPassword;
        private string _statusMessage;
        private bool _isResetStage = false;

        // --- Propiedades para el Binding en XAML ---
        public string Email { get => _email; set { SetProperty(ref _email, value); OnPropertyChanged(nameof(CanRequestToken)); } }
        public string Token { get => _token; set { SetProperty(ref _token, value); OnPropertyChanged(nameof(CanResetPassword)); } }
        public string NewPassword { get => _newPassword; set { SetProperty(ref _newPassword, value); OnPropertyChanged(nameof(CanResetPassword)); } }
        public string ConfirmPassword { get => _confirmPassword; set { SetProperty(ref _confirmPassword, value); OnPropertyChanged(nameof(CanResetPassword)); } }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public bool IsResetStage
        {
            get => _isResetStage;
            private set
            {
                SetProperty(ref _isResetStage, value);

                // AVISO CLAVE: Notificamos a la interfaz que la propiedad dependiente también cambió.
                OnPropertyChanged(nameof(IsRequestStage));
            }
        }

        // --- NUEVA PROPIEDAD para controlar la primera etapa del formulario ---
        public bool IsRequestStage => !IsResetStage;

        // --- Propiedades para habilitar/deshabilitar botones ---
        public bool CanRequestToken => !string.IsNullOrWhiteSpace(Email);
        public bool CanResetPassword => !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword);
        public event Action GoBackToLoginRequested;

        // --- Comandos ---
        public ICommand RequestTokenCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand OpenEmailClientCommand { get; } 
        public ICommand GoBackToLoginCommand { get; }

        // --- Constructor ---
        public PasswordRecoveryViewModel(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            RequestTokenCommand = new ViewModelCommand(async p => await ExecuteRequestToken());
            ResetPasswordCommand = new ViewModelCommand(async p => await ExecuteResetPassword());
            OpenEmailClientCommand = new ViewModelCommand(ExecuteOpenEmailClient); // <-- INICIALIZACIÓN

            GoBackToLoginCommand = new ViewModelCommand(ExecuteGoBackToLogin);
        }

        // --- Métodos de Ejecución de Comandos ---

        private void ExecuteOpenEmailClient(object obj)
        {
            string url = "mailto:"; // Comando genérico para abrir el cliente de correo por defecto
            if (!string.IsNullOrEmpty(Email))
            {
                if (Email.Contains("gmail")) url = "https://mail.google.com";
                else if (Email.Contains("outlook") || Email.Contains("hotmail")) url = "https://outlook.live.com";
            }

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo abrir el cliente de correo: {ex.Message}";
            }
        }

        // --- Métodos de Ejecución de Comandos ---

        private async Task ExecuteRequestToken()
        {
            StatusMessage = "Procesando solicitud...";
            try
            {
                // --- INICIO DE LA CORRECCIÓN ---
                // Usamos el método GetUserByEmail que ya existe en tu repositorio.
                var user = await _unitOfWork.Usuarios.GetUserByEmail(Email);

                if (user == null)
                {
                    // Si el usuario no existe, no hacemos nada y damos un mensaje genérico por seguridad.
                    // No queremos que un atacante sepa qué correos están registrados y cuáles no.
                    StatusMessage = "Si la dirección de correo es válida, se ha iniciado el proceso de recuperación.";
                    return;
                }

                // Ahora que sabemos que el usuario existe, generamos su token.
                var passwordResetRequest = await _unitOfWork.Usuarios.GeneratePasswordResetTokenAsync(Email);

                // Verificamos que se haya creado la solicitud de reseteo.
                if (passwordResetRequest != null)
                {
                    // Guardamos el token en la base de datos ANTES de enviarlo.
                    await _unitOfWork.CompleteAsync();

                    string generatedToken = passwordResetRequest.Token.ToString();

                    // Enviamos el email con los datos correctos del usuario que encontramos.
                    await _emailService.SendPasswordResetEmailAsync(user.Email, user.Nombre, generatedToken);

                    StatusMessage = "Se ha enviado un token a tu correo. Por favor, revísalo y pégalo arriba.";
                    IsResetStage = true; // Cambiamos a la segunda etapa del formulario.
                }
                else
                {
                    // Este caso podría ocurrir si hubo un problema al crear la solicitud en el repositorio.
                    StatusMessage = "No se pudo iniciar el proceso de recuperación en este momento.";
                }
                // --- FIN DE LA CORRECCIÓN ---
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ocurrió un error al enviar el correo: {ex.Message}";
            }
        }

        private async Task ExecuteResetPassword()
        {
            if (NewPassword != ConfirmPassword)
            {
                StatusMessage = "Las nuevas contraseñas no coinciden.";
                return;
            }

            StatusMessage = "Verificando token y restableciendo contraseña...";
            try
            {
                var success = await _unitOfWork.Usuarios.ResetPasswordAsync(Email, Token, NewPassword);
                await _unitOfWork.CompleteAsync(); // Guardamos el cambio de contraseña y la invalidación del token

                if (success)
                {
                    StatusMessage = "¡Contraseña restablecida con éxito! Ya puedes cerrar esta ventana e iniciar sesión.";
                }
                else
                {
                    StatusMessage = "El token es inválido o ha expirado. Por favor, solicita uno nuevo.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ocurrió un error: {ex.Message}";
            }
        }

        // --- Métodos de Validación de Comandos ---

        private bool CanExecuteRequestToken()
        {
            // El botón de solicitar token solo se activa si hay un email escrito
            return !string.IsNullOrWhiteSpace(Email);
        }

        private bool CanExecuteResetPassword()
        {
            // El botón de restablecer solo se activa si los campos necesarios no están vacíos
            return !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword);
        }

        private void ExecuteGoBackToLogin(object obj)
        {
            GoBackToLoginRequested?.Invoke();
        }
    }
}

