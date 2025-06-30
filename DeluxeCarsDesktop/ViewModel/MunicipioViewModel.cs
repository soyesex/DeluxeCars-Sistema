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

namespace DeluxeCarsDesktop.ViewModel
{
    public class MunicipioViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Listas Maestras (Caché) ---
        private List<Municipio> _todosLosMunicipios;

        // --- Propiedades Públicas para Binding ---
        private string _searchText;
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); FiltrarMunicipios(); } }

        private ObservableCollection<Municipio> _municipios;
        public ObservableCollection<Municipio> Municipios { get => _municipios; private set => SetProperty(ref _municipios, value); }

        private Municipio _municipioSeleccionado;
        public Municipio MunicipioSeleccionado
        {
            get => _municipioSeleccionado;
            set
            {
                SetProperty(ref _municipioSeleccionado, value);
                (EditarMunicipioCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (ToggleEstadoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Propiedades para el filtro por Departamento
        private ObservableCollection<Departamento> _departamentos;
        public ObservableCollection<Departamento> Departamentos { get => _departamentos; private set => SetProperty(ref _departamentos, value); }

        private Departamento _departamentoFiltro;
        public Departamento DepartamentoFiltro { get => _departamentoFiltro; set { SetProperty(ref _departamentoFiltro, value); FiltrarMunicipios(); } }

        // --- Comandos ---
        public ICommand NuevoMunicipioCommand { get; }
        public ICommand EditarMunicipioCommand { get; }
        public ICommand ToggleEstadoCommand { get; }

        // --- Constructor ---
        public MunicipioViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todosLosMunicipios = new List<Municipio>();
            Municipios = new ObservableCollection<Municipio>();
            Departamentos = new ObservableCollection<Departamento>();

            NuevoMunicipioCommand = new ViewModelCommand(ExecuteNuevoMunicipioCommand);
            EditarMunicipioCommand = new ViewModelCommand(ExecuteEditarMunicipioCommand, CanExecuteActions);
            ToggleEstadoCommand = new ViewModelCommand(ExecuteToggleEstadoCommand, CanExecuteActions);
        }

        // --- Métodos de Lógica ---
        public async Task LoadAsync()
        {
            await LoadDepartamentosAsync();
            await LoadMunicipiosAsync();
        }

        private async Task LoadDepartamentosAsync()
        {
            try
            {
                var depts = await _unitOfWork.Departamentos.GetAllAsync();
                Departamentos.Clear();
                // Añadimos una opción para ver todos los municipios sin filtrar
                Departamentos.Add(new Departamento { Id = 0, Nombre = "Todos los Departamentos" });
                foreach (var dept in depts.OrderBy(d => d.Nombre))
                {
                    Departamentos.Add(dept);
                }
                // Seleccionamos la opción "Todos" por defecto
                DepartamentoFiltro = Departamentos.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los departamentos: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadMunicipiosAsync()
        {
            try
            {
                // ¡Importante! Usamos Include para traer los datos del Departamento asociado a cada Municipio.
                // Esto nos permite mostrar el nombre del departamento en la tabla.
                _todosLosMunicipios = (await _unitOfWork.Municipios.GetAllWithDepartamentosAsync()).ToList();
                FiltrarMunicipios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los municipios: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarMunicipios()
        {
            IEnumerable<Municipio> itemsFiltrados = _todosLosMunicipios;

            // 1. Primer filtro: por Departamento
            if (DepartamentoFiltro != null && DepartamentoFiltro.Id != 0)
            {
                itemsFiltrados = itemsFiltrados.Where(m => m.IdDepartamento == DepartamentoFiltro.Id);
            }

            // 2. Segundo filtro: por texto de búsqueda (sobre el resultado anterior)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(m => m.Nombre.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Municipios = new ObservableCollection<Municipio>(itemsFiltrados.OrderBy(m => m.Nombre));
        }

        private bool CanExecuteActions(object obj) => MunicipioSeleccionado != null;

        private async void ExecuteNuevoMunicipioCommand(object obj)
        {
            await _navigationService.OpenFormWindow(Utils.FormType.Municipio, 0);
            await LoadAsync(); // Recargamos todo por si hay nuevos departamentos o municipios
        }

        private async void ExecuteEditarMunicipioCommand(object obj)
        {
            // Le pasamos el ID del producto seleccionado
            await _navigationService.OpenFormWindow(Utils.FormType.Municipio, MunicipioSeleccionado.Id);
            await LoadAsync();
        }

        private async void ExecuteToggleEstadoCommand(object obj)
        {
            var municipio = MunicipioSeleccionado;
            string accion = municipio.Estado ? "desactivar" : "activar";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} el municipio '{municipio.Nombre}'?", "Confirmar Cambio", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                municipio.Estado = !municipio.Estado;
                await _unitOfWork.Municipios.UpdateAsync(municipio);
                await _unitOfWork.CompleteAsync();

                // Refrescamos la UI
                FiltrarMunicipios();
                MessageBox.Show($"Municipio {accion}do exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                municipio.Estado = !municipio.Estado; // Revertir el cambio si falla
                MessageBox.Show($"No se pudo cambiar el estado del municipio.\n\nError: {ex.Message}", "Error de Actualización", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
