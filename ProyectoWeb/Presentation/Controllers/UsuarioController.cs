using Aplicacion.Application.Services;
using Aplicacion.Application.ViewModels;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        // 1. Inyectamos el servicio de usuarios correcto.
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            // 2. Simplemente llamamos al método del servicio.
            //    Él se encarga de todo y ya devuelve la lista de ViewModels.
            var usuarios = await _usuarioService.GetAllUsersWithRolesAsync();
            return View(usuarios);
        }
    }
}
