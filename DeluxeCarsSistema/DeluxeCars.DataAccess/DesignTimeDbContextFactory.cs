using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DeluxeCars.DataAccess
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1. Construye la configuración para leer el appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                // Establece la ruta base al directorio del proyecto DataAccess
                .SetBasePath(Directory.GetCurrentDirectory())
                // Añade el archivo json como fuente de configuración
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // 2. Lee la cadena de conexión desde el archivo json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
