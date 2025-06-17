using DeluxeCarsDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades para las Tarjetas del Dashboard ---
        private int _numeroDeClientes;
        public int NumeroDeClientes { get => _numeroDeClientes; set => SetProperty(ref _numeroDeClientes, value); }

        private int _productosEnInventario;
        public int ProductosEnInventario { get => _productosEnInventario; set => SetProperty(ref _productosEnInventario, value); }

        private int _productosBajoStock;
        public int ProductosBajoStock { get => _productosBajoStock; set => SetProperty(ref _productosBajoStock, value); }

        private decimal _ventasDeHoy;
        public decimal VentasDeHoy { get => _ventasDeHoy; set => SetProperty(ref _ventasDeHoy, value); }

        public DashboardViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Nuevo método público para cargar los datos
        public async Task LoadAsync()
        {
            try
            {
                var clientes = await _unitOfWork.Clientes.GetAllAsync();
                var productos = (await _unitOfWork.Productos.GetAllAsync()).ToList();
                var facturasHoy = await _unitOfWork.Facturas.GetFacturasByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));

                NumeroDeClientes = clientes.Count();
                ProductosEnInventario = productos.Count();
                ProductosBajoStock = productos.Count(p => p.Stock < 10);

                // Corrección aquí: Sumar sobre una colección de decimales nulables
                VentasDeHoy = facturasHoy.Sum(f => f.Total ?? 0m);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos del dashboard: {ex.Message}");
            }
        }
    }
}
