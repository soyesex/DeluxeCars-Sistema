using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Application.Services
{
    public class ContentService : IContentService
    {
        // 1. Inyectamos IUnitOfWork en lugar del DbContext viejo
        private readonly IUnitOfWork _unitOfWork;

        public ContentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Dictionary<string, string>> GetPageContentAsync(string pageName)
        {
            // 2. Usamos el repositorio a través del UnitOfWork
            var content = await _unitOfWork.Context.SiteContents
                .Where(c => c.Key.StartsWith(pageName))
                .AsNoTracking() // Buena práctica para consultas de solo lectura
                .ToDictionaryAsync(c => c.Key, c => c.Value);

            return content;
        }

        public async Task UpdateContentAsync(string key, string value)
        {
            var content = await _unitOfWork.Context.SiteContents.FindAsync(key);
            if (content != null)
            {
                content.Value = value;
                // 3. Guardamos los cambios a través del UnitOfWork
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
