using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class AjusteInventarioViewModel : ViewModelBase, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades para el Binding ---
        public ObservableCollection<Producto> ProductosDisponibles { get; private set; }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado { get => _productoSeleccionado; set { SetProperty(ref _productoSeleccionado, value); (GuardarAjusteCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        private int _cantidad;
        public int Cantidad { get => _cantidad; set { SetProperty(ref _cantidad, value); (GuardarAjusteCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        private bool _esEntrada = true; // Por defecto, es un ajuste positivo
        public bool EsEntrada { get => _esEntrada; set => SetProperty(ref _esEntrada, value); }
        // --- AÑADE ESTA NUEVA PROPIEDAD ---
        public bool EsSalida
        {
            get => !_esEntrada;
            // Cuando el usuario haga clic en este RadioButton, actualizará la propiedad principal
            set => EsEntrada = !value;
        }

        private string _motivo;
        public string Motivo { get => _motivo; set { SetProperty(ref _motivo, value); (GuardarAjusteCommand as ViewModelCommand)?.RaiseCanExecuteChanged(); } }

        // --- Comandos ---
        public ICommand GuardarAjusteCommand { get; }
        public Action CloseAction { get; set; }

        public AjusteInventarioViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ProductosDisponibles = new ObservableCollection<Producto>();
            GuardarAjusteCommand = new ViewModelCommand(async p => await ExecuteGuardarAjuste(), p => CanExecuteGuardar());
            LoadProductosAsync();
        }

        private async void LoadProductosAsync()
        {
            var productos = await _unitOfWork.Productos.GetAllAsync();
            ProductosDisponibles.Clear();
            foreach (var p in productos)
            {
                ProductosDisponibles.Add(p);
            }
        }

        private bool CanExecuteGuardar()
        {
            return ProductoSeleccionado != null && Cantidad != 0 && !string.IsNullOrWhiteSpace(Motivo);
        }

        private async Task ExecuteGuardarAjuste()
        {
            // Determinamos el tipo de movimiento y la cantidad (positiva o negativa)
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

            await _unitOfWork.Context.MovimientosInventario.AddAsync(movimiento);
            await _unitOfWork.CompleteAsync();

            System.Windows.MessageBox.Show("Ajuste de inventario guardado exitosamente.", "Éxito");
            CloseAction?.Invoke();
        }
    }
}
