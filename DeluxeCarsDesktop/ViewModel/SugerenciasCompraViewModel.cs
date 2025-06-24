using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class SugerenciasCompraViewModel : ViewModelBase, IAsyncLoadable, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        public Action CloseAction { get; set; }
        // private readonly INavigationService _navigationService; // Lo añadiremos cuando lo necesitemos

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        public ObservableCollection<SugerenciaCompraItemViewModel> Sugerencias { get; private set; }

        public ICommand GenerarBorradorOrdenCommand { get; }

        public SugerenciasCompraViewModel(IUnitOfWork unitOfWork, ICurrentUserService currentUserService,
                                  INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            Sugerencias = new ObservableCollection<SugerenciaCompraItemViewModel>();

            GenerarBorradorOrdenCommand = new ViewModelCommand(ExecuteGenerarBorradorOrden, CanExecuteGenerarBorradorOrden);
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            Sugerencias.Clear();

            try
            {
                var productosBajoStock = await _unitOfWork.Productos.GetLowStockProductsAsync();
                var productIds = productosBajoStock.Select(p => p.Id).ToList();
                var stocksActuales = await _unitOfWork.Productos.GetCurrentStocksAsync(productIds);

                foreach (var producto in productosBajoStock)
                {
                    // ---> MODIFICADO: Al crear el item, le pasamos el método que debe llamar.
                    // Esta lambda se ejecutará cada vez que un checkbox cambie.
                    var itemVM = new SugerenciaCompraItemViewModel(producto,
                        () => (GenerarBorradorOrdenCommand as ViewModelCommand)?.RaiseCanExecuteChanged());

                    itemVM.StockActual = stocksActuales.GetValueOrDefault(producto.Id, 0);
                    Sugerencias.Add(itemVM);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar sugerencias de compra: {ex.Message}");
                MessageBox.Show("Ocurrió un error al cargar las sugerencias de compra.", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }
        private bool CanExecuteGenerarBorradorOrden(object obj)
        {
            if (Sugerencias == null) return false;
            return Sugerencias.Any(s => s.IsSelected);
        }

        private async void ExecuteGenerarBorradorOrden(object obj)
        {
            // 1. Obtener los productos seleccionados por el usuario.
            var sugerenciasSeleccionadas = Sugerencias.Where(s => s.IsSelected).ToList();

            if (!sugerenciasSeleccionadas.Any())
            {
                _notificationService.ShowInfo("Por favor, seleccione al menos un producto para generar una orden.");
                return;
            }

            // --- REFINAMIENTO 1: Lógica del "Mejor Proveedor" ---
            var productosConMejorOpcion = sugerenciasSeleccionadas
                .Select(s => new {
                    Sugerencia = s,
                    // Para cada producto, encontramos la relación Producto-Proveedor con el precio más bajo.
                    MejorOpcion = s.Producto.ProductoProveedores
                                   .OrderBy(pp => pp.PrecioCompra)
                                   .FirstOrDefault()
                })
                .ToList();

            // --- REFINAMIENTO 3: Feedback al usuario ---
            var productosSinProveedor = productosConMejorOpcion.Where(p => p.MejorOpcion == null).ToList();
            if (productosSinProveedor.Any())
            {
                var nombres = string.Join(", ", productosSinProveedor.Select(p => p.Sugerencia.Nombre));
                _notificationService.ShowWarning($"Los siguientes productos no tienen proveedores asignados y serán ignorados: {nombres}");
            }

            // Ahora agrupamos por el ID del proveedor de la "Mejor Opción" que encontramos.
            var gruposPorProveedor = productosConMejorOpcion
                .Where(p => p.MejorOpcion != null) // Filtramos los que sí tienen proveedor.
                .GroupBy(p => p.MejorOpcion.IdProveedor);

            // --- REFINAMIENTO 2: Obtener el Método de Pago por defecto ---
            var metodoPagoDefecto = await _unitOfWork.MetodosPago.GetByConditionAsync(m => m.Descripcion.Contains("Crédito"));
            var idMetodoPagoDefecto = metodoPagoDefecto.FirstOrDefault()?.Id ?? 1;


            IsLoading = true;
            try
            {
                int borradoresCreados = 0;
                // 3. Crear un Pedido en estado "Borrador" por cada proveedor diferente.
                foreach (var grupo in gruposPorProveedor)
                {
                    var idProveedor = grupo.Key;

                    var nuevoPedido = new Pedido
                    {
                        NumeroPedido = $"BORRADOR-{DateTime.Now:yyyyMMddHHmmss}-{borradoresCreados + 1}",
                        FechaEmision = DateTime.Now,
                        FechaEstimadaEntrega = DateTime.Now.AddDays(15), // Valor por defecto
                        Estado = EstadoPedido.Borrador, // Nace como un borrador
                        IdProveedor = idProveedor,
                        IdUsuario = _currentUserService.CurrentUser.Id,
                        IdMetodoPago = 1, // Asumimos un método por defecto, ej: ID 1 = 'Por Definir'
                        DetallesPedidos = new List<DetallePedido>()
                    };

                    foreach (var itemWrapper in grupo)
                    {
                        nuevoPedido.DetallesPedidos.Add(new DetallePedido
                        {
                            IdProducto = itemWrapper.Sugerencia.Producto.Id,
                            Cantidad = itemWrapper.Sugerencia.CantidadSugerida > 0 ? itemWrapper.Sugerencia.CantidadSugerida : 1,
                            PrecioUnitario = itemWrapper.MejorOpcion.PrecioCompra, // Usamos el precio de la mejor opción encontrada.
                            Descripcion = itemWrapper.Sugerencia.Nombre,
                            UnidadMedida = itemWrapper.Sugerencia.Producto.UnidadMedida ?? "Unidad"
                        });
                    }

                    await _unitOfWork.Pedidos.AddAsync(nuevoPedido);
                    borradoresCreados++;
                }

                // 5. Si se creó al menos un borrador, guardamos todo en una sola transacción.
                if (borradoresCreados > 0)
                {
                    await _unitOfWork.CompleteAsync();
                    _notificationService.ShowSuccess($"{borradoresCreados} borrador(es) de órdenes de compra han sido generados exitosamente.");

                    CloseAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ocurrió un error al generar las órdenes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
