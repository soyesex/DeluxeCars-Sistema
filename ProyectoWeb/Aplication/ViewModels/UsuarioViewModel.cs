namespace Aplicacion.Application.ViewModels
{
    public class UsuarioViewModel
    {
        // El ID en Identity es un string (GUID)
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        // La lista de roles que tiene el usuario
        public IEnumerable<string> Roles { get; set; }

    }
}
