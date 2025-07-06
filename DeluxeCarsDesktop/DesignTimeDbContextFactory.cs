using DeluxeCars.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop
{
    /// <summary>
    /// Esta clase es utilizada únicamente por las herramientas de diseño de Entity Framework (como Add-Migration).
    /// Su propósito es enseñarle a las herramientas cómo crear una instancia del AppDbContext
    /// leyendo la configuración desde el archivo App.config.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1. Leemos la cadena de conexión exactamente como lo hace tu App.xaml.cs
            string connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;

            // 2. Creamos las opciones para el DbContext y le decimos que use SQL Server
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(connectionString);

            // 3. Devolvemos una nueva instancia del DbContext configurada
            return new AppDbContext(builder.Options);
        }
    }
}

