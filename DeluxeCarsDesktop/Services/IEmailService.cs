using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string userEmail, string userName, string resetToken);
        Task EnviarEmailPedidoCreado(Pedido pedido);
        Task EnviarEmailPedidoRecibido(Pedido pedido);
        Task EnviarEmailPagoRealizado(PagoProveedor pago, Pedido pedidoAfectado);
    }
}
