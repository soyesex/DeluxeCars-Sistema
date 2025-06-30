using DeluxeCarsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IMovimientoInventarioRepository : IGenericRepository<MovimientoInventario>
    {
        // Por ahora no necesitamos métodos especializados, pero la interfaz está lista para el futuro.
        // Aquí podrían ir métodos como GetMovimientosPorProductoEnRangoDeFechasAsync(), etc.
    }
}
