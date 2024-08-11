using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using System;
using OrderService.Repositories;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(s => 
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");

            var cosmosClientOptions = new CosmosClientOptions
            {
                ApplicationRegion = Environment.GetEnvironmentVariable("PrimaryRegion"),
            };

            return new CosmosClient(connectionString, cosmosClientOptions);
        });

        services.AddSingleton<OrderRepository>(sp =>
        {
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var databaseName = Environment.GetEnvironmentVariable("DatabaseName");
            var containerName = Environment.GetEnvironmentVariable("ContainerName");
            return new OrderRepository(cosmosClient, databaseName, containerName);
        });
    })
    .Build();

host.Run();