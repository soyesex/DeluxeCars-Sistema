using DeluxeCars.DataAccess;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aplicacion.Presentation.Controllers // Revisa que tu namespace sea el correcto
{
    [Authorize(Roles = "Administrador")]
    public class ContentAdminController : Controller
    {
        // PASO 2: Cambiar el tipo de ApplicationDbContext a AppDbContext
        private readonly AppDbContext _context;

        public ContentAdminController(AppDbContext context) // <-- Cambiado aquí
        {
            _context = context;
        }

        public class UpdateRequest
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Key))
            {
                return Json(new { success = false, message = "Datos inválidos." });
            }

            // Esta lógica ahora funciona porque AppDbContext sí tiene SiteContents
            var content = await _context.SiteContents.FindAsync(request.Key);

            if (content == null)
            {
                return Json(new { success = false, message = $"Clave '{request.Key}' no encontrada." });
            }

            content.Value = request.Value;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}