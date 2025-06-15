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
    public class MetodoPagoFormViewModel : ViewModelBase
    {
        // --- Dependencias ---
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades de Estado ---
        private MetodoPago _metodoPagoActual;
        private bool _esModoEdicion;

        // --- Propiedades para Binding a la UI ---
        private string _tituloVentana;
        public string TituloVentana
        {
            get => _tituloVentana;
            set => SetProperty(ref _tituloVentana, value);
        }

        private string _codigo;
        public string Codigo
        {
            get => _codigo;
            set => SetProperty(ref _codigo, value);
        }

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        private bool _disponible;
        public bool Disponible
        {
            get => _disponible;
            set => SetProperty(ref _disponible, value);
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        // --- Acción para cerrar la ventana ---
        public Action CloseAction { get; set; }

        // --- Constructor ---
        public MetodoPagoFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCommand = new ViewModelCommand(ExecuteGuardarCommand);
            CancelarCommand = new ViewModelCommand(ExecuteCancelarCommand);
        }

        /// <summary>
        /// Inicializa el ViewModel para un método de pago (editar) o para uno nuevo.
        /// </summary>
        /// <param name="metodoPagoId">El ID del método a editar, o 0 para crear uno nuevo.</param>
        public async Task LoadMetodoPagoAsync(int metodoPagoId)
        {
            if (metodoPagoId == 0) // Modo Creación
            {
                _esModoEdicion = false;
                _metodoPagoActual = new MetodoPago();
                TituloVentana = "Nuevo Método de Pago";
                Disponible = true; // Por defecto, uno nuevo está disponible.
            }
            else // Modo Edición
            {
                _esModoEdicion = true;
                _metodoPagoActual = await _unitOfWork.MetodosPago.GetByIdAsync(metodoPagoId);
                if (_metodoPagoActual != null)
                {
                    TituloVentana = "Editar Método de Pago";
                    Codigo = _metodoPagoActual.Codigo;
                    Descripcion = _metodoPagoActual.Descripcion;
                    Disponible = _metodoPagoActual.Disponible;
                }
                else
                {
                    MessageBox.Show("No se encontró el método de pago solicitado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseAction?.Invoke();
                }
            }
        }

        // --- Lógica de los Comandos ---
        private async void ExecuteGuardarCommand(object obj)
        {
            // --- Validación de Datos ---
            if (string.IsNullOrWhiteSpace(Codigo) || string.IsNullOrWhiteSpace(Descripcion))
            {
                MessageBox.Show("Tanto el Código como la Descripción son obligatorios.", "Validación Fallida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Actualización del Modelo ---
            _metodoPagoActual.Codigo = Codigo.ToUpper(); // Guardamos el código en mayúsculas por consistencia
            _metodoPagoActual.Descripcion = Descripcion;
            _metodoPagoActual.Disponible = Disponible;

            try
            {
                // --- Persistencia en la Base de Datos ---
                if (_esModoEdicion)
                {
                    await _unitOfWork.MetodosPago.UpdateAsync(_metodoPagoActual);
                }
                else
                {
                    await _unitOfWork.MetodosPago.AddAsync(_metodoPagoActual);
                }

                await _unitOfWork.CompleteAsync(); // Confirma la transacción

                MessageBox.Show("Método de pago guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el método de pago.\n\nError: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelarCommand(object obj)
        {
            CloseAction?.Invoke();
        }
    }
}
