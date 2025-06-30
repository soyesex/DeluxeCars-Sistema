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

namespace DeluxeCarsDesktop.ViewModel
{
    public class ProveedorViewModel : ViewModelBase, IAsyncLoadable
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;

        // --- Listas Maestras (Caché) ---
        private List<Proveedor> _todosLosProveedores;
        private List<Municipio> _todosLosMunicipios;

        // --- Propiedades Públicas para Binding ---
        public bool IsViewVisible { get; set; } = true;

        private string _searchText;
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); FiltrarProveedores(); } }

        private ObservableCollection<Proveedor> _proveedores;
        public ObservableCollection<Proveedor> Proveedores { get => _proveedores; private set => SetProperty(ref _proveedores, value); }

        private Proveedor _proveedorSeleccionado;
        public Proveedor ProveedorSeleccionado
        {
            get => _proveedorSeleccionado;
            set
            {
                SetProperty(ref _proveedorSeleccionado, value);
                (EditarProveedorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (ToggleEstadoCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- NUEVAS PROPIEDADES PARA FILTROS DE UBICACIÓN ---
        public ObservableCollection<Departamento> Departamentos { get; private set; }
        public ObservableCollection<Municipio> MunicipiosDisponibles { get; private set; }

        private Departamento _departamentoFiltro;
        public Departamento DepartamentoFiltro
        {
            get => _departamentoFiltro;
            set
            {
                SetProperty(ref _departamentoFiltro, value);
                CargarMunicipiosPorDepartamento(); // Lógica en cascada
                FiltrarProveedores();
            }
        }

        private Municipio _municipioFiltro;
        public Municipio MunicipioFiltro { get => _municipioFiltro; set { SetProperty(ref _municipioFiltro, value); FiltrarProveedores(); } }

        // --- Comandos ---
        public ICommand NuevoProveedorCommand { get; }
        public ICommand EditarProveedorCommand { get; }
        public ICommand ToggleEstadoCommand { get; }
        public ICommand GestionarProductosCommand { get; }

        // --- Constructor ---
        public ProveedorViewModel(IUnitOfWork unitOfWork, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;

            _todosLosProveedores = new List<Proveedor>();
            _todosLosMunicipios = new List<Municipio>();
            Proveedores = new ObservableCollection<Proveedor>();
            Departamentos = new ObservableCollection<Departamento>();
            MunicipiosDisponibles = new ObservableCollection<Municipio>();

            NuevoProveedorCommand = new ViewModelCommand(async p => await ExecuteNuevoProveedorCommand());
            EditarProveedorCommand = new ViewModelCommand(async p => await ExecuteEditarProveedorCommand(), p => CanExecuteActions());
            ToggleEstadoCommand = new ViewModelCommand(async p => await ExecuteToggleEstadoCommand(), p => CanExecuteActions());

            GestionarProductosCommand = new ViewModelCommand( async p => await ExecuteGestionarProductos(), p => ProveedorSeleccionado != null);
        }

        // --- Métodos de Lógica ---

        public async Task LoadAsync()
        {
            await LoadUbicacionesAsync();
            await LoadProveedoresAsync();
        }

        private async Task LoadUbicacionesAsync()
        {
            try
            {
                var depts = await _unitOfWork.Departamentos.GetAllAsync();
                _todosLosMunicipios = (await _unitOfWork.Municipios.GetAllAsync()).ToList();

                Departamentos.Clear();
                Departamentos.Add(new Departamento { Id = 0, Nombre = "Todos los Deptos." });
                foreach (var d in depts.OrderBy(d => d.Nombre)) Departamentos.Add(d);

                DepartamentoFiltro = Departamentos.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando ubicaciones: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarMunicipiosPorDepartamento()
        {
            var municipioSeleccionadoAnteriormente = MunicipioFiltro;
            MunicipiosDisponibles.Clear();
            MunicipiosDisponibles.Add(new Municipio { Id = 0, Nombre = "Todos los Municipios" });

            if (DepartamentoFiltro != null && DepartamentoFiltro.Id != 0)
            {
                foreach (var m in _todosLosMunicipios.Where(m => m.IdDepartamento == DepartamentoFiltro.Id).OrderBy(m => m.Nombre))
                {
                    MunicipiosDisponibles.Add(m);
                }
            }

            // Intentar re-seleccionar el municipio si aún existe en la nueva lista
            MunicipioFiltro = MunicipiosDisponibles.FirstOrDefault(m => m.Id == municipioSeleccionadoAnteriormente?.Id) ?? MunicipiosDisponibles.FirstOrDefault();
        }

        private async Task LoadProveedoresAsync()
        {
            try
            {
                var proveedoresDesdeRepo = await _unitOfWork.Proveedores.GetAllWithLocationAsync();
                _todosLosProveedores = proveedoresDesdeRepo.ToList();
                FiltrarProveedores();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los proveedores: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MÉTODO DE FILTRADO MEJORADO ---
        private async Task ExecuteGestionarProductos()
        {
            // Necesitarás un nuevo FormType para esto
            await _navigationService.OpenFormWindow(FormType.GestionarProductosProveedor, ProveedorSeleccionado.Id);
        }

        // En ProveedorViewModel.cs
        private void FiltrarProveedores()
        {
            // Empezamos con la lista completa de proveedores que tenemos en caché.
            IEnumerable<Proveedor> itemsFiltrados = _todosLosProveedores;

            // Aplicamos el filtro de Departamento
            if (DepartamentoFiltro != null && DepartamentoFiltro.Id != 0)
            {
                // Usamos el '?' (operador de anulación de referencia) para evitar errores si un proveedor no tiene municipio.
                itemsFiltrados = itemsFiltrados.Where(p => p.Municipio?.IdDepartamento == DepartamentoFiltro.Id);
            }

            // Aplicamos el filtro de Municipio
            if (MunicipioFiltro != null && MunicipioFiltro.Id != 0)
            {
                itemsFiltrados = itemsFiltrados.Where(p => p.IdMunicipio == MunicipioFiltro.Id);
            }

            // Aplicamos el filtro de texto
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                itemsFiltrados = itemsFiltrados.Where(p => p.RazonSocial.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // --- LA CORRECCIÓN CLAVE ESTÁ AQUÍ ---
            // 1. Limpiamos la colección que la UI está observando.
            Proveedores.Clear();

            // 2. Llenamos ESA MISMA colección con los resultados filtrados y ordenados.
            foreach (var proveedor in itemsFiltrados.OrderBy(p => p.RazonSocial))
            {
                Proveedores.Add(proveedor);
            }
        }

        private bool CanExecuteActions() => ProveedorSeleccionado != null;

        private async Task ExecuteNuevoProveedorCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Proveedor, 0);
            await LoadProveedoresAsync();
        }

        private async Task ExecuteEditarProveedorCommand()
        {
            await _navigationService.OpenFormWindow(FormType.Proveedor, ProveedorSeleccionado.Id);
            await LoadProveedoresAsync();
        }

        private async Task ExecuteToggleEstadoCommand()
        {
            var proveedorId = ProveedorSeleccionado.Id; // Captura el Id
            string accion = ProveedorSeleccionado.Estado ? "desactivar" : "activar";

            var result = MessageBox.Show($"¿Estás seguro de que deseas {accion} al proveedor '{ProveedorSeleccionado.RazonSocial}'?", "Confirmar Cambio", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                // Cargar la entidad desde el contexto actual de la UoW
                var proveedorToUpdate = await _unitOfWork.Proveedores.GetByIdAsync(proveedorId);

                if (proveedorToUpdate == null)
                {
                    MessageBox.Show("El proveedor no fue encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                proveedorToUpdate.Estado = !proveedorToUpdate.Estado; // Modifica la entidad trackeada

                // No necesitas llamar a UpdateAsync si tu UnitOfWork ya maneja el seguimiento
                // Simplemente el CompleteAsync (SaveChanges) detectará el cambio
                await _unitOfWork.CompleteAsync();

                FiltrarProveedores(); // Refresca la UI
                MessageBox.Show($"Proveedor {accion}do exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cambiar el estado del proveedor.\n\nError: {ex.Message}", "Error de Actualización", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
