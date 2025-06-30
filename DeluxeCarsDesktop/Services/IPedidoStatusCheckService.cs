using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public interface IPedidoStatusCheckService
    {
        Task CheckPendingPedidosAsync();
    }
}
