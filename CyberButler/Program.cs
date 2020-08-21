using CyberButler.Commands;
using CyberButler.EntityContext;
using Microsoft.Extensions.DependencyInjection;

namespace CyberButler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create service collection and configure our services
            var services = ConfigureServices();

            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            // Kick off our actual code
            serviceProvider.GetService<CyberButler>().Run().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddDbContext<CyberButlerContext>();

            services.AddTransient<CyberButler>();

            return services;
        }
    }
}