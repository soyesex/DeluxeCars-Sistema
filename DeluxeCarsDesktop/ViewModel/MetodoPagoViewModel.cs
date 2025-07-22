using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DeluxeCars.DataAccess.Repositories.Interfaces;

namespace DeluxeCarsDesktop.ViewModel
{
    // RECOMENDACIÓN: Usar este ViewModel para la vista de GESTIÓN de Métodos de Pago.
    public class MetodoPagoViewModel : ViewModelBase , IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Propiedades Públicas para Binding ---
        private ObservableCollection<MetodoPago> _metodosDePago;
        public ObservableCollection<MetodoPago> MetodosDePago
        {
            get => _metodosDePago;
            private set => SetProperty(ref _metodosDePago, value);
        }

        private MetodoPago _metodoSeleccionado;
        public MetodoPago MetodoSeleccionado
        {
            get => _metodoSeleccionado;
            set
            {
                SetProperty(ref _metodoSeleccionado, value);
                (EditarMetodoPagoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (ToggleDisponibilidadCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Comandos ---
        public ICommand NuevoMetodoPagoCommand { get; }
        public ICommand EditarMetodoPagoCommand { get; }
        public ICommand ToggleDisponibilidadCommand { get; }

        // --- Constructor ---
        public MetodoPagoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            MetodosDePago = new ObservableCollection<MetodoPago>();

            NuevoMetodoPagoCommand = new ViewModelCommand(ExecuteNuevoMetodoPagoCommand);
            EditarMetodoPagoCommand = new ViewModelCommand(ExecuteEditarMetodoPagoCommand, CanExecuteActions);
            ToggleDisponibilidadCommand = new ViewModelCommand(ExecuteToggleDisponibilidadCommand, CanExecuteActions);
        }

        // --- Métodos de Lógica ---
        public async Task LoadAsync()
        {
            try
            {
                var metodosDesdeRepo = await _unitOfWork.MetodosPago.GetAllAsync();
                MetodosDePago = new ObservableCollection<MetodoPago>(metodosDesdeRepo.OrderBy(m => m.Descripcion));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los métodos de pago: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteActions(object obj)
        {
            return MetodoSeleccionado != null;
        }

        private async void ExecuteNuevoMetodoPagoCommand(object parameter)
        {
            // Navega al formulario para crear un nuevo método de pago.
            // La recarga de datos se hace para reflejar cualquier cambio.
            await _navigationService.OpenFormWindow(Utils.FormType.MetodoPago,0);
            await LoadAsync();
        }

        private async void ExecuteEditarMetodoPagoCommand(object obj)
        {
            // Le pasamos el ID del producto seleccionado
            await _navigationService.OpenFormWindow(Utils.FormType.Producto, MetodoSeleccionado.Id);
            await LoadAsync();
        }

        private async void ExecuteToggleDisponibilidadCommand(object obj)
        {
            var metodo = MetodoSeleccionado;
            string accion = metodo.Disponible ? "desactivar" : "activar";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} el método de pago '{metodo.Descripcion}'?", "Confirmar Cambio", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                // Cambiamos el estado
                metodo.Disponible = !metodo.Disponible;

                await _unitOfWork.MetodosPago.UpdateAsync(metodo);
                await _unitOfWork.CompleteAsync();

                // Forzamos la actualización en la UI refrescando toda la lista.
                // Es la forma más sencilla de asegurar que los bindings (ej. un color de fondo) se actualicen.
                await LoadAsync();
                MessageBox.Show($"Método de pago {accion}do exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Revertimos el cambio en el objeto si la DB falla.
                metodo.Disponible = !metodo.Disponible;
                MessageBox.Show($"No se pudo cambiar el estado del método de pago.\n\nError: {ex.Message}", "Error de Actualización", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
