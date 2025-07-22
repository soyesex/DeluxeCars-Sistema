using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsShared.Dtos
{
    public class ProveedorSearchCriteria
    {
        public string? SearchText { get; set; }
        public int? DepartamentoId { get; set; }
        public int? MunicipioId { get; set; }
        public bool? Estado { get; set; } // Para filtrar por activos/inactivos

        // Paginación
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }
}
