namespace Aplicacion.Core.Interfaces
{
    public interface IContentService
    {
        Task<Dictionary<string, string>> GetPageContentAsync(string pageName);
        Task UpdateContentAsync(string key, string value);
    }
}
