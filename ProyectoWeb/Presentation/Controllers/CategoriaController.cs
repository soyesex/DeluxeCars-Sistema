
using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Implementations;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Categoria/
        public async Task<IActionResult> Index()
        {
            // Correcto: Llamar al repositorio de Categorías
            var categorias = await _unitOfWork.Categorias.GetAllAsync();

            // Mapeamos de la entidad al ViewModel para la vista
            var viewModels = categorias.Select(c => new CategoriaViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            });

            return View(viewModels);
        }

        // GET: /Categoria/Crear
        public IActionResult Crear()
        {
            return View(new CategoriaViewModel());
        }

        // POST: /Categoria/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mapeamos del ViewModel a la Entidad para guardarla
                var nuevaCategoria = new Categoria
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                };

                await _unitOfWork.Categorias.AddAsync(nuevaCategoria);
                await _unitOfWork.CompleteAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Categoria/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria == null) return NotFound();

            var model = new CategoriaViewModel
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return View(model);
        }

        // POST: /Categoria/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CategoriaViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
                if (categoria == null) return NotFound();

                categoria.Nombre = model.Nombre;
                categoria.Descripcion = model.Descripcion;

                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Categoria/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // Advertencia: Este es un borrado físico.
            // Si algún producto está usando esta categoría, la base de datos lanzará un error.
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria != null)
            {
                _unitOfWork.Categorias.RemoveAsync(categoria);
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
