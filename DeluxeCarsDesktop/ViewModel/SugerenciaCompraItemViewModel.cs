using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class SugerenciaCompraItemViewModel : ViewModelBase
    {
        public Producto Producto { get; set; }
        private readonly Action _onSelectionChanged;

        // Controla si el checkbox de esta fila esta seleccionado o no.
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                // ---> AÑADIDO: Después de cambiar el valor, llamamos a la acción para notificar al padre.
                _onSelectionChanged?.Invoke();
            }
        }

        // --- Propiedades de solo lectura para facilitar el Binding en la UI ---

        public string Nombre => Producto.Nombre;
        public int? StockMinimo => Producto.StockMinimo;
        public int? StockMaximo => Producto.StockMaximo;

        /// <summary>
        /// El proveedor principal o preferido para este producto.
        /// Tomamos el primero que encontremos en la lista de proveedores del producto.
        /// </summary>
        public string ProveedorPrincipal => Producto.ProductoProveedores?.FirstOrDefault()?.Proveedor.RazonSocial ?? "N/A";

        // --- Propiedades calculadas ---

        private int _stockActual;
        public int StockActual
        {
            get => _stockActual;
            set => SetProperty(ref _stockActual, value);
        }

        /// <summary>
        /// Calcula cuántas unidades se deberían pedir para llegar al Stock Máximo.
        /// </summary>
        public int CantidadSugerida
        {
            get
            {
                if (Producto.StockMaximo.HasValue && Producto.StockMaximo.Value > StockActual)
                {
                    return Producto.StockMaximo.Value - StockActual;
                }
                // Si no hay máximo, o si el stock actual ya es mayor o igual (caso raro),
                // sugerimos 1 si el stock es realmente bajo, si no, 0.
                return (StockActual <= (Producto.StockMinimo ?? 0)) ? 1 : 0;
            }
        }


        public SugerenciaCompraItemViewModel(Producto producto, Action onSelectionChanged)
        {
            Producto = producto;
            _onSelectionChanged = onSelectionChanged; // Guardamos la acción
        }
    }
}
