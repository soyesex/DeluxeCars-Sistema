using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class DevolucionItemViewModel : ViewModelBase
    {
        // Guardamos una referencia al detalle original de la factura
        public DetalleFactura DetalleFacturaOriginal { get; }

        // Propiedades de solo lectura para mostrar en la UI
        public string DescripcionProducto => DetalleFacturaOriginal.Descripcion;
        public int CantidadVendida => DetalleFacturaOriginal.Cantidad;
        public decimal PrecioVenta => DetalleFacturaOriginal.PrecioUnitario;

        // Propiedades que el usuario puede editar en la UI
        private int _cantidadADevolver;
        public int CantidadADevolver
        {
            get => _cantidadADevolver;
            set
            {
                // Validación para no devolver más de lo que se compró, ni números negativos.
                if (value > CantidadVendida)
                    SetProperty(ref _cantidadADevolver, CantidadVendida);
                else if (value < 0)
                    SetProperty(ref _cantidadADevolver, 0);
                else
                    SetProperty(ref _cantidadADevolver, value);
            }
        }

        private bool _reingresaAInventario = true; // Por defecto, asumimos que sí reingresa
        public bool ReingresaAInventario
        {
            get => _reingresaAInventario;
            set => SetProperty(ref _reingresaAInventario, value);
        }

        public DevolucionItemViewModel(DetalleFactura detalleOriginal)
        {
            DetalleFacturaOriginal = detalleOriginal;
            // Inicialmente, no se devuelve nada.
            _cantidadADevolver = 0;
        }
    }
}
