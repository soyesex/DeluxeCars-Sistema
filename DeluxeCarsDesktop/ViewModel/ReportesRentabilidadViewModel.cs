using DeluxeCarsDesktop.Dtos;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class ReportesRentabilidadViewModel : ViewModelBase, IAsyncLoadable
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObservableCollection<ReporteRentabilidadDto> Resultados { get; private set; }

        private DateTime _fechaInicio = DateTime.Now.AddMonths(-1);
        public DateTime FechaInicio { get => _fechaInicio; set => SetProperty(ref _fechaInicio, value); }

        private DateTime _fechaFin = DateTime.Now;
        public DateTime FechaFin { get => _fechaFin; set => SetProperty(ref _fechaFin, value); }

        public ICommand CargarReporteCommand { get; }

        public ReportesRentabilidadViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Resultados = new ObservableCollection<ReporteRentabilidadDto>();
            CargarReporteCommand = new ViewModelCommand(async _ => await LoadAsync());
        }

        public async Task LoadAsync()
        {
            try
            {
                var reporteData = await _unitOfWork.Facturas.GetReporteRentabilidadAsync(FechaInicio, FechaFin);
                Resultados.Clear();
                foreach (var item in reporteData)
                {
                    Resultados.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Manejar error, mostrar un mensaje al usuario
            }
        }
    }
}
