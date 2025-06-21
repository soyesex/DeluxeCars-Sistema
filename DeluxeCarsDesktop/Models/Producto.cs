using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Models
{
    public class Producto : ViewModelBase
    {
        private int _id;
        [Key]
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private int _idCategoria;
        public int IdCategoria { get => _idCategoria; set => SetProperty(ref _idCategoria, value); }

        private string _originalEquipamentManufacture;
        public string OriginalEquipamentManufacture { get => _originalEquipamentManufacture; set => SetProperty(ref _originalEquipamentManufacture, value); }

        private string _nombre;
        [Required]
        [StringLength(60)]
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

        private decimal _precio;
        public decimal Precio { get => _precio; set => SetProperty(ref _precio, value); }

        private string _descripcion;
        public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

        private bool _estado;
        public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

        private string? _imagenUrl;
        public string? ImagenUrl { get => _imagenUrl; set => SetProperty(ref _imagenUrl, value); }

        // --- Nuevas Columnas de la Fase 1 ---
        private int? _stockMinimo;
        public int? StockMinimo { get => _stockMinimo; set => SetProperty(ref _stockMinimo, value); }

        private int? _stockMaximo;
        public int? StockMaximo { get => _stockMaximo; set => SetProperty(ref _stockMaximo, value); }
        private decimal? _ultimoPrecioCompra;
        public decimal? UltimoPrecioCompra
        {
            get => _ultimoPrecioCompra;
            set => SetProperty(ref _ultimoPrecioCompra, value);
        }

        // --- Propiedades de Navegación ---
        private Categoria _categoria;
        public virtual Categoria Categoria { get => _categoria; set => SetProperty(ref _categoria, value); }

        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; } = new HashSet<DetallePedido>();
        public virtual ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new HashSet<ProductoProveedor>();
    }
}
