using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ConfiguracionViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;

        // --- Propiedades para la Vista ---
        private string _nombreTienda;
        public string NombreTienda
        {
            get => _nombreTienda;
            set => SetProperty(ref _nombreTienda, value);
        }

        private string _direccion;
        public string Direccion
        {
            get => _direccion;
            set => SetProperty(ref _direccion, value);
        }

        private string _telefono;
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _horarioAtencion;
        public string HorarioAtencion
        {
            get => _horarioAtencion;
            set => SetProperty(ref _horarioAtencion, value);
        }

        private decimal _porcentajeIVA;
        public decimal PorcentajeIVA
        {
            get => _porcentajeIVA;
            set => SetProperty(ref _porcentajeIVA, value);
        }

        // --- Comandos ---
        public ICommand GuardarCambiosCommand { get; }
        public ICommand CambiarPinCommand { get; }
        public ICommand CambiarLogoCommand { get; }
        public ICommand CambiarBannerCommand { get; }

        public ConfiguracionViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuardarCambiosCommand = new ViewModelCommand(async _ => await ExecuteGuardarCambios());
            // Inicializamos los otros comandos (la lógica se añadirá en el futuro)
            CambiarPinCommand = new ViewModelCommand(p => { /* Lógica futura para cambiar PIN */ });
            CambiarLogoCommand = new ViewModelCommand(p => { /* Lógica futura para cambiar logo */ });
            CambiarBannerCommand = new ViewModelCommand(p => { /* Lógica futura para cambiar banner */ });
        }

        // Método de inicialización asíncrona para cargar los datos
        public async Task LoadAsync()
        {
            // Buscamos la única fila de configuración (la que tiene Id = 1)
            var config = await _unitOfWork.Configuraciones.GetByIdAsync(1);
            if (config != null)
            {
                NombreTienda = config.NombreTienda;
                Direccion = config.Direccion;
                Telefono = config.Telefono;
                Email = config.Email;
                HorarioAtencion = config.HorarioAtencion;
                PorcentajeIVA = config.PorcentajeIVA;
            }
            else
            {
                // Manejar el caso de que la fila no exista (aunque el Seeding debería prevenirlo)
                // Podríamos mostrar un error o usar valores por defecto.
                ShowTemporaryErrorMessage("No se encontró la configuración de la empresa.", 10);
            }
        }

        private async Task ExecuteGuardarCambios()
        {
            var config = await _unitOfWork.Configuraciones.GetByIdAsync(1);
            if (config != null)
            {
                // Actualizamos el objeto de la entidad con los valores de las propiedades del ViewModel
                config.NombreTienda = this.NombreTienda;
                config.Direccion = this.Direccion;
                config.Telefono = this.Telefono;
                config.Email = this.Email;
                config.HorarioAtencion = this.HorarioAtencion;
                config.PorcentajeIVA = this.PorcentajeIVA;

                // Le decimos a UnitOfWork que la entidad ha sido modificada
                _unitOfWork.Configuraciones.UpdateAsync(config);

                // Guardamos los cambios en la base de datos
                await _unitOfWork.CompleteAsync();

                // Notificamos al usuario que todo salió bien
                ShowTemporaryErrorMessage("✅ Configuración guardada exitosamente.", 5);
            }
            else
            {
                ShowTemporaryErrorMessage("Error: No se encontró la configuración para guardar.", 10);
            }
        }
    }
}
