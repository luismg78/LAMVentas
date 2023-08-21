using LAMInventarios.Configuraciones;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace LAMVentas.Escritorio
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            IConfiguration configuration;
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var configuracion = configuration.GetSection("Configuracion").Get<Configuracion>();

            IServiceCollection services = new ServiceCollection();
            //services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<Configuracion, Configuracion>();
            services.AddScoped<VentasForm>();

            
            Application.Run(new VentasForm(configuracion!));

        }
    }
}