using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RecepcionPedidoItemViewModel : ViewModelBase
    {
        public DetallePedido DetalleOriginal { get; }

        public string NombreProducto => DetalleOriginal.Producto.Nombre;
        public int CantidadPedida => DetalleOriginal.Cantidad;

        private int _cantidadRecibida;
        public int CantidadRecibida
        {
            get => _cantidadRecibida;
            set => SetProperty(ref _cantidadRecibida, value);
        }

        public RecepcionPedidoItemViewModel(DetallePedido detalle)
        {
            DetalleOriginal = detalle;
            CantidadRecibida = detalle.Cantidad; // Por defecto, la cantidad recibida es la pedida.
        }
    }
}
