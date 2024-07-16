using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddSingleton<VaultService>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Get the VaultService from the DI container
        var vaultService = serviceProvider.GetService<VaultService>();

        // Use the VaultService to get a secret
        // need to fix the possible null reference warning on the next line
        // var secretValue = await vaultService.GetSecretAsync("secret/data/myapp/config")!;

        var secretValue = await vaultService!.GetSecretAsync("secret/data/myapp/config");

        Console.WriteLine($"Secret Value: {secretValue}");
    }
}
