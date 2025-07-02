using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using DeluxeCarsDesktop.Services;
using DeluxeCarsDesktop.Utils;
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
    public class DepartamentoViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Estado Interno ---
        private List<Departamento> _todosLosDepartamentos;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                // Usando el helper que acabamos de explicar
                SetProperty(ref _searchText, value);
                FiltrarDepartamentos();
            }
        }

        private ObservableCollection<Departamento> _departamentos;
        public ObservableCollection<Departamento> Departamentos
        {
            get => _departamentos;
            private set => SetProperty(ref _departamentos, value);
        }

        private Departamento _departamentoSeleccionado;
        public Departamento DepartamentoSeleccionado
        {
            get => _departamentoSeleccionado;
            set
            {
                SetProperty(ref _departamentoSeleccionado, value);
                // Actualizamos el estado de los comandos cuando cambia la selección
                (EditarDepartamentoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (EliminarDepartamentoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }
        // --- Comandos ---
        public ICommand NuevoDepartamentoCommand { get; }
        public ICommand EditarDepartamentoCommand { get; }
        public ICommand EliminarDepartamentoCommand { get; }

        // --- Constructor ---
        public DepartamentoViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todosLosDepartamentos = new List<Departamento>();
            Departamentos = new ObservableCollection<Departamento>();

            NuevoDepartamentoCommand = new ViewModelCommand(ExecuteNuevoDepartamentoCommand);
            EditarDepartamentoCommand = new ViewModelCommand(ExecuteEditarDepartamentoCommand, CanExecuteEditDelete);
            EliminarDepartamentoCommand = new ViewModelCommand(ExecuteEliminarDepartamentoCommand, CanExecuteEditDelete);
        }

        // --- Métodos de Lógica ---
        public async Task LoadAsync()
        {
            try
            {
                var departamentosDesdeRepo = await _unitOfWork.Departamentos.GetAllAsync();
                _todosLosDepartamentos = departamentosDesdeRepo.ToList();
                FiltrarDepartamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los clientes: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarDepartamentos()
        {
            IEnumerable<Departamento> itemsFiltrados = _todosLosDepartamentos;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(d =>
                    d.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Departamentos = new ObservableCollection<Departamento>(itemsFiltrados.OrderBy(d => d.Nombre));
        }
        private bool CanExecuteEditDelete(object obj)
        {
            return DepartamentoSeleccionado != null;
        }
        private async void ExecuteNuevoDepartamentoCommand(object parameter)
        {
            // Se usa 'await' para esperar a que el formulario se cierre.
            await _navigationService.OpenFormWindow(FormType.Departamento, 0);
            // La recarga ocurre DESPUÉS de cerrar el formulario.
            await LoadAsync();
        }
        private async void ExecuteEditarDepartamentoCommand(object obj)
        {
            await _navigationService.OpenFormWindow(FormType.Departamento, DepartamentoSeleccionado.Id);
            await LoadAsync();
        }
        private async void ExecuteEliminarDepartamentoCommand(object obj)
        {
            var departamentoAEliminar = DepartamentoSeleccionado;

            var result = MessageBox.Show($"¿Estás seguro de que deseas eliminar el departamento'{departamentoAEliminar.Nombre}'?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            try
            {
                await _unitOfWork.Departamentos.RemoveAsync(departamentoAEliminar);
                await _unitOfWork.CompleteAsync();

                _todosLosDepartamentos.Remove(departamentoAEliminar);
                FiltrarDepartamentos();

                MessageBox.Show("Departamento eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar ese departamento.\n\nError: {ex.Message}", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
