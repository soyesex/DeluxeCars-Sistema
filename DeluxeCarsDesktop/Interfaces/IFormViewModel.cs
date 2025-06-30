using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface IFormViewModel
    {
        /// <summary>
        /// Carga los datos necesarios para el formulario, ya sea para una nueva entidad o para editar una existente.
        /// </summary>
        /// <param name="entityId">El ID de la entidad a editar. Será 0 si es una nueva entidad.</param>
        Task LoadAsync(int entityId);
    }
}
