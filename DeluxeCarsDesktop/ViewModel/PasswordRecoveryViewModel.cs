using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
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
        private string _email;
        private string _token;
        private SecureString _newPassword;
        private SecureString _confirmPassword;
        private string _statusMessage;
        private bool _isResetStage = false; // Controla si mostramos la parte de solicitar o la de restablecer
        private readonly IEmailService _emailService;

        // --- Propiedades para el Binding en XAML ---
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                // Notificamos al comando que su condición 'CanExecute' pudo haber cambiado
                (RequestTokenCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Token
        {
            get => _token;
            set
            {
                SetProperty(ref _token, value);
                (ResetPasswordCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public SecureString NewPassword
        {
            get => _newPassword;
            set
            {
                SetProperty(ref _newPassword, value);
                (ResetPasswordCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public SecureString ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                (ResetPasswordCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        public bool IsResetStage { get => _isResetStage; private set => SetProperty(ref _isResetStage, value); }

        // --- Comandos ---
        public ICommand RequestTokenCommand { get; }
        public ICommand ResetPasswordCommand { get; }

        // --- Constructor ---
        public PasswordRecoveryViewModel(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService; // <-- Guárdalo

            // Inicializamos los comandos con su lógica de ejecución y validación
            RequestTokenCommand = new ViewModelCommand(async p => await ExecuteRequestToken(), p => CanExecuteRequestToken());
            ResetPasswordCommand = new ViewModelCommand(async p => await ExecuteResetPassword(), p => CanExecuteResetPassword());
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

                    StatusMessage = "Se ha enviado un token a tu correo. Por favor, revísalo y pégalo abajo.";
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
            if (NewPassword.Unsecure() != ConfirmPassword.Unsecure())
            {
                StatusMessage = "Las nuevas contraseñas no coinciden.";
                return;
            }

            StatusMessage = "Verificando token y restableciendo contraseña...";
            try
            {
                var success = await _unitOfWork.Usuarios.ResetPasswordAsync(Email, Token, NewPassword.Unsecure());
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
            return !string.IsNullOrWhiteSpace(Token) && NewPassword != null && NewPassword.Length > 0 && ConfirmPassword != null && ConfirmPassword.Length > 0;
        }
    }
}

