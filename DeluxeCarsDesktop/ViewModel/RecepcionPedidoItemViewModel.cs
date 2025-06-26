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
            set
            {
                // Calculamos la cantidad máxima que se puede recibir para este item
                int cantidadPendiente = this.CantidadPedida - (this.DetalleOriginal.CantidadRecibida ?? 0);

                // --- INICIO DE LA VALIDACIÓN ---

                if (value < 0)
                {
                    // Regla 1: No permitir números negativos.
                    SetProperty(ref _cantidadRecibida, 0);
                }
                else if (value > cantidadPendiente)
                {
                    // Regla 2: No permitir recibir más de lo que falta.
                    // Si el usuario intenta, lo limitamos al máximo permitido.
                    SetProperty(ref _cantidadRecibida, cantidadPendiente);
                }
                else
                {
                    // Si el valor es válido, lo aceptamos.
                    SetProperty(ref _cantidadRecibida, value);
                }
            }
        }

        private string _notaRecepcion;
        public string NotaRecepcion
        {
            get => _notaRecepcion;
            set => SetProperty(ref _notaRecepcion, value);
        }

        public RecepcionPedidoItemViewModel(DetallePedido detalle)
        {
            DetalleOriginal = detalle;
            CantidadRecibida = detalle.Cantidad - (detalle.CantidadRecibida ?? 0);
            NotaRecepcion = detalle.NotaRecepcion;
        }
    }
}
