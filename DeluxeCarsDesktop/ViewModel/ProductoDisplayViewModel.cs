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

        private int _stockCalculado;
        public int StockCalculado
        {
            get => _stockCalculado;
            set => SetProperty(ref _stockCalculado, value);
        }

        public ProductoDisplayViewModel(Producto producto)
        {
            Producto = producto;
        }
    }
}
