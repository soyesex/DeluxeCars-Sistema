using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class DashboardViewModel : ViewModelBase, IAsyncLoadable
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

        // --- NUEVA PROPIEDAD ---
        private int _pedidosPendientes;
        public int PedidosPendientes { get => _pedidosPendientes; set => SetProperty(ref _pedidosPendientes, value); }


        public DashboardViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LoadAsync()
        {
            try
            {
                NumeroDeClientes = await _unitOfWork.Clientes.CountAsync();
                ProductosEnInventario = await _unitOfWork.Productos.CountAsync();
                ProductosBajoStock = await _unitOfWork.Productos.CountLowStockProductsAsync();

                var facturasHoy = await _unitOfWork.Facturas.GetFacturasByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));
                VentasDeHoy = facturasHoy.Sum(f => f.Total ?? 0m);

                PedidosPendientes = await _unitOfWork.Pedidos.CountAsync(p => p.Estado == EstadoPedido.Aprobado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos del dashboard: {ex.Message}");
            }
        }
    }
}
