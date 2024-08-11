using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using System;
using InventoryService.Repositories;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(s => 
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
            return new CosmosClient(connectionString);
        });

        services.AddSingleton<InventoryRepository>(sp =>
        {
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var databaseName = Environment.GetEnvironmentVariable("DatabaseName");
            var containerName = Environment.GetEnvironmentVariable("ContainerName");
            return new InventoryRepository(cosmosClient, databaseName, containerName);
        });
    })
    .Build();

host.Run();