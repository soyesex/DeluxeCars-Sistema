namespace Aplicacion.Models
{
    [Tags("TiposServicios")]
    public class TipoServicio 
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public ICollection<Servicio> Servicios { get; set; }
    }
}
