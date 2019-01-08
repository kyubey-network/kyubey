using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration2(this IServiceCollection self, out IConfiguration config, string fileName = "config")
        {
            var services = self.BuildServiceProvider();
            var env = services.GetRequiredService<IHostingEnvironment>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"{fileName}.json")
                .AddJsonFile($"{fileName}.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            self.AddSingleton<IConfiguration>(configuration);
            config = configuration;

            return self;
        }

        public static IServiceCollection AddConfiguration2(this IServiceCollection self, string fileName = "config")
        {
            var services = self.BuildServiceProvider();
            var env = services.GetRequiredService<IHostingEnvironment>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"{fileName}.json")
                .AddJsonFile($"{fileName}.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            self.AddSingleton<IConfiguration>(configuration);

            return self;
        }
    }
}
