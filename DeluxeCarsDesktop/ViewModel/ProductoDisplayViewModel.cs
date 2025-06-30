using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ProductoDisplayViewModel : ViewModelBase
    {
        public Producto Producto { get; }
        public int Id => Producto.Id;
        public string Nombre => Producto.Nombre;
        public string OEM => Producto.OriginalEquipamentManufacture;
        public decimal Precio => Producto.Precio;
        public bool Estado => Producto.Estado;
        public string CategoriaNombre => Producto.Categoria?.Nombre ?? "N/A";
        public string EstadoStock
        {
            get
            {
                if (StockCalculado <= 0) return "Agotado";
                if (Producto.StockMinimo.HasValue && StockCalculado < Producto.StockMinimo.Value) return "Bajo Stock";
                return "En Stock";
            }
        }
        private int _stockCalculado;
        public int StockCalculado
        {
            get => _stockCalculado;
            set
            {
                SetProperty(ref _stockCalculado, value);
                OnPropertyChanged(nameof(EstadoStock)); // <-- AVISA QUE EL ESTADO TAMBIÉN CAMBIÓ
            }
        }

        public ProductoDisplayViewModel(Producto producto)
        {
            Producto = producto;
        }
    }
}
