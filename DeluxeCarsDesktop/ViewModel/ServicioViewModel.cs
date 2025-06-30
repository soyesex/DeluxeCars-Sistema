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
    public class ServicioViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Estado Interno ---
        private List<Servicio> _todosLosServicios;

        // --- Propiedades para Binding ---
        private string _searchText;
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); FiltrarServicios(); } }

        private ObservableCollection<Servicio> _servicios;
        public ObservableCollection<Servicio> Servicios { get => _servicios; private set => SetProperty(ref _servicios, value); }

        private Servicio _servicioSeleccionado;
        public Servicio ServicioSeleccionado
        {
            get => _servicioSeleccionado;
            set
            {
                SetProperty(ref _servicioSeleccionado, value);
                (EditarServicioCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (ToggleEstadoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Propiedades para el filtro por Tipo de Servicio
        public ObservableCollection<TipoServicio> TiposDeServicio { get; private set; }
        private TipoServicio _tipoServicioFiltro;
        public TipoServicio TipoServicioFiltro { get => _tipoServicioFiltro; set { SetProperty(ref _tipoServicioFiltro, value); FiltrarServicios(); } }

        // --- Comandos ---
        public ICommand NuevoServicioCommand { get; }
        public ICommand EditarServicioCommand { get; }
        public ICommand ToggleEstadoCommand { get; }
        public ICommand ShowTipoServicioViewCommand { get; }

        public ServicioViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todosLosServicios = new List<Servicio>();
            Servicios = new ObservableCollection<Servicio>();
            TiposDeServicio = new ObservableCollection<TipoServicio>();

            NuevoServicioCommand = new ViewModelCommand(ExecuteNuevoServicioCommand);
            EditarServicioCommand = new ViewModelCommand(ExecuteEditarServicioCommand, CanExecuteActions);
            ToggleEstadoCommand = new ViewModelCommand(ExecuteToggleEstadoCommand, CanExecuteActions);
            ShowTipoServicioViewCommand = new ViewModelCommand(ExecuteShowTipoServicioViewCommand);
        }

        public async Task LoadAsync()
        {
            await LoadTiposDeServicioAsync();
            await LoadServiciosAsync();
        }

        private async Task LoadTiposDeServicioAsync()
        {
            try
            {
                var tipos = await _unitOfWork.TiposServicios.GetAllAsync();
                TiposDeServicio.Clear();
                TiposDeServicio.Add(new TipoServicio { Id = 0, Nombre = "Todos los Tipos" });
                foreach (var tipo in tipos.OrderBy(t => t.Nombre))
                {
                    TiposDeServicio.Add(tipo);
                }
                TipoServicioFiltro = TiposDeServicio.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los tipos de servicio: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadServiciosAsync()
        {
            try
            {
                _todosLosServicios = (await _unitOfWork.Servicios.GetAllWithTipoServicioAsync()).ToList();
                FiltrarServicios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los servicios: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarServicios()
        {
            IEnumerable<Servicio> itemsFiltrados = _todosLosServicios;

            if (TipoServicioFiltro != null && TipoServicioFiltro.Id != 0)
            {
                itemsFiltrados = itemsFiltrados.Where(s => s.IdTipoServicio == TipoServicioFiltro.Id);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(s => s.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Servicios = new ObservableCollection<Servicio>(itemsFiltrados.OrderBy(s => s.Nombre));
        }

        private bool CanExecuteActions(object obj) => ServicioSeleccionado != null;

        private async void ExecuteNuevoServicioCommand(object obj)
        {
            await _navigationService.OpenFormWindow(Utils.FormType.Servicio,0);
            await LoadAsync();
        }

        private async void ExecuteEditarServicioCommand(object obj)
        {
            // Le pasamos el ID del producto seleccionado
            await _navigationService.OpenFormWindow(Utils.FormType.Servicio, ServicioSeleccionado.Id);
            await LoadAsync();
        }

        private async void ExecuteToggleEstadoCommand(object obj)
        {
            var servicio = ServicioSeleccionado;
            string accion = servicio.Estado ? "desactivar" : "activar";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} el servicio '{servicio.Nombre}'?", "Confirmar Cambio", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                servicio.Estado = !servicio.Estado;
                await _unitOfWork.Servicios.UpdateAsync(servicio);
                await _unitOfWork.CompleteAsync();

                FiltrarServicios();
                MessageBox.Show($"Servicio {accion}do exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                servicio.Estado = !servicio.Estado; // Revertir si falla
                MessageBox.Show($"No se pudo cambiar el estado del servicio.\n\nError: {ex.Message}", "Error de Actualización", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteShowTipoServicioViewCommand(object obj)
        {
            // Esperamos a que el formulario de Tipos de Servicio se cierre
            await _navigationService.OpenFormWindow(Utils.FormType.TipoServicio, 0);
            // Y después, recargamos solo la lista de tipos para el ComboBox
            await LoadTiposDeServicioAsync();
        }
    }
}
