using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel
{
    public class FacturaDetalleViewModel : ViewModelBase, IFormViewModel // Asegúrate de que implemente tu interfaz
    {
        private readonly IUnitOfWork _unitOfWork;

        private Factura _factura;
        public Factura Factura
        {
            get => _factura;
            private set => SetProperty(ref _factura, value);
        }

        // Constructor para el Inyector de Dependencias (ya no recibe el ID)
        public FacturaDetalleViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Método de la interfaz para cargar los datos
        public async Task LoadAsync(int facturaId)
        {
            if (facturaId > 0)
            {
                Factura = await _unitOfWork.Facturas.GetFacturaWithDetailsAsync(facturaId);
            }
        }
    
    }
}
