using DeluxeCarsShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailService()
        {
            _smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            _smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
        }

        public async Task SendPasswordResetEmailAsync(string userEmail, string userName, string resetToken)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser, "Soporte Deluxe Cars"),
                    Subject = "Restablecimiento de Contraseña - Deluxe Cars",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(userEmail);
                mailMessage.Body = $@"
                <html>
                <body>
                    <h2>Hola {userName},</h2>
                    <p>Hemos recibido una solicitud para restablecer tu contraseña. Por favor, usa el siguiente token para completar el proceso en la aplicación:</p>
                    <h3 style='font-family: monospace; background-color: #f0f0f0; padding: 10px; border-radius: 5px;'>{resetToken}</h3>
                    <p>Este token expirará en 1 hora.</p>
                    <p>Si no solicitaste esto, puedes ignorar este correo de forma segura.</p>
                    <br/>
                    <p>Gracias,<br/>El equipo de Deluxe Cars</p>
                </body>
                </html>";

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
