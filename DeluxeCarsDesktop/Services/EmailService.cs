using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsEntities;
using MailKit.Net.Smtp; // <-- ASEGÚRATE DE QUE ESTA LÍNEA EXISTA
using MimeKit;
using System.Text;

namespace DeluxeCarsDesktop.Services
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Un único constructor que recibe IUnitOfWork
        public EmailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Un método privado y reutilizable para configurar y enviar cualquier correo
        private async Task EnviarEmailAsync(MimeMessage message)
        {
            try // <-- Asegúrate de que el try empiece aquí
            {
                var emailConfig = await _unitOfWork.Configuraciones.GetFirstAsync();
                if (emailConfig == null || !emailConfig.NotificacionesActivas || string.IsNullOrEmpty(emailConfig.SmtpHost))
                {
                    return;
                }

                string decryptedPass = EncryptionHelper.Decrypt(emailConfig.PasswordEmailEmisor);
                if (string.IsNullOrEmpty(decryptedPass))
                {
                    throw new InvalidOperationException("La contraseña del correo no está configurada.");
                }

                using (var client = new SmtpClient())
                {
                    var secureSocketOptions = emailConfig.EnableSsl
                        ? MailKit.Security.SecureSocketOptions.StartTls
                        : MailKit.Security.SecureSocketOptions.None;

                    await client.ConnectAsync(emailConfig.SmtpHost, emailConfig.SmtpPort, secureSocketOptions);
                    await client.AuthenticateAsync(emailConfig.EmailEmisor, decryptedPass);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // --- PON EL PUNTO DE INTERRUPCIÓN EN LA SIGUIENTE LÍNEA ---
                System.Diagnostics.Debug.WriteLine($"Error en EmailService: {ex.Message}");
                throw; // Vuelve a lanzar la excepción para que el ViewModel la atrape y la muestre
            }
        }

        public async Task EnviarEmailPedidoCreado(Pedido pedido)
        {
            var emailConfig = await _unitOfWork.Configuraciones.GetFirstAsync();
            // La validación de NotificacionesActivas ya está en EnviarEmailAsync

            // --- Lógica para crear la tabla de productos ---
            var detallesHtml = new StringBuilder();
            detallesHtml.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse; width:100%; font-family:sans-serif;'>")
                        .Append("<thead><tr style='background-color:#f0f0f0;'><th>Producto</th><th>Cantidad</th><th>Precio Unit.</th><th>Subtotal</th></tr></thead>")
                        .Append("<tbody>");

            foreach (var detalle in pedido.DetallesPedidos)
            {
                detallesHtml.AppendFormat("<tr><td>{0}</td><td style='text-align:center;'>{1}</td><td style='text-align:right;'>{2:C}</td><td style='text-align:right;'>{3:C}</td></tr>",
                    detalle.Descripcion, detalle.Cantidad, detalle.PrecioUnitario, detalle.SubtotalPreview);
            }

            detallesHtml.AppendFormat("<tr><td colspan='3' style='text-align:right; font-weight:bold;'>Total del Pedido:</td><td style='text-align:right; font-weight:bold;'>{0:C}</td></tr>", pedido.MontoTotal);
            detallesHtml.Append("</tbody></table>");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailConfig.NombreTienda, emailConfig.EmailEmisor));
            message.To.Add(new MailboxAddress(pedido.Proveedor.RazonSocial, pedido.Proveedor.Email));
            message.Subject = $"Nuevo Pedido Aprobado: {pedido.NumeroPedido}";

            message.Body = new TextPart("html")
            {
                Text = $@"
            <p>Estimado/a {pedido.Proveedor.RazonSocial},</p>
            <p>Le informamos que hemos aprobado un nuevo pedido de compra con el número <strong>{pedido.NumeroPedido}</strong>. A continuación, el detalle:</p>
            <br/>
            {detallesHtml.ToString()}
            <br/>
            <p>Por favor, proceda con la preparación del mismo. La fecha estimada de entrega es el {pedido.FechaEstimadaEntrega:dd/MM/yyyy}.</p>
            <p>Atentamente,<br/>El equipo de {emailConfig.NombreTienda}</p>"
            };

            await EnviarEmailAsync(message);
        }

        public async Task EnviarEmailPedidoRecibido(Pedido pedido)
        {
            var emailConfig = await _unitOfWork.Configuraciones.GetFirstAsync();
            if (emailConfig == null) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailConfig.NombreTienda, emailConfig.EmailEmisor));
            message.To.Add(new MailboxAddress(pedido.Proveedor.RazonSocial, pedido.Proveedor.Email));
            message.Subject = $"Confirmación de Recepción - Pedido {pedido.NumeroPedido}";
            message.Body = new TextPart("html")
            {
                Text = $"<p>Estimado/a {pedido.Proveedor.RazonSocial},</p><p>Le informamos que hemos recibido la mercancía correspondiente al pedido <strong>{pedido.NumeroPedido}</strong>.</p><p>Atentamente,<br/>El equipo de {emailConfig.NombreTienda}</p>"
            };
            await EnviarEmailAsync(message);
        }

        public async Task EnviarEmailPagoRealizado(PagoProveedor pago, Pedido pedidoAfectado)
        {
            var emailConfig = await _unitOfWork.Configuraciones.GetFirstAsync();
            if (emailConfig == null) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailConfig.NombreTienda, emailConfig.EmailEmisor));
            message.To.Add(new MailboxAddress(pedidoAfectado.Proveedor.RazonSocial, pedidoAfectado.Proveedor.Email));
            message.Subject = $"Notificación de Pago - Pedido {pedidoAfectado.NumeroPedido}";
            message.Body = new TextPart("html")
            {
                Text = $"<p>Estimado/a {pedidoAfectado.Proveedor.RazonSocial},</p><p>Hemos registrado un pago por <strong>{pago.MontoPagado:C}</strong> para el pedido <strong>{pedidoAfectado.NumeroPedido}</strong>. El saldo pendiente es de {pedidoAfectado.SaldoPendiente:C}.</p><p>Atentamente,<br/>El equipo de {emailConfig.NombreTienda}</p>"
            };
            await EnviarEmailAsync(message);
        }

        // Se reescribe el método de reseteo de contraseña para usar el mismo patrón
        public async Task SendPasswordResetEmailAsync(string userEmail, string userName, string resetToken)
        {
            var emailConfig = await _unitOfWork.Configuraciones.GetFirstAsync();
            if (emailConfig == null) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress($"Soporte {emailConfig.NombreTienda}", emailConfig.EmailEmisor));
            message.To.Add(new MailboxAddress(userName, userEmail));
            message.Subject = "Restablecimiento de Contraseña";
            message.Body = new TextPart("html")
            {
                Text = $"<h2>Hola {userName},</h2><p>Usa el siguiente token para restablecer tu contraseña:</p><h3 style='font-family: monospace;'>{resetToken}</h3>"
            };
            await EnviarEmailAsync(message);
        }
    }
}
