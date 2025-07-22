using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    public class AjusteInventarioViewModel : ViewModelBase, ICloseable, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockAlertService _stockAlertService;

        // --- AÑADE ESTO ---
        // Lista maestra que contiene TODOS los productos. No se modifica después de la carga inicial.
        private List<Producto> _todosLosProductos;

        // --- AÑADE ESTO ---
        // Propiedad para el texto de búsqueda, bindeada al TextBox.
        private string _textoDeBusqueda;
        public string TextoDeBusqueda
        {
            get => _textoDeBusqueda;
            set
            {

                SetProperty(ref _textoDeBusqueda, value);
                // Cada vez que el texto cambia, filtramos la lista.
                FiltrarProductos();
            }
        }

        // --- RENOMBRA ESTO --- (Antes: ProductosDisponibles)
        // Esta colección ahora solo contiene los productos filtrados para mostrar en la UI.
        public ObservableCollection<Producto> ProductosFiltrados { get; private set; }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado { get => _productoSeleccionado; set { SetProperty(ref _productoSeleccionado, value); (GuardarAjusteCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        private int _cantidad;
        public int Cantidad { get => _cantidad; set { SetProperty(ref _cantidad, value); (GuardarAjusteCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        private bool _esEntrada = true;
        public bool EsEntrada { get => _esEntrada; set => SetProperty(ref _esEntrada, value); }
        public bool EsSalida
        {
            get => !_esEntrada;
            set => EsEntrada = !value;
        }

        private string _motivo;
        public string Motivo { get => _motivo; set { SetProperty(ref _motivo, value); (GuardarAjusteCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        public ICommand GuardarAjusteCommand { get; }
        public Action CloseAction { get; set; }

        public AjusteInventarioViewModel(IUnitOfWork unitOfWork, IStockAlertService stockAlertService)
        {
            _unitOfWork = unitOfWork;
            _stockAlertService = stockAlertService;

            _todosLosProductos = new List<Producto>();
            // --- MODIFICA ESTO ---
            ProductosFiltrados = new ObservableCollection<Producto>();
            GuardarAjusteCommand = new ViewModelCommand(async p => await ExecuteGuardarAjuste(), p => CanExecuteGuardar());
        }

        // --- MODIFICA ESTO ---
        // El método LoadAsync ahora carga la lista maestra y luego la filtra.
        public async Task LoadAsync()
        {

            // 1. Obtenemos todos los productos y los guardamos en nuestra lista maestra.
            var productos = await _unitOfWork.Productos.GetAllAsync();
            _todosLosProductos = new List<Producto>(productos);

            // 2. Ejecutamos el filtro inicial para mostrar todos los productos al principio.
            FiltrarProductos();
        }

        // --- AÑADE ESTO ---
        // Nueva lógica centralizada para el filtrado de productos.
        private void FiltrarProductos()
        {

            // Limpiamos la lista actual de la UI.
            ProductosFiltrados.Clear();

            // Verificamos si hay un texto de búsqueda.
            if (string.IsNullOrWhiteSpace(TextoDeBusqueda))
            {
                // Si no hay búsqueda, añadimos todos los productos de la lista maestra.
                foreach (var producto in _todosLosProductos)
                {
                    ProductosFiltrados.Add(producto);
                }
            }
            else
            {
                // Si hay búsqueda, filtramos la lista maestra y añadimos los resultados.
                var productosCoincidentes = _todosLosProductos.Where(p =>
                    p.Nombre.Contains(TextoDeBusqueda, StringComparison.OrdinalIgnoreCase) ||
                    (p.OriginalEquipamentManufacture ?? "").Contains(TextoDeBusqueda, StringComparison.OrdinalIgnoreCase)
                );

                foreach (var producto in productosCoincidentes)
                {
                    ProductosFiltrados.Add(producto);
                }
            }
        }

        private bool CanExecuteGuardar()
        {
            return ProductoSeleccionado != null && Cantidad != 0 && !string.IsNullOrWhiteSpace(Motivo);
        }

        private async Task ExecuteGuardarAjuste()
        {
            // Tu lógica de negocio original se mantiene intacta, ¡es perfecta!
            var tipo = EsEntrada ? "Ajuste Positivo" : "Ajuste Negativo";
            var cantidadAjuste = EsEntrada ? Math.Abs(Cantidad) : -Math.Abs(Cantidad);

            var movimiento = new MovimientoInventario
            {
                IdProducto = ProductoSeleccionado.Id,
                Fecha = DateTime.UtcNow,
                TipoMovimiento = tipo,
                Cantidad = cantidadAjuste,
                MotivoAjuste = this.Motivo
            };
            try
            {
                await _unitOfWork.Context.MovimientosInventario.AddAsync(movimiento);
                await _unitOfWork.CompleteAsync();

                if (cantidadAjuste < 0)
                {
                    try
                    {
                        await _stockAlertService.CheckAndCreateStockAlertAsync(ProductoSeleccionado.Id, _unitOfWork);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al verificar alertas de stock post-ajuste: {ex.Message}");
                    }
                }

                System.Windows.MessageBox.Show("Ajuste de inventario guardado exitosamente.", "Éxito");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ocurrió un error al guardar el ajuste: {ex.Message}", "Error de Guardado");
            }
        }
    }
}
