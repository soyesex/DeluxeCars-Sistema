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
    public class ServicioFormViewModel : ViewModelBase, IFormViewModel
    {
        // --- Dependencias y Estado ---
        private readonly IUnitOfWork _unitOfWork;
        private Servicio _servicioActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding ---
        private string _tituloVentana;
        public string TituloVentana { get => _tituloVentana; set => SetProperty(ref _tituloVentana, value); }

        private string _nombre;
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }

        private string _descripcion;
        public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }

        private decimal _precio;
        public decimal Precio { get => _precio; set => SetProperty(ref _precio, value); }

        private int? _duracionEstimada;
        public int? DuracionEstimada { get => _duracionEstimada; set => SetProperty(ref _duracionEstimada, value); }

        private bool _estado;
        public bool Estado { get => _estado; set => SetProperty(ref _estado, value); }

        public ObservableCollection<TipoServicio> TiposDeServicio { get; private set; }
        private TipoServicio _tipoServicioSeleccionado;
        public TipoServicio TipoServicioSeleccionado { get => _tipoServicioSeleccionado; set => SetProperty(ref _tipoServicioSeleccionado, value); }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }

        public ServicioFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            TiposDeServicio = new ObservableCollection<TipoServicio>();
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(p => CloseAction?.Invoke());
        }

        public async Task LoadAsync(int servicioId)
        {
            await LoadTiposDeServicioAsync();

            if (servicioId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _servicioActual = new Servicio();
                TituloVentana = "Nuevo Servicio";
                Estado = true;
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _servicioActual = await _unitOfWork.Servicios.GetByIdAsync(servicioId);
                if (_servicioActual != null)
                {
                    TituloVentana = "Editar Servicio";
                    Nombre = _servicioActual.Nombre;
                    Descripcion = _servicioActual.Descripcion;
                    Precio = _servicioActual.Precio;
                    DuracionEstimada = _servicioActual.DuracionEstimada;
                    Estado = _servicioActual.Estado;
                    TipoServicioSeleccionado = TiposDeServicio.FirstOrDefault(t => t.Id == _servicioActual.IdTipoServicio);
                }
                else
                {
                    MessageBox.Show("No se encontró el servicio solicitado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseAction?.Invoke();
                }
            }
        }

        private async Task LoadTiposDeServicioAsync()
        {
            try
            {
                var tipos = await _unitOfWork.TiposServicios.GetAllAsync();
                TiposDeServicio = new ObservableCollection<TipoServicio>(tipos.OrderBy(t => t.Nombre));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los tipos de servicio: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Nombre) || Precio < 0 || TipoServicioSeleccionado == null)
            {
                MessageBox.Show("Nombre, Precio (no negativo) y Tipo de Servicio son obligatorios.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _servicioActual.Nombre = Nombre;
            _servicioActual.Descripcion = Descripcion;
            _servicioActual.Precio = Precio;
            _servicioActual.DuracionEstimada = DuracionEstimada;
            _servicioActual.Estado = Estado;
            _servicioActual.IdTipoServicio = TipoServicioSeleccionado.Id;

            try
            {
                if (_esModoEdicion)
                {
                    await _unitOfWork.Servicios.UpdateAsync(_servicioActual);
                }
                else
                {
                    await _unitOfWork.Servicios.AddAsync(_servicioActual);
                }
                await _unitOfWork.CompleteAsync();

                MessageBox.Show("Servicio guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el servicio: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
