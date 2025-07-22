using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace Aplicacion.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        // 1. Inyectamos IUnitOfWork en lugar del DbContext viejo.
        private readonly IUnitOfWork _unitOfWork;

        public CategoriaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoriaViewModel>> GetAllAsync()
        {
            // 2. Usamos el repositorio de categorías a través del UnitOfWork.
            var categorias = await _unitOfWork.Categorias.GetAllAsync();
            return categorias.Select(c => new CategoriaViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            });
        }

        public async Task<CategoriaViewModel?> GetByIdAsync(int id)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria == null) return null;

            return new CategoriaViewModel
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };
        }

        public async Task CreateAsync(CategoriaViewModel model)
        {
            var categoria = new Categoria { Nombre = model.Nombre, Descripcion = model.Descripcion };
            await _unitOfWork.Categorias.AddAsync(categoria);
            // 3. Guardamos los cambios a través de UnitOfWork, que asegura la transacción.
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(CategoriaViewModel model)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(model.Id);
            if (categoria != null)
            {
                categoria.Nombre = model.Nombre;
                categoria.Descripcion = model.Descripcion;
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria != null)
            {
                // 4. IMPORTANTE: Este es un borrado físico.
                // Si una categoría tiene productos asociados, esto lanzará un error
                // de base de datos debido a la restricción de clave foránea.
                // Para evitarlo, deberías primero asegurarte de que ningún producto use esta categoría.
                await _unitOfWork.Categorias.RemoveAsync(categoria);
                await _unitOfWork.CompleteAsync();
            }
        }
    }   
}
