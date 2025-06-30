using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Creamos un constructor de opciones manualmente
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Leemos la cadena de conexión desde el archivo App.config,
            // tal como lo hace tu aplicación al iniciar.
            string connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;

            // Configuramos el DbContext para usar SQL Server con esa cadena de conexión
            optionsBuilder.UseSqlServer(connectionString);

            // Creamos y devolvemos la instancia del AppDbContext
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
