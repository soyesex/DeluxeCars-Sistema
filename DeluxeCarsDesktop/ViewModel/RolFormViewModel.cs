using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class RolFormViewModel : ViewModelBase, IFormViewModel, ICloseable
    {
        private readonly IUnitOfWork _unitOfWork;
        private Rol _rolActual;
        private bool _esModoEdicion;

        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public Action CloseAction { get; set; }

        public RolFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(p => CloseAction?.Invoke());
        }

        public async Task LoadAsync(int entityId)
        {
            if (entityId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _rolActual = new Rol();
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _rolActual = await _unitOfWork.Roles.GetByIdAsync(entityId);
                if (_rolActual != null)
                {
                    Nombre = _rolActual.Nombre;
                    Descripcion = _rolActual.Descripcion;
                }
            }
            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Descripcion));
        }

        private async void ExecuteGuardarCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("El nombre del rol es obligatorio.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _rolActual.Nombre = Nombre;
            _rolActual.Descripcion = Descripcion;

            try
            {
                if (_esModoEdicion)
                    await _unitOfWork.Roles.UpdateAsync(_rolActual);
                else
                    await _unitOfWork.Roles.AddAsync(_rolActual);

                await _unitOfWork.CompleteAsync();
                MessageBox.Show("Rol guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el rol: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
