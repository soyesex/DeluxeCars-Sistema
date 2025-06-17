using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class FacturasHistorialViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private List<Factura> _todasLasFacturas;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FiltrarFacturas();
            }
        }

        private ObservableCollection<Factura> _facturas;
        public ObservableCollection<Factura> Facturas
        {
            get => _facturas;
            private set => SetProperty(ref _facturas, value);
        }

        private Factura _facturaSeleccionada;
        public Factura FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set
            {
                SetProperty(ref _facturaSeleccionada, value);
                (VerDetallesCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (AnularFacturaCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }


        public event Action OnRequestNuevaFactura;
        public ICommand NuevaFacturaCommand { get; }
        public ICommand VerDetallesCommand { get; }
        public ICommand AnularFacturaCommand { get; }
        public ICommand RefrescarCommand { get; }

        public FacturasHistorialViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todasLasFacturas = new List<Factura>();
            Facturas = new ObservableCollection<Factura>();

            VerDetallesCommand = new ViewModelCommand(ExecuteVerDetallesCommand, CanExecuteActions);
            AnularFacturaCommand = new ViewModelCommand(ExecuteAnularFacturaCommand, CanExecuteActions);
            RefrescarCommand = new ViewModelCommand(p => LoadFacturasAsync());

            NuevaFacturaCommand = new ViewModelCommand(p => OnRequestNuevaFactura?.Invoke());
            LoadFacturasAsync();
        }

        private async Task LoadFacturasAsync()
        {
            try
            {
                // Llamamos al nuevo método que incluye los detalles
                var facturasDesdeRepo = await _unitOfWork.Facturas.GetAllWithClienteYMetodoPagoAsync();
                _todasLasFacturas = facturasDesdeRepo.ToList();
                FiltrarFacturas(); // Tu lógica de filtrado
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar las facturas: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarFacturas()
        {
            IEnumerable<Factura> itemsFiltrados = _todasLasFacturas;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLower();
                itemsFiltrados = itemsFiltrados.Where(f =>
                    f.NumeroFactura.ToLower().Contains(lowerSearchText) ||
                    (f.Cliente?.Nombre.ToLower().Contains(lowerSearchText) ?? false) || // Se busca en el nombre del cliente
                    f.Total.ToString().Contains(SearchText) // Se busca en el total como texto
                );
            }

            Facturas = new ObservableCollection<Factura>(itemsFiltrados.OrderByDescending(f => f.FechaEmision));
        }

        private bool CanExecuteActions(object obj) => FacturaSeleccionada != null;

        private void ExecuteVerDetallesCommand(object obj)
        {
            // Lógica para abrir una ventana que muestre los detalles de la FacturaSeleccionada
            MessageBox.Show($"Mostrando detalles para la factura: {FacturaSeleccionada.NumeroFactura}", "Información");
        }

        private async void ExecuteAnularFacturaCommand(object obj)
        {
            // La anulación implica crear una Nota de Crédito. Es una lógica compleja.
            MessageBox.Show("Funcionalidad de anular (Nota de Crédito) pendiente de implementación.", "Información");
        }
    }
}