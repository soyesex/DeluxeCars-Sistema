namespace DeluxeCarsEntities
{
    public class Orden
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string DireccionEnvio { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaOrden { get; set; }
        public decimal TotalOrden { get; set; }
        public string Estado { get; set; }


        // Esta es la colección de los detalles de esta orden
        public List<OrdenDetalle> OrdenDetalles { get; set; } = new List<OrdenDetalle>();
    }
}
